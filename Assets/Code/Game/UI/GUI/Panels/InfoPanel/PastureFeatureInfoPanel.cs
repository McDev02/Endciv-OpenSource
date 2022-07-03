using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
	public class PastureFeatureInfoPanel : BaseFeatureInfoPanel
	{
		[SerializeField] PastureCattleListEntry cattleListEntryPrefab;
		private List<PastureCattleListEntry> cattleListEntryPool;
		[SerializeField] Button allEntriesButton;
		[SerializeField] Transform cattleListContainer;
		PastureFeature feature;

		[SerializeField] GameObject panelAll;
		[SerializeField] GameObject panelCattle;

		[SerializeField] Text countAdults;
		[SerializeField] Text countJuniors;
		[SerializeField] Text countKeep;
		[SerializeField] Text countSlaughter;

		[SerializeField] Transform productionListContainer;
		[SerializeField] GUIResourceInfoEntry productionEntryPrefab;
		private List<GUIResourceInfoEntry> productionEntryPool;

		[SerializeField] GUIProgressBar waterStorage;
		[SerializeField] Text waterValue;
		[SerializeField] GUIProgressBar nutritionStorage;
		[SerializeField] Text nutritionValue;

		Dictionary<string, int> productionCache;
		Dictionary<string, int> cattleAmountCache;

		PastureCattleListEntry activePanel;

		public override void Setup(GameGUIController controller, BaseEntity entity)
		{
			if (entity == null) return;

			if (!entity.HasFeature<PastureFeature>())
				return;
			feature = entity.GetFeature<PastureFeature>();

			PrepareLists();
			PopulateCattleEntries();

			base.Setup(controller, entity);
			SelectOverview();
		}

		void PrepareLists()
		{
			if (cattleListEntryPool == null)
			{
				cattleListEntryPool = new List<PastureCattleListEntry>();
				//Prefab is part of the list already
				if (cattleListEntryPrefab.transform.parent != null && cattleListEntryPrefab.transform.parent == cattleListContainer)
					cattleListEntryPool.Add(cattleListEntryPrefab);
			}
			if (productionEntryPool == null)
			{
				productionEntryPool = new List<GUIResourceInfoEntry>();
				//Prefab is part of the list already
				if (productionEntryPrefab.transform.parent != null && productionEntryPrefab.transform.parent == productionListContainer)
					productionEntryPool.Add(productionEntryPrefab);
			}

			if (cattleAmountCache == null) cattleAmountCache = new Dictionary<string, int>();
			else cattleAmountCache.Clear();
			if (productionCache == null) productionCache = new Dictionary<string, int>();
			else productionCache.Clear();
		}

		void PopulateCattleEntries()
		{
			var cattle = feature.StaticData.Cattle;
			for (int i = 0; i < cattle.Length; i++)
			{
				PastureCattleListEntry entry;
				if (cattleListEntryPool.Count <= i)
				{
					entry = Instantiate(cattleListEntryPrefab, cattleListContainer, false);
					cattleListEntryPool.Add(entry);
				}
				else
					entry = cattleListEntryPool[i];

				var data = cattle[i];
				entry.label.text = LocalizationManager.GetText($"#Animals/{data.ID}/name", LocalizationManager.ETextVersion.Plural);
				entry.cattleID = data.ID;
				entry.AllowCattle = true;  //<--  load from runtime data of the PastureFeature, not yet existing

				if (!cattleAmountCache.ContainsKey(data.ID))
					cattleAmountCache.Add(data.ID, 0);

				var boxedEntry = entry;
				entry.button.onClick.RemoveAllListeners();
				entry.button.onClick.AddListener(() => SelectCattle(boxedEntry));
			}
		}

		public void SelectOverview()
		{
			SelectCattle(null);
		}

		private void SelectCattle(PastureCattleListEntry entry)
		{
			activePanel = entry;
			//Handle selected cattle based on ID
			if (entry == null)
			{
				//Else if no cattle matches show all:
				panelCattle.SetActive(false);
				allEntriesButton.interactable = false;
			}
			else
			{
				//For each cattle show this info:
				panelCattle.SetActive(true);
				allEntriesButton.interactable = true;
				//Here we have to setup the Cattle panel, Kevin will handle this
			}

			//UpdateView
			for (int i = 0; i < cattleListEntryPool.Count; i++)
			{
				cattleListEntryPool[i].button.interactable = cattleListEntryPool[i] != entry;
			}
			UpdateData();
		}

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			if (feature == null)
			{
				OnClose();
				return;
			}

			UpdateEntryLists();

			if (activePanel == null)
				UpdateOverviewPanel();
			else
				UpdateCattlePanel();
		}

		public void ChangeSlaugtherLimit(int amount)
		{
			// ChangeSlaugther limit for current panel:
			// activePanel.cattleID;
			//Feature slaughter limit +=  amount ; Then clamp to 0 and 99
		}

		void UpdateEntryLists()
		{
			foreach (var key in cattleAmountCache.Keys.ToList())
			{
				cattleAmountCache[key] = 0;
			}

			for (int i = 0; i < feature.Cattle.Count; i++)
			{
				var id = feature.Cattle[i].Entity.StaticData.ID;

				if (!cattleAmountCache.ContainsKey(id)) //<-- Should never happen anyway
					cattleAmountCache.Add(id, 1);
				else
					cattleAmountCache[id]++;
			}

			for (int i = 0; i < cattleListEntryPool.Count; i++)
			{
				var id = cattleListEntryPool[i].cattleID;
				foreach (var item in cattleAmountCache)
				{
					if (item.Key == id)
						cattleListEntryPool[i].amount.text = item.Value.ToString();
				}
			}
		}

		void UpdateOverviewPanel()
		{
			int adultCount = 0;
			int juniorCount = 0;
			for (int i = 0; i < feature.Cattle.Count; i++)
			{
				var cattle = feature.Cattle[i];
				var being = cattle.Entity.GetFeature<LivingBeingFeature>();
				if (being.age == ELivingBeingAge.Child)
					juniorCount++;
				else
					adultCount++;
			}
			countAdults.text = adultCount.ToString();
			countJuniors.text = juniorCount.ToString();
		}

		void UpdateCattlePanel()
		{
			int adultCount = 0;
			int juniorCount = 0;
			for (int i = 0; i < feature.Cattle.Count; i++)
			{
				var cattle = feature.Cattle[i];
				if (cattle.Entity.StaticData.ID != activePanel.cattleID) continue;

				var being = cattle.Entity.GetFeature<LivingBeingFeature>();
				if (being.age == ELivingBeingAge.Child)
					juniorCount++;
				else
					adultCount++;
			}
			countAdults.text = adultCount.ToString();
			countJuniors.text = juniorCount.ToString();
		}
	}
}