using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

namespace Endciv
{
	public class CitizenOverviewPanel : ContentPanel
	{
		[SerializeField] GUICitizenOverviewListEntry listEntryPrefab;
		[SerializeField] RectTransform listContainer;

		List<GUICitizenOverviewListEntry> entries;
		UnitSystem unitSystem;
		CameraController cameraController;
		UserTool_Selection selectionTool;

		public void Setup(UnitSystem unitSystem, CameraController cameraController, UserTool_Selection selectionTool)
		{
			this.selectionTool = selectionTool;
			this.cameraController = cameraController;
			this.unitSystem = unitSystem;
			entries = new List<GUICitizenOverviewListEntry>(32);
			unitSystem.OnCitizenAdded += RecalculateList;
			unitSystem.OnCitizenRemoved += RecalculateList;

			var count = listContainer.childCount;
			for (int i = 0; i < count; i++)
			{
				var child = listContainer.GetChild(i);
				var entry = child.GetComponent<GUICitizenOverviewListEntry>();
				if (entry != null)
					entries.Add(entry);
				else
					Debug.LogError("Citizen Overview child was not of hte propper type! Delete it.");
			}
			RecalculateList();
		}

		public void Run()
		{
			//Upate list on open
			OnWindowOpened += RecalculateList;
		}

		void RecalculateList()
		{
			var citizens = unitSystem.Citizens[SystemsManager.MainPlayerFaction];
			int count = Mathf.Max(citizens.Count, entries.Count);
			GUICitizenOverviewListEntry entry;
			for (int i = 0; i < count; i++)
			{
				//Entry pooling
				if (i >= entries.Count)
				{
					entry = Instantiate(listEntryPrefab, listContainer, false);
					entries.Add(entry);
				}
				else
					entry = entries[i];
				//Update data or disable
				if (i >= citizens.Count)
					entry.gameObject.SetActive(false);
				else
				{
					var citizen = citizens[i];	
					entry.button.onClick.RemoveAllListeners();
					entry.button.onClick.AddListener(() =>
					{
						selectionTool.SelectEntity(citizen);
						cameraController.FollowUnit(citizen);
					});
					entry.Setup(citizen);
				}
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