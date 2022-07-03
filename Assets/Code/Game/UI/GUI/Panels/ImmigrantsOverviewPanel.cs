using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class ImmigrantsOverviewPanel : ContentPanel
	{
		[SerializeField] GUIImmigrantGroupListEntry listEntryPrefab;
		[SerializeField] RectTransform listContainer;

        public List<GUIImmigrantGroupListEntry> immigrationEntries = new List<GUIImmigrantGroupListEntry>();
        public Stack<GUIImmigrantGroupListEntry> immigrationPool = new Stack<GUIImmigrantGroupListEntry>();

        public void Setup()
        {
            UpdateData();
        }

		public override void UpdateData()
		{
            RemoveAllItems();
            var system = Main.Instance.GameManager.SystemsManager.NpcSpawnSystem;
            foreach(var group in system.immigrationGroups)
            {
                var entry = GetEntry();
                entry.Setup(group);
                immigrationEntries.Add(entry);
            }
		}

        private void RemoveAllItems()
        {
            for(int i = immigrationEntries.Count - 1; i >= 0; i--)
            {
                RemoveEntry(immigrationEntries[i]);
            }
            immigrationEntries.Clear();
        }

        private void RemoveEntry(GUIImmigrantGroupListEntry entry)
        {
            entry.gameObject.SetActive(false);
            immigrationEntries.Remove(entry);
            immigrationPool.Push(entry);
        }

        private GUIImmigrantGroupListEntry GetEntry()
        {
            GUIImmigrantGroupListEntry entry = null;
            while(entry == null && immigrationPool.Count > 0)
            {
                entry = immigrationPool.Pop();
            }
            if (entry == null)
            {
                entry = Instantiate(listEntryPrefab, listContainer);              
            }
                
            entry.gameObject.SetActive(true);
            return entry;
        }
	}
}