using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class ImmigrantGroupInfoPanel : BaseFeatureInfoPanel
	{
		private NpcSpawnSystem npcSpawnSystem;
		[SerializeField] GameGUIController gameGUIController;
		ImmigrantAIAgentFeature feature;
		[SerializeField] Button AcceptButton;
		[SerializeField] Button DenyButton;

		public override void Setup(GameGUIController controller, BaseEntity entity)
		{
			if (entity == null)
				return;
			feature = entity.GetFeature<ImmigrantAIAgentFeature>();
			npcSpawnSystem = Main.Instance.GameManager.SystemsManager.NpcSpawnSystem;
			base.Setup(controller, entity);									
		}

		public override void UpdateData()
		{
			base.UpdateData();
			AcceptButton.interactable = false;
			DenyButton.interactable = false;
			if (entity == null)
				return;
			if (!entity.HasFeature<ImmigrantAIAgentFeature>())
				return;
			feature = entity.GetFeature<ImmigrantAIAgentFeature>();
			if (!npcSpawnSystem.immigrantGroupReference.ContainsKey(feature))
				return;
			bool everyoneArrived = true;
			var group = npcSpawnSystem.immigrantGroupReference[feature];
			foreach(var immigrant in group.immigrants)
			{
				if (immigrant == null)
					continue;
				if(immigrant.State != EImmigrantState.Waiting)
				{
					everyoneArrived = false;
					break;
				}
			} 
			AcceptButton.interactable = everyoneArrived;
			DenyButton.interactable = everyoneArrived;
		}

		public void Accept()
		{
			npcSpawnSystem.ConvertImmigrantsToCitizens(npcSpawnSystem.immigrantGroupReference[feature]);
			OnClose();
		}

		public void Deny()
		{
			npcSpawnSystem.DenyImmigrationGroup(npcSpawnSystem.immigrantGroupReference[feature]);
			OnClose();
		}
	}
}