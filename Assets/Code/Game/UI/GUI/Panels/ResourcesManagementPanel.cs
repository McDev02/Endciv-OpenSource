using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class ResourcesManagementPanel : ContentPanel
	{
		[SerializeField] GUIResourceManagementListEntry listEntryPrefab;
		[SerializeField] RectTransform listContainer;

		List<GUIResourceManagementListEntry> entries;
		ProductionSystem productionSystem;


		public void Setup(ProductionSystem productionSystem)
		{
			this.productionSystem = productionSystem;
			var products = productionSystem.Orders;

			productionSystem.OnFeatureAdded += UpdateProductionList;
			productionSystem.OnFeatureRemoved += UpdateProductionList;

			entries = new List<GUIResourceManagementListEntry>(products.Length);
			for (int i = 0; i < products.Length; i++)
			{
				var newentry = Instantiate(listEntryPrefab, listContainer, false);
				newentry.name = "entry_" + i.ToString();
				newentry.Setup(products[i]);
				entries.Add(newentry);
			}
			UpdateProductionList();
		}
		public override void UpdateData()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].UpdateValues();
			}
		}

		public void UpdateProductionList()
		{
			var recipes = productionSystem.AvailableRecipes;
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].gameObject.SetActive
					(recipes.Contains(entries[i].Order.Entity.StaticData.ID));
			}
		}
	}
}