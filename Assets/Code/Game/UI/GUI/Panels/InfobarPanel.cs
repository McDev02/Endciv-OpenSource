using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	public class InfobarPanel : ContentPanel
	{
		private class PanelSettings
		{
			public string iconID;
			public EInfoEntryType entryType;

			public PanelSettings(string iconID, EInfoEntryType entryType)
			{
				this.iconID = iconID;
				this.entryType = entryType;
			}
		}

		[SerializeField] Text lbl_Residents;
		[SerializeField] Text lbl_HomeSpace;
		[SerializeField] Text lbl_Electricity;

		[SerializeField] Text lbl_Mood;

		[SerializeField] Text lbl_Boards;
		[SerializeField] Text lbl_Scrap;
		[SerializeField] Text lbl_Tarp;
		[SerializeField] Text lbl_Food;
		[SerializeField] Text lbl_Water;

		[SerializeField] GUICanvasGroup bannerInfoPanel;
		[SerializeField] LocalizedText bannerInfoPanelText;
		[SerializeField] int bannerInfoWidth;
		[SerializeField] int bannerInfoPadding;

		[SerializeField] GUICanvasGroup panel;
		[SerializeField] Transform townInfoCollection;
		[SerializeField] GUITownInfoListEntry townInfoPrefab;
		[SerializeField] CitizenMoodController moodController;
		[SerializeField] Color UrgencyColor;
		[SerializeField] Color InfoColor;
		[SerializeField] Color ActivityColor;

		private InfobarSystem InfobarSystem { get; set; }
		private Dictionary<EInfobarCategory, GUITownInfoListEntry> panels;
		GUITownInfoListEntry currentlyHoveredInfoPanel;
		bool currentInfoPanelChanged;

		GameInputManager inputManager;

		protected override void Awake()
		{
			base.Awake();
			var rect = (RectTransform)bannerInfoPanel.transform;
			bannerInfoWidth = (int)rect.sizeDelta.x;

			inputManager = Main.Instance.gameInputManager;
		}

		internal void ShowInfo(GUITownInfoListEntry panel)
		{
			currentInfoPanelChanged = currentlyHoveredInfoPanel != panel;
			currentlyHoveredInfoPanel = panel;
		}

		internal void HideInfo(GUITownInfoListEntry panel)
		{
			if (currentlyHoveredInfoPanel == panel)
				currentlyHoveredInfoPanel = null;
		}

		private Stack<GUITownInfoListEntry> panelPool;
		private Dictionary<EInfobarCategory, PanelSettings> panelSettings =
			new Dictionary<EInfobarCategory, PanelSettings>
			{
				{ EInfobarCategory.DeadUnits, new PanelSettings(UI3DFactory.IconDeath, EInfoEntryType.Urgency) },
				{ EInfobarCategory.HungryUnits, new PanelSettings(UI3DFactory.IconHunger, EInfoEntryType.Urgency) },
				{ EInfobarCategory.ThirstyUnits, new PanelSettings(UI3DFactory.IconThirst, EInfoEntryType.Urgency) },
				{ EInfobarCategory.TroubledCattle, new PanelSettings(UI3DFactory.IconCattle, EInfoEntryType.Urgency) },
				{ EInfobarCategory.HomelessUnits, new PanelSettings(UI3DFactory.IconHomeless, EInfoEntryType.Urgency) },
				{ EInfobarCategory.ImmigrantUnits, new PanelSettings(UI3DFactory.IconImmigrant, EInfoEntryType.Info) },
				{ EInfobarCategory.TraderUnits, new PanelSettings(UI3DFactory.IconTrader, EInfoEntryType.Info) },
				{ EInfobarCategory.Exploration, new PanelSettings(UI3DFactory.IconExpedition, EInfoEntryType.Activity) }
			};

		public override void Run()
		{
			base.Run();
			InfobarSystem = Main.Instance.GameManager.SystemsManager.InfobarSystem;
			panels = new Dictionary<EInfobarCategory, GUITownInfoListEntry>();
			panelPool = new Stack<GUITownInfoListEntry>();
			InfobarSystem.OnEntitiesUpdated -= OnEntitiesUpdated;
			InfobarSystem.OnEntitiesUpdated += OnEntitiesUpdated;
			for (int i = 0; i < Enum.GetNames(typeof(EInfobarCategory)).Length; i++)
			{
				OnEntitiesUpdated((EInfobarCategory)i, null);
			}
		}

		private void LateUpdate()
		{
			if (currentInfoPanelChanged)
			{
				bool showOrHideInfoPanel = currentlyHoveredInfoPanel != null;
				if (showOrHideInfoPanel)
				{
					bannerInfoPanel.OnOpen();
					string locaKey = "#UI/Game/Tooltip/InfoBanners/" + currentlyHoveredInfoPanel.category.ToString();
					bannerInfoPanelText.SetLocaKey(locaKey);

					var settings = bannerInfoPanelText.GetGenerationSettings(new Vector2((bannerInfoWidth - 2 * bannerInfoPadding), 0));
					settings.horizontalOverflow = HorizontalWrapMode.Wrap;
					settings.verticalOverflow = VerticalWrapMode.Overflow;
					settings.updateBounds = true;

					bannerInfoPanelText.cachedTextGenerator.Populate(bannerInfoPanelText.text, settings);
					var size = bannerInfoPanelText.cachedTextGenerator.rectExtents.size;
					var rect = (RectTransform)bannerInfoPanel.transform;

					//This doesn't really work well
					size *= inputManager.UIScaleInv;
					size.y = size.y * 1.1f + 112 + bannerInfoPadding * 2;
					size.x += 2 * bannerInfoPadding;

					size.x = CivMath.GetNextMultipleOf(size.x, 4);
					size.y = CivMath.GetNextMultipleOf(size.y, 4);

					rect.sizeDelta = size;
				}
				else
				{
					bannerInfoPanel.OnClose();
				}
			}
		}

		private void OnEntitiesUpdated(EInfobarCategory category, BaseEntity entity)
		{
			var entities = InfobarSystem.Entities[category];
			if (entities.Count <= 0)
			{
				if (panels.ContainsKey(category))
				{
					var panel = panels[category];
					panels.Remove(category);
					RecyclePanel(panel);
				}
				return;
			}
			if (!panels.ContainsKey(category))
			{
				var panel = GetPanel();
				var settings = panelSettings[category];
				SetupPanel(panel, settings.iconID, category, settings.entryType);
				panels.Add(category, panel);
			}
			panels[category].UpdateEntities(InfobarSystem.Entities[category]);
		}

		private GUITownInfoListEntry GetPanel()
		{
			GUITownInfoListEntry entry = null;
			if (panelPool.Count > 0)
			{
				entry = panelPool.Pop();
			}
			else
			{
				entry = Instantiate(townInfoPrefab, townInfoCollection);
			}
			entry.gameObject.SetActive(true);
			return entry;
		}

		private void RecyclePanel(GUITownInfoListEntry entry)
		{
			entry.Reset();
			entry.gameObject.SetActive(false);
			panelPool.Push(entry);
		}

		private void SetupPanel(GUITownInfoListEntry entry, string iconID, EInfobarCategory category, EInfoEntryType entryType)
		{
			entry.category = category;
			entry.controller = this;
			entry.icon.overrideSprite = ResourceManager.Instance.GetIcon(iconID, EResourceIconType.Notification);
			switch (entryType)
			{
				case EInfoEntryType.Info:
					entry.background.color = InfoColor;
					break;

				case EInfoEntryType.Urgency:
					entry.background.color = UrgencyColor;
					break;

				case EInfoEntryType.Activity:
					entry.background.color = ActivityColor;
					break;
			}
		}

		public override void UpdateData()
		{
			moodController.UpdateData();

			var townStats = GameStatistics.MainTownStatistics;
			lbl_Residents.text = townStats.TotalPeople.ToString();
			lbl_HomeSpace.text = (townStats.TotalHomeSpace - townStats.TotalPeople).ToString();
			lbl_Electricity.text = $"{townStats.TotalElectricityBalance.ToString("0")}   <size=30>Storage:</size> {townStats.TotalElectricityStored.ToString("0")}/{townStats.TotalElectricityCapacity.ToString("0")}";

			var invStats = GameStatistics.InventoryStatistics;
			lbl_Boards.text = invStats.Items[FactoryConstants.BoardsID].ToString();
			lbl_Scrap.text = invStats.Items[FactoryConstants.ScrapID].ToString();
			lbl_Tarp.text = invStats.Items[FactoryConstants.TarpID].ToString();

			lbl_Food.text = invStats.Nutrition.ToString("0");
			string waterString = CivHelper.GetWaterString(invStats.Items[FactoryConstants.WaterID]);
			lbl_Water.text = waterString;// invStats.Water.ToString("0");

			var ts = GameStatistics.MainTownStatistics;
			lbl_Mood.text = Mathf.RoundToInt(ts.averageNeedMood * 100).ToString();
		}

	}
}