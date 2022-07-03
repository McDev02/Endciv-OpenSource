using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	public class ConstructionSiteInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] GUIResourceInfoEntry resourceListEntry;
		[SerializeField] Transform resoruceListContainer;
		[SerializeField] GUIProgressBar constructionProgress;

		public Text assignedConstructorsState;
		
		ResoruceListHelper resoruceListHelper;

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
            if (!entity.HasFeature<ConstructionFeature>())
            {
                //Call fallback panel (currently inventory)
                Main.Instance.GameManager.GameGUIController.OnShowSelectedEntityInfo(entity);
                OnClose();
                return;
            }
            var feature = entity.GetFeature<ConstructionFeature>();
            if(feature.ConstructionState == ConstructionSystem.EConstructionState.Ready)
			{
				//Call fallback panel (currently inventory)
				Main.Instance.GameManager.GameGUIController.OnShowSelectedEntityInfo(entity);
				OnClose();
				return;
			}
			constructionProgress.Value = feature.ConstructionProgress;

			//Display construction site related info
			assignedConstructorsState.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Construction/Constructors")} {(feature.Constructors.Count == 0 ? "None" : feature.Constructors.Count.ToString())} Suppliers: {(feature.Transporters.Count == 0 ? "None" : feature.Transporters.Count.ToString())}";

			var resources = feature.GetMissingResources().ToArray();
			if (resources == null)
				resources = new ResourceStack[0];

			if (resoruceListHelper == null)
				resoruceListHelper = new ResoruceListHelper(resourceListEntry, resoruceListContainer);
			resoruceListHelper.UpdateResourceList(resources);
		}
	}
}