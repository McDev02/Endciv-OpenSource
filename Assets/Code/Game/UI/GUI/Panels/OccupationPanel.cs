using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System;

namespace Endciv
{
	public class OccupationPanel : ContentPanel
	{
		private CitizenAISystem aiSystem;
		[SerializeField] GUIOccupationListEntry listEntryPrefab;
		[SerializeField] RectTransform listContainer;

		List<GUIOccupationListEntry> entries;
		ConstructionSystem constructionSystem;

		internal void IncreaseValue(EOccupation id)
		{
			aiSystem.IncreaseWantedOccupation(id);
		}

		internal void DecreaseValue(EOccupation id)
		{
			aiSystem.DecreaseWantedOccupation(id);
		}

		public void Setup(CitizenAISystem aiSystem,ConstructionSystem constructionSystem )
		{
			this.aiSystem = aiSystem;
			int count = (int)EOccupation.COUNT;
			entries = new List<GUIOccupationListEntry>(count);

			for (int i = 0; i < count; i++)
			{
				var newentry = Instantiate(listEntryPrefab, listContainer, false);
				newentry.name = "entry_" + i.ToString();
				newentry.Setup(aiSystem, aiSystem.OccupationSettings[i], i > (int)EOccupation.Labour);
				entries.Add(newentry);
			}

			aiSystem.OnOccupationUpdated += UpdateData;
			UpdateData();

			this.constructionSystem = constructionSystem;
			constructionSystem.OnTechChanged -= UpdateTech;
			constructionSystem.OnTechChanged += UpdateTech;
		}

		public void UpdateTech()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				bool active = true;
				var type = (EOccupation)i;
				switch (type)
				{
					case EOccupation.Construction:
					case EOccupation.Scavenger:
					case EOccupation.Supply:
						active = constructionSystem.HasTech(ETechnologyType.Storage);
						break;
					case EOccupation.Production:
						active = constructionSystem.HasTech(ETechnologyType.Production);
						break;
					case EOccupation.Farmer:
						active = constructionSystem.HasTech(ETechnologyType.Crops);
						break;
					case EOccupation.Herder:
						active= constructionSystem.HasTech(ETechnologyType.Livestock);
						break;
				}

				entries[i].Interactable = active;
			}
		}

		public override void UpdateData()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].UpdateValues();
			}
		}
	}
}