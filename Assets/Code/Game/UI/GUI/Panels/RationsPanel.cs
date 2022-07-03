using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class RationsPanel : ContentPanel
	{
		[SerializeField] Slider waterSlider;
		[SerializeField] Text waterAmount;
		[SerializeField] Slider nutritionSlider;
		[SerializeField] Text nutritionAmount;
		
		[SerializeField] GUIConsumableRationListEntry rationEntryPrefab;
		[SerializeField] Transform itemListContainer;
		private List<GUIConsumableRationListEntry> rationEntries;
        private Stack<GUIConsumableRationListEntry> entryPool;
        private bool canUpdateValues;
		
		CitizenAISystem citizenAISystem;

		const int waterPortionsPerStep = 2;
		const int nutritionPortionsPerStep = 1;

		public void Setup(CitizenAISystem citizenAISystem, SimpleEntityFactory factory)
		{
			this.citizenAISystem = citizenAISystem;
			waterSlider.minValue = 0;
			waterSlider.maxValue = 6;
			nutritionSlider.minValue = 0;
			nutritionSlider.maxValue = 6;

			waterSlider.value = 3;
			nutritionSlider.value = 3;

            rationEntries = new List<GUIConsumableRationListEntry>();
            entryPool = new Stack<GUIConsumableRationListEntry>();
            var children = itemListContainer.GetComponentsInChildren<GUIConsumableRationListEntry>();
            rationEntries.AddRange(children);
            CleanupEntries();

            var consumableIDs = factory.GetStaticDataIDList<ConsumableFeatureStaticData>();
            canUpdateValues = false;
            foreach(var id in consumableIDs)
            {
                var entry = GetEntry();
                entry.UpdateValues(id, !CitizenAISystem.consumableFilter.Contains(id));
                entry.UpdateUI();
            }
            canUpdateValues = true;
            UpdateRationSelection();
			UpdateData();
		}

		public override void UpdateData()
		{
			float value = waterSlider.value * waterPortionsPerStep * GameConfig.WaterPortion;
			var singularOrPlural = Mathf.Approximately(value, 1) ? LocalizationManager.ETextVersion.Singular : LocalizationManager.ETextVersion.Plural;
			waterAmount.text = $"{value.ToString("0.##")} {LocalizationManager.GetText("#UI/Game/Windows/RationsWindow/RationWaterInfo", singularOrPlural)}";

			value = nutritionSlider.value * nutritionPortionsPerStep;
			singularOrPlural = Mathf.Approximately(value, 1) ? LocalizationManager.ETextVersion.Singular : LocalizationManager.ETextVersion.Plural;
			nutritionAmount.text = $"{value.ToString("0")} {LocalizationManager.GetText("#UI/Game/Windows/RationsWindow/RationNutritionInfo", singularOrPlural)}";
		}

		public void OnValuesChanged()
		{
			UpdateData();
			citizenAISystem.SetRationValues((int)(waterSlider.value * waterPortionsPerStep), (int)(nutritionSlider.value * nutritionPortionsPerStep));
		}

        public void UpdateRationSelection()
        {
            if (!canUpdateValues)
                return;
            CitizenAISystem.consumableFilter.Clear();
            for (int i = rationEntries.Count - 1; i >= 0; i--)
            {
                if (!rationEntries[i].isAllowed)
                    CitizenAISystem.consumableFilter.Add(rationEntries[i].ItemID);
            }
        }

        private GUIConsumableRationListEntry GetEntry()
        {
            GUIConsumableRationListEntry entry = null;
            while (entry == null && entryPool.Count > 0)
                entry = entryPool.Pop();
            if(entry == null)
            {
                entry = Instantiate(rationEntryPrefab, itemListContainer);                
            }
            entry.gameObject.SetActive(true);
            rationEntries.Add(entry);
            return entry;
        }

        private void RecycleEntry(GUIConsumableRationListEntry entry)
        {
            entry.gameObject.SetActive(false);
            rationEntries.Remove(entry);
            entryPool.Push(entry);
        }

        private void CleanupEntries()
        {
            for(int i = rationEntries.Count - 1; i >= 0; i--)
            {
                RecycleEntry(rationEntries[i]);
            }
        }
	}
}