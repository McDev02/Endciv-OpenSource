using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class FarmlandFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] GUICropListEntry cropListEntry;
		[SerializeField] GUIToggleGroup cropListContainer;
		List<GUICropListEntry> cropListEntries = new List<GUICropListEntry>();

		[SerializeField] GUIProgressBar slotsProgress;
		[SerializeField] Text slotsValues;

		[SerializeField] Text plantCropsInfoText;
		[SerializeField] Slider amountSlider;
		[SerializeField] Button plantCropButton;
		[SerializeField] Text plantCropButtonText;
		[SerializeField] ScrollRect plantScrollview;

		[SerializeField] ScrollRect plantedCropsScrollview;
		[SerializeField] GUICropPlantedListEntry cropPlantedListEntry;
		[SerializeField] Transform cropPlantedListContainer;
		List<GUICropPlantedListEntry> cropPlantedListEntries = new List<GUICropPlantedListEntry>();

		[SerializeField] GameObject cropOverviewWindow;
		[SerializeField] GameObject plantCropsWindow;

		FarmlandFeature feature;
		bool islinked;
		string activeCrop;

		public override void Setup(GameGUIController controller, BaseEntity entity)
		{
			if (entity == null) return;
			feature = entity.GetFeature<FarmlandFeature>();

			if (!islinked)
				cropListContainer.OnChange += OnCropSelectionChanged;
			islinked = true;

			if (feature == null)
				return;

			SetupPlantCropsList();
			SetupPlantedCropsList();

			cropListContainer.Setup();

			base.Setup(controller, entity);
			ChangeWindowView(0);
		}

		public void ChangeWindowView(int id)
		{
			cropOverviewWindow.SetActive(id == 0);
			plantCropsWindow.SetActive(id == 1);
		}

		void SetupPlantCropsList()
		{
			int totalEntries = cropListContainer.transform.childCount;
			int count = Mathf.Max(feature.StaticData.CropIDs.Length, totalEntries);
			cropListEntries.Clear();
			for (int i = 0; i < totalEntries; i++)
			{
				var obj = cropListContainer.transform.GetChild(i);
				obj.gameObject.SetActive(false);
			}
			for (int i = 0; i < count; i++)
			{
				if (i >= feature.StaticData.CropIDs.Length) break;
				GUICropListEntry entry;
				if (i >= totalEntries)
					entry = Instantiate(cropListEntry, cropListContainer.transform);
				else entry = cropListContainer.transform.GetChild(i).GetComponent<GUICropListEntry>();

				entry.toggle.group = cropListContainer;
				entry.toggle.isOn = false;
				entry.gameObject.SetActive(true);
				cropListEntries.Add(entry);
			}
			plantScrollview.verticalScrollbar.value = 1;
		}

		void SetupPlantedCropsList()
		{
			int totalToggles = cropPlantedListContainer.transform.childCount;
			int count = Mathf.Max(feature.CropGroups.Count, totalToggles);
			cropPlantedListEntries.Clear();
			for (int i = 0; i < totalToggles; i++)
			{
				var obj = cropPlantedListContainer.transform.GetChild(i);
				obj.gameObject.SetActive(false);
			}
			for (int i = 0; i < count; i++)
			{
				if (i >= feature.StaticData.CropIDs.Length) break;
				GUICropPlantedListEntry entry;
				if (i >= totalToggles)
					entry = Instantiate(cropPlantedListEntry, cropPlantedListContainer);
				else entry = cropPlantedListContainer.transform.GetChild(i).GetComponent<GUICropPlantedListEntry>();

				entry.gameObject.SetActive(true);
				cropPlantedListEntries.Add(entry);
			}
			plantedCropsScrollview.verticalScrollbar.value = 1;
		}
		public void OnPlantCrops()
		{
			if (feature == null)
				return;
			ValidateData();

			if (!string.IsNullOrEmpty(activeCrop))
				AgricultureSystem.PlantCrops(feature, activeCrop, (int)amountSlider.value);
			SetupPlantedCropsList();
			ChangeWindowView(0);
		}

		public void OnCropSelectionChanged(int id)
		{
			ValidateData();
		}

		public void OnSliderChangeValue()
		{
			UpdateData();
		}

		void ValidateData()
		{
			if (feature == null)
				return;

			activeCrop = null;
			int seeds = 0;
			for (int i = 0; i < cropListEntries.Count; i++)
			{
				var cropData = feature.StaticData.CropIDs[i];
				cropListEntries[i].UpdateValues(cropData, feature.System.GetSeeds(cropData));
				if (cropListEntries[i].toggle.isOn)
				{
					seeds = feature.System.GetSeeds(cropData);
					activeCrop = cropData;
				}
			}
			int canplant = Mathf.Min(feature.SpaceLeft(), seeds);

			amountSlider.minValue = 0;
			amountSlider.maxValue = canplant;
			amountSlider.minValue = canplant > 0 ? 1 : 0;

			plantCropButton.interactable = amountSlider.value > 0;
			//plantCropButtonText.text=
		}

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			if (feature == null)
			{
				OnClose();
				return;
			}

			ValidateData();
			//InfoData
			var spaceLeft = feature.SpaceLeft();
			slotsProgress.Value = (spaceLeft / (float)feature.MaxSpace);
			slotsValues.text = $"{spaceLeft} / {feature.MaxSpace}";


			if (feature.CropGroups.Count < cropPlantedListEntries.Count)
			{
				for (int i = cropPlantedListEntries.Count - 1; i >= feature.CropGroups.Count; i--)
				{
					cropPlantedListEntries[i].gameObject.SetActive(false);
					cropPlantedListEntries.RemoveAt(i);
				}
			}
			if (feature.CropGroups.Count > cropPlantedListEntries.Count)
			{
				int totalToggles = cropPlantedListContainer.transform.childCount;
				for (int i = cropPlantedListEntries.Count; i < feature.CropGroups.Count; i++)
				{
					GUICropPlantedListEntry entry = null;
					if (i < totalToggles)
					{
						entry = cropPlantedListContainer.transform.GetChild(i).GetComponent<GUICropPlantedListEntry>();
						entry.gameObject.SetActive(true);
					}
					else
					{
						entry = Instantiate(cropPlantedListEntry, cropPlantedListContainer);
					}
					cropPlantedListEntries.Add(entry);
				}
			}
			int count = Mathf.Min(cropPlantedListEntries.Count, feature.CropGroups.Count);
			for (int i = 0; i < count; i++)
			{
				var group = feature.CropGroups[i];
				GUICropPlantedListEntry.Data data = new GUICropPlantedListEntry.Data();
				data.progressMin = 1;
				data.progressMax = 0;
				data.humidityMin = 1;
				data.humidityMax = 0;
				data.plantedCrops = group.Count;

				for (int p = 0; p < group.Count; p++)
				{
					var crop = group[p];
					data.fruits += crop.Fruits;
					if (crop.Progress <= data.progressMin)
						data.progressMin = crop.Progress;
					if (crop.Progress >= data.progressMax)
						data.progressMax = crop.Progress;

					if (crop.humidity.Value <= data.humidityMin)
						data.humidityMin = crop.humidity.Value;
					if (crop.humidity.Value >= data.humidityMax)
						data.humidityMax = crop.humidity.Value;
				}
				cropPlantedListEntries[i].UpdateValues(group[0].staticData, data);
			}

			if (activeCrop == null)
			{
				plantCropButton.interactable = false;
				plantCropButton.gameObject.SetActive(false);
				amountSlider.gameObject.SetActive(false);
				plantCropsInfoText.text = LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/selectcroptoplant");
			}
			else
			{
				plantCropsInfoText.text = LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/howmany");
				plantCropButton.gameObject.SetActive(true);
				plantCropButton.interactable = true;
				amountSlider.gameObject.SetActive(true);
				var plantText = LocalizationManager.GetText($"#Resources/{activeCrop}/name");
				var val = (int)amountSlider.value;

				var plantingText = LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/planting");
				plantCropButtonText.text = $"{plantingText} {val} {plantText}";
			}
		}
	}
}