using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class ProductionFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] Text infoText;

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			if (!entity.HasFeature<ProductionFeature>())
			{
				OnClose();
				return;
			}
			var production = entity.GetFeature<ProductionFeature>();
			if (production.ProductionLines == null || production.ProductionLines.Length <= 0)
				return;
			if (production.ProductionLines[0] == null)
			{
				infoText.text = "Currently no production";
			}
			else
			{
				infoText.text = $"Production: {production.ProductionLines[0].targetAmount}x {LocalizationManager.GetResourceName(production.ProductionLines[0].StaticData.OutputResources.ResourceID)}";
			}
		}

		public void OpenProductionWindow()
		{
		}
	}
}