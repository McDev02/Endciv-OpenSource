
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	internal class ResoruceListHelper
	{
		private List<GUIResourceInfoEntry> entries = new List<GUIResourceInfoEntry>();
		private List<GUIResourceInfoEntry> entryPool = new List<GUIResourceInfoEntry>();

		[SerializeField] GUIResourceInfoEntry resourceListEntry;
		[SerializeField] Transform resoruceListContainer;

		public ResoruceListHelper(GUIResourceInfoEntry resourceListEntry, Transform resoruceListContainer)
		{
			this.resourceListEntry = resourceListEntry;
			this.resoruceListContainer = resoruceListContainer;
		}

		internal void UpdateResourceList(ResourceStack[] resources)
		{
			//Pooling Management
			while (resources.Length > entries.Count)
			{
				if (entryPool.Count > 0)
				{
					var entry = entryPool[0];
					entry.gameObject.SetActive(true);
					entries.Add(entry);
					entryPool.RemoveAt(0);
				}
				else
				{
					entries.Add(GameObject.Instantiate(resourceListEntry, resoruceListContainer));
				}
			}
			while (entries.Count > resources.Length)
			{
				var entry = entries[0];
				entries.RemoveAt(0);
				entry.gameObject.SetActive(false);
				entryPool.Add(entry);
			}

			for (int i = 0; i < resources.Length; i++)
			{
				var tex = Main.Instance.resourceManager.GetIcon(resources[i].ResourceID, EResourceIconType.General,  resourceListEntry.iconSize);
				entries[i].Setup(tex, resources[i].Amount, resources[i].ResourceID);
			}
		}
	}
}
