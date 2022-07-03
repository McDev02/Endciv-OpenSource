using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIResourceManagementListEntry : MonoBehaviour
	{
		public int ID;
		public RecipeFeature Order { get; private set; }
		[SerializeField] Text title;
		[SerializeField] Text amount;
		[SerializeField] Text toProduceAmount;
		[SerializeField] Text info;
		[SerializeField] Slider maxSlider;

		private bool canUpdateSlider = true;


		public void UpdateValues()
		{
			canUpdateSlider = false;
			var itemID = Order.StaticData.OutputResources.ResourceID;
			title.text = itemID;
			int currentAmount = StorageSystem.Statistics.GetItemCount(itemID);
			amount.text = currentAmount.ToString();
			int toProduce = Mathf.Max(0, Order.targetAmount - currentAmount);
			toProduceAmount.text = maxSlider.value.ToString();
			Order.amountInProgress = toProduce;
			maxSlider.value = Order.targetAmount;
			var productionSystem = Main.Instance.GameManager.SystemsManager.ProductionSystem;
			info.text = $"{LocalizationManager.GetText("#UI/Game/Windows/Production/InProduction")} {productionSystem.GetActiveRecipeInProgress(Order)}";
			canUpdateSlider = true;
		}

		public void OnMinValueChanged()
		{

		}

		public void OnMaxValueChanged()
		{
			if (!canUpdateSlider)
				return;
			Order.targetAmount = (int)maxSlider.value;
			UpdateValues();
		}

		internal void Setup(RecipeFeature order)
		{
			this.Order = order;
			UpdateValues();
		}
	}
}