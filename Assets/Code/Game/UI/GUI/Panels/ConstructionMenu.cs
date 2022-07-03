using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Endciv
{
	public class ConstructionMenu : GUIAnimatedPanel
	{
		RectTransform rect;
		GameGUIController guiController;
		SimpleEntityFactory factory;
		ConstructionSystem constructionSystem;
		UserToolSystem userToolSystem;
		TimeManager timeManager;

		IDictionary<string, Button> CategoryButtons;
		Dictionary<string, List<EntityStaticData>> buildingsPerCategory;
		string currentCategory;
		int idleShowHide;

		[SerializeField] RectTransform[] menuContainingRects;

		[SerializeField] Color normalColor;
		[SerializeField] Color selectedColor;
		[SerializeField] Button categoryPrefab;
		[SerializeField] Transform categoryContainer;
		[SerializeField] GUIConstructionListEntry buildingButtonPrefab;
		[SerializeField] RectTransform buildingWindowContainer;
		[SerializeField] RectTransform buildingHighlightRect;
		[SerializeField] int listWindowPadding = 14;
		[SerializeField] int buildingElementPadding;

		List<GUIConstructionListEntry> pooledBuildingListEntries;
		List<string> categories;

		[SerializeField] GUICanvasGroup buildingDetails;
		[SerializeField] Text buildingDescription;
		[SerializeField] Text buildTimeValue;
		[SerializeField] Text buildingMaxWorkersValue;
		[SerializeField] Text buildingGridSizeValue;
		[SerializeField] GameObject buildTime;
		[SerializeField] GameObject buildingMaxWorkers;
		[SerializeField] GameObject buildingGridSize;

		[SerializeField] Sprite missingCategoryIcon;
		[SerializeField] CategoryIcons[] categoryIcons;

		[SerializeField] int minWidth;
		bool closeWindowNextFrame;
		GameInputManager inputManager;

		[Serializable]
		public struct CategoryIcons
		{
			public string categoryID;
			public Sprite icon;
		}

		public void Setup(GameManager gameManager)
		{
			factory = gameManager.Factories.SimpleEntityFactory;
			userToolSystem = gameManager.UserToolSystem;
			constructionSystem = gameManager.SystemsManager.ConstructionSystem;
			timeManager = gameManager.timeManager;
			rect = (RectTransform)transform;
			inputManager = gameManager.gameInputManager;

			userToolSystem.OnToolChanged -= OnUserToolChanged;
			userToolSystem.OnToolChanged += OnUserToolChanged;
		}

		public void Run()
		{
			var structures = factory.GetStaticDataIDList<StructureFeatureStaticData>();
			categories = new List<string>();

			constructionSystem.OnTechChanged -= UpdateConstructionCategories;
			constructionSystem.OnTechChanged += UpdateConstructionCategories;

			pooledBuildingListEntries = new List<GUIConstructionListEntry>();
			CategoryButtons = new Dictionary<string, Button>();
			buildingsPerCategory = new Dictionary<string, List<EntityStaticData>>();
			//Setup categories and building data
			foreach (var item in structures)
			{
				var entity = factory.GetStaticData<StructureFeatureStaticData>(item).entity;
				var building = entity.GetFeature<ConstructionStaticData>();
				if (building == null)
					continue;
				var cat = building.Category;
				if (!building.ShowInConstructionMenu || string.IsNullOrEmpty(cat) || cat.ToLower() == "hide") continue;
				if (!entity.HasFeature(typeof(GridObjectFeatureStaticData)))
					continue;
				if (!categories.Contains(cat))
					categories.Add(cat);

				//Add building data to list per Category.
				if (buildingsPerCategory.ContainsKey(cat))
					buildingsPerCategory[cat].Add(entity);
				else
					buildingsPerCategory.Add(cat, new List<EntityStaticData>() { entity });
			}

			//Generate building category Buttons
			for (int i = 0; i < categories.Count; i++)
			{
				string cat = categories[i];

				var button = Instantiate(categoryPrefab, categoryContainer, false);
				button.name = cat;
				var icon = button.GetComponentInChildren<Image>();
				icon.sprite = GetIcon(cat);
				button.onClick.AddListener(delegate { SelectCategory(cat); });
				CategoryButtons.Add(cat, button);
			}

			currentCategory = null;
			UpdateConstructionCategories();
		}

		internal void ShowDetails(int id)
		{
			var rect = (RectTransform)buildingDetails.transform;
			var entry = pooledBuildingListEntries[id];
			var pos = rect.position;
			pos.x = ((RectTransform)entry.transform).position.x;
			rect.position = pos;
			idleShowHide = 1;

			var c = entry.data.GetFeature<ConstructionStaticData>();

			buildingDescription.text = LocalizationManager.GetText(LocalizationManager.StructurePath + entry.data.ID + "/description");

			if (c.MaxConstructionPoints > 0)
			{
				var ticks = c.MaxConstructionPoints / (GameConfig.Instance.GeneralEconomyValues.ConstructionSpeed * c.MaxWorkers);
				buildTimeValue.text = CivHelper.GetTimeStringDoubledot(timeManager.MinutesPerTick * ticks);
				buildingMaxWorkersValue.text = c.MaxWorkers.ToString();
				buildTime.gameObject.SetActive(true);
				buildingMaxWorkers.gameObject.SetActive(true);
			}
			else
			{
				buildTime.gameObject.SetActive(false);
				buildingMaxWorkers.gameObject.SetActive(false);
			}
			var gridData = entry.data.GetFeature<GridObjectFeatureStaticData>();
			if (gridData == null)
				buildingGridSizeValue.text = "Error";
			else if (gridData.GridIsFlexible)
				buildingGridSizeValue.text = "Free area";
			else
			{
				Vector2i size = new Vector2i((int)((gridData.SizeX + 1) / 2f), (int)((gridData.SizeY + 1) / 2f));
				buildingGridSizeValue.text = $"{size.X}x{size.Y}";
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
		}

		internal void HideDetails(int id)
		{
			if (idleShowHide != 1)
				idleShowHide = -1;
		}

		public void UpdateConstructionCategories()
		{
			for (int i = 0; i < categories.Count; i++)
			{
				bool showCategory = false;
				var buildings = buildingsPerCategory[categories[i]];
				for (int j = 0; j < buildings.Count; j++)
				{
					if (constructionSystem.CanBuild(buildings[j].GetFeature<StructureFeatureStaticData>()))
					{
						showCategory = true;
						break;
					}
				}
				CategoryButtons[categories[i]].gameObject.SetActive(showCategory);
			}
			//Update layout
			var fitter = categoryContainer.GetComponent<ContentSizeFitter>();
			var layout = categoryContainer.GetComponent<HorizontalLayoutGroup>();
			if (layout != null) layout.enabled = true;
			if (fitter != null) fitter.enabled = true;
			buildingHighlightRect.parent = null;
			Canvas.ForceUpdateCanvases();
			if (fitter != null) fitter.enabled = false;
			if (layout != null) layout.enabled = false;
			buildingHighlightRect.parent = categoryContainer;
			buildingHighlightRect.SetAsFirstSibling();

			SelectCategory(currentCategory);
		}
		public override void OnClose()
		{
			base.OnClose();
			buildingDetails.OnClose();
		}
		public override void OnHide()
		{
			base.OnHide();
			buildingDetails.OnClose();
		}

		private void LateUpdate()
		{
			if (!IsActive) return;

			if (idleShowHide == 1)
				buildingDetails.OnOpen();
			else if (idleShowHide != 0)
				buildingDetails.OnClose();

			idleShowHide = 0;

			if (closeWindowNextFrame)
			{
				closeWindowNextFrame = false;
				SelectCategory(null);
			}

			if (Input.GetMouseButtonDown(0) && !IsMouseInRect())
				closeWindowNextFrame = true;
		}

		bool IsMouseInRect()
		{
			var mousePos = Input.mousePosition;
			for (int i = 0; i < menuContainingRects.Length; i++)
			{
				if (RectTransformUtility.RectangleContainsScreenPoint(menuContainingRects[i], mousePos))
					return true;
			}
			return false;
		}

		/// <summary>
		/// When category is null or invalid nothing is being selected
		/// </summary>
		public void SelectCategory(string category)
		{
			closeWindowNextFrame = false;
			//Debug.Log($"SelectCategory( { category })");
			if (!string.IsNullOrEmpty(currentCategory) && CategoryButtons.ContainsKey(currentCategory))
				CategoryButtons[currentCategory].image.color = normalColor;

			if (category != null && categories.Contains(category))
				currentCategory = category;
			else
				currentCategory = null;

			if (!string.IsNullOrEmpty(category) && CategoryButtons.ContainsKey(category))
			{
				var btn = CategoryButtons[category];
				var pos = buildingHighlightRect.localPosition;
				pos.x = btn.transform.localPosition.x;
				buildingHighlightRect.localPosition = pos;
				buildingHighlightRect.gameObject.SetActive(true);
				btn.image.color = selectedColor;
			}
			else
				buildingHighlightRect.gameObject.SetActive(false);

			UpdateConstructionWindow(currentCategory);
		}

		void UpdateConstructionWindow(string category)
		{
			if (string.IsNullOrEmpty(category) || !buildingsPerCategory.ContainsKey(category))
			{
				OnClose();
				return;
			}
			GUIConstructionListEntry btn;
			var data = buildingsPerCategory[category];
			var size = rect.sizeDelta;
			size.x = 2 * listWindowPadding - buildingElementPadding;
			int poolID = -1;
			for (int i = 0; i < data.Count; i++)
			{
				var building = data[i];

				if (!constructionSystem.CanBuild(building.GetFeature<StructureFeatureStaticData>()))
					continue;
				poolID++;
				if (pooledBuildingListEntries.Count > poolID)
					btn = pooledBuildingListEntries[poolID];
				else
				{
					btn = Instantiate(buildingButtonPrefab, buildingWindowContainer, false);
					pooledBuildingListEntries.Add(btn);
				}
#if UNITY_EDITOR
				btn.name = building.Name;
#endif
				var sprite = ResourceManager.Instance.GetIcon(building.ID, EResourceIconType.Building);
				btn.Setup(this, poolID, building, sprite);

				var sid = building.ID;
				btn.button.onClick.AddListener(delegate { OnStructureButtonClicked(sid); });
				btn.gameObject.SetActive(true);
				size.x += ((RectTransform)btn.transform).rect.width + buildingElementPadding;
			}
			for (int j = poolID + 1; j < pooledBuildingListEntries.Count; j++)
			{
				pooledBuildingListEntries[j].gameObject.SetActive(false);
			}
			//size.x = buildingWindowContainer.rect.width + 2 * listWindowPadding;
			rect.sizeDelta = size;

			OnOpen();
		}

		Sprite GetIcon(string catID)
		{
			for (int i = 0; i < categoryIcons.Length; i++)
			{
				if (categoryIcons[i].categoryID == catID)
					return categoryIcons[i].icon;
			}
			return missingCategoryIcon;
		}

		public void OnStructureButtonClicked(string sid)
		{
			userToolSystem.PlaceStructure(sid);
		}

		void OnUserToolChanged()
		{
			SelectCategory(null);
		}
	}
}