using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

namespace Endciv
{
	public class GatheringPanel : ContentPanel
	{
		[SerializeField] Slider materialSlider;
		[SerializeField] Slider wasteSlider;

		int activeSlider;
		CitizenAISystem citizenAISystem;

		public void Setup(CitizenAISystem citizenAISystem)
		{
			this.citizenAISystem = citizenAISystem;
			materialSlider.minValue = wasteSlider.minValue = 0;
			materialSlider.maxValue = wasteSlider.maxValue = 1;
			materialSlider.value = wasteSlider.value = 0.5f;

			UpdateData();
		}

		public override void UpdateData()
		{
			if (activeSlider == 0)
				wasteSlider.value = 1f - materialSlider.value;
			else
				materialSlider.value = 1f - wasteSlider.value;
		}

		public void OnValuesChanged(int id)
		{
			activeSlider = id;
			UpdateData();
		}
	}
}