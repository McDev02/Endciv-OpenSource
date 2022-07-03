using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class InventoryFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] GUIResourceInfoEntry resourceListEntry;
		[SerializeField] Transform resoruceListContainer;
		ResoruceListHelper resoruceListHelper;

		[SerializeField] GUIProgressBar progress;
		public Text capacity;
		//public Text totalChambers;


		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			var inventory = entity.Inventory;
			if (inventory == null)
			{
				OnClose();
				return;
			}

			progress.Value = inventory.LoadProgress;
			float factor = 1f / 10f;
			capacity.text = $"{(inventory.Load * factor).ToString()} / {(inventory.MaxCapacity * factor).ToString()}";
			//totalChambers.text = inventory.TotalChambers.ToString();

			var resources = InventorySystem.GetChamberContentList(inventory, InventorySystem.ChamberMainID);
			if (resources == null)
				resources = new ResourceStack[0];

			if (resoruceListHelper == null)
				resoruceListHelper = new ResoruceListHelper(resourceListEntry, resoruceListContainer);
			resoruceListHelper.UpdateResourceList(resources);
		}
	}
}