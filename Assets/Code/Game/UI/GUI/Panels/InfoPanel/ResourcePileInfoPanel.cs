using UnityEngine.UI;

namespace Endciv
{
	public class ResourcePileInfoPanel : BaseFeatureInfoPanel
	{
		public Text assignedGathererState;
		public Text FullResourcesState;
		public Text resourcesState;

		public override void UpdateData()
		{
			base.UpdateData();
            if (entity == null)
                return;			
			if (!entity.HasFeature<ResourcePileFeature>())
            {
                OnClose();
                return;
            }
            var pile = entity.GetFeature<ResourcePileFeature>();
            assignedGathererState.text = pile.assignedCollector == null ? "None" : pile.assignedCollector.Entity.GetFeature<EntityFeature>().View.name;
			FullResourcesState.text = pile.fullResources.ToString();
			resourcesState.text = pile.resources.Count.ToString();
		}
	}
}