using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class HousingFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] GUIUnitListEntry occupantListEntryPrefab;
		[SerializeField] Transform occupantListContainer;
		List<GUIUnitListEntry> occupantEntries = new List<GUIUnitListEntry>();
		[SerializeField] Text infoLbl;

		protected override void Awake()
		{
			base.Awake();
			if (occupantListEntryPrefab.transform.parent == occupantListContainer)
				occupantEntries.Add(occupantListEntryPrefab);
		}

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			var home = entity.GetFeature<HousingFeature>();
			if (home == null)
			{
				OnClose();
				return;
			}

			infoLbl.text = $"{LocalizationManager.GetText("#UI/Game/InfoPanels/Home/Occupants")} {home.CurrentOccupants} / {home.MaxOccupants}";

			var cam = controller.gameManager.CameraController;
			int count = Mathf.Max(home.CurrentOccupants, occupantEntries.Count);
			for (int i = 0; i < count; i++)
			{
				if (i >= home.CurrentOccupants)
				{
					occupantEntries[i].gameObject.SetActive(false);
				}
				else
				{
					GUIUnitListEntry entry;
					if (occupantEntries.Count <= i)
					{
						entry = Instantiate(occupantListEntryPrefab, occupantListContainer);
						occupantEntries.Add(entry);
					}
					else
						entry = occupantEntries[i];

					entry.gameObject.SetActive(true);

					var e = home.Occupants[i];
					entry.button.onClick.RemoveAllListeners();
					entry.button.onClick.AddListener(() =>
					{
						cam.FollowUnit(e);
					});
					entry.Setup(home.Occupants[i]);
				}
			}
		}
	}
}