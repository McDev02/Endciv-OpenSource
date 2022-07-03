using System;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class GUICropListEntry : MonoBehaviour
	{
		public Toggle toggle;
		public Text cropTitle;

		public Text growTime;
		public Text waterNeed;
		public Text maxYield;


		internal void UpdateValues(string cropID, int seedAmount)
		{
            var cropData = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<CropFeatureStaticData>(cropID);
			//Hack to remote "crop_" prefix from name because no duplicate Entity IDs are allowed"
			var cropTitle = LocalizationManager.GetText(LocalizationManager.CropPath + cropData.entity.ID.Remove(0,5) + "/name");
			var textVersion = seedAmount == 1 ? LocalizationManager.ETextVersion.Singular : LocalizationManager.ETextVersion.Plural;
			this.cropTitle.text = $"{cropTitle}: {seedAmount} {LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/seed", textVersion)}";
			toggle.interactable = seedAmount > 0;

			var waterPerDay = cropData.waterConsumption.ToString("0.#");
			growTime.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/growTime")}: {seedAmount} {LocalizationManager.GetText("#General/Inline/day", textVersion)}";
			growTime.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/waterPerDay")}: {waterPerDay} L";
			growTime.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Farmland/maxYield")}: {cropData.fruitAmount.Average}";
		}
	}
}