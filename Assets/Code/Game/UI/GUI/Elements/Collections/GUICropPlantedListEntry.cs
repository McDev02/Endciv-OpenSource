using System;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class GUICropPlantedListEntry : MonoBehaviour
	{
		public Text cropTitleLbl;
		public GUIProgressBar2Values growthBar;
		public GUIProgressBar2Values humidityBar;
		public Text fruits;
		public Image fruitIcon;
		public Button yieldButton;

		internal void UpdateValues(CropFeatureStaticData cropData, Data itemData)
		{
			yieldButton.gameObject.SetActive(false);

			var singularorPlural = LocalizationManager.ETextVersion.Plural;// itemData.plantedCrops > 0 ? LocalizationManager.ETextVersion.Plural : LocalizationManager.ETextVersion.Singular;
			var cropTitle = LocalizationManager.GetText(LocalizationManager.CropPath + cropData.entity.ID + "/name", singularorPlural);
			cropTitleLbl.text = $"{itemData.plantedCrops} {cropTitle}";

			growthBar.Value = itemData.progressMin;
			growthBar.Value2 = itemData.progressMax;
			humidityBar.Value = itemData.humidityMin;
			humidityBar.Value2 = itemData.humidityMax;
			fruits.text = itemData.fruits.ToString();
			var icon = ResourceManager.Instance.GetIcon(cropData.entity.ID, EResourceIconType.General);

			var col = Color.white;
			col.a = fruitIcon == null ? 0 : 1;
			fruitIcon.color = col;
			fruitIcon.sprite = icon;
		}

		public struct Data
		{
			public int plantedCrops;
			public float progressMin, progressMax;
			public int fruits;
			public float humidityMin, humidityMax;
		}
	}
}