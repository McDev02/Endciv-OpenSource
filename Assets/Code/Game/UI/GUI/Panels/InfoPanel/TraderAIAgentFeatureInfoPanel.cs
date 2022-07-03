using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TraderAIAgentFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] GameGUIController gameGUIController;
		TraderAIAgentFeature feature;
		[SerializeField] Button TradeButton;
		[SerializeField] Text remainingTime;

		public override void Setup(GameGUIController controller, BaseEntity entity)
		{
			base.Setup(controller, entity);
			if (entity == null) return;
			feature = entity.GetFeature<TraderAIAgentFeature>();
		}
		public void OpenTraderWindow()
		{
			DeselectEntity();
			gameGUIController.ShowTradingWindow(feature);
		}
		public override void UpdateData()
		{
			base.UpdateData();

			if (entity == null) return;
			if (!entity.HasFeature<TraderAIAgentFeature>())
				return;
			feature = entity.GetFeature<TraderAIAgentFeature>();
			switch (feature.state)
			{
				case NpcSpawnSystem.ETraderState.Arrival:
					remainingTime.text = LocalizationManager.GetText("#UI/Game/InfoPanels/Trader/OnTheWay");
					TradeButton.interactable = false;
					break;
				case NpcSpawnSystem.ETraderState.Waiting:
					remainingTime.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Trader/TimeRemaining")} {CivHelper.GetTimeStringDoubledot(feature.waitCounter) }";
					TradeButton.interactable = true;
					break;
				case NpcSpawnSystem.ETraderState.Leaving:
					remainingTime.text = LocalizationManager.GetText("#UI/Game/InfoPanels/Trader/Leaving");
					TradeButton.interactable = false;
					break;
				case NpcSpawnSystem.ETraderState.Left:
					remainingTime.text = LocalizationManager.GetText("#UI/Game/InfoPanels/Trader/Leaving");
					TradeButton.interactable = false;
					break;
			}
		}
	}
}