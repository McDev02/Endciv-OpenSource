using System;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class GUIOccupationListEntry : MonoBehaviour
	{
		CitizenAISystem aiSystem;

		[SerializeField] Text Title;
		[SerializeField] Text currentAmount;
		[SerializeField] Text wantedAmount;
		[SerializeField] CanvasGroup canvasGroup;
		private bool interactable;
		public bool Interactable
		{
			get { return interactable; }
			set
			{
				interactable = value;
				canvasGroup.interactable = interactable;
				canvasGroup.alpha = interactable ? 1 : 0.5f;
			}
		}
		CitizenAISystem.OccupationSetting occupationSetting;

		public void OnIncreaseAssignment()
		{
			aiSystem.IncreaseWantedOccupation(occupationSetting.occupation);
		}
		public void OnDecreaseAssignment()
		{
			aiSystem.DecreaseWantedOccupation(occupationSetting.occupation);
		}

		internal void Setup(CitizenAISystem aiSystem, CitizenAISystem.OccupationSetting occupationSetting, bool isAssignable)
		{
			this.aiSystem = aiSystem;
			this.occupationSetting = occupationSetting;
			wantedAmount.gameObject.SetActive(isAssignable);
			//Todo temporary implementation
			if (occupationSetting.occupation == EOccupation.None)
				Title.text = LocalizationManager.GetText($"#UI/Game/Windows/Occupation/Children");
			else Title.text = LocalizationManager.GetText($"#UI/Game/Windows/Occupation/{occupationSetting.occupation.ToString()}");

		}

		internal void UpdateValues()
		{
			currentAmount.text = occupationSetting.assignedCitizens.Count.ToString();
			wantedAmount.text = occupationSetting.wantedAmount.ToString();
		}
	}
}