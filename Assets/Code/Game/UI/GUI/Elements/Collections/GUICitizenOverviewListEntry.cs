using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUICitizenOverviewListEntry : MonoBehaviour
	{
		public Button button;
		[SerializeField] private new Text name;
		[SerializeField] private Text mood;
		[SerializeField] private Text occupation;
		[SerializeField] private Text home;

		bool hasHome;
		int oldMood;
		EOccupation tmpOccupation;
		CitizenShedule.ESheduleType tmpSheduleType;

		BaseEntity citizen;

		public void Setup(BaseEntity citizen)
		{
			this.citizen = citizen;
			UpdateValues();
		}

		public void UpdateValues()
		{
			if (citizen == null)
			{
				gameObject.SetActive(false);
				return;
			}

			//We chache values to limit GC allocation
			var ai = citizen.GetFeature<CitizenAIAgentFeature>();
			name.text = citizen.GetFeature<EntityFeature>().EntityName;

			var roundedMood = Mathf.RoundToInt(ai.mood * 100);
			if (oldMood != roundedMood)
			{
				oldMood = roundedMood;
				mood.text = roundedMood.ToString();
			}
			if (tmpOccupation != ai.Occupation || tmpSheduleType != ai.currentShedule)
			{
				tmpOccupation = ai.Occupation; tmpSheduleType = ai.currentShedule;
				occupation.text = $"Occupation: {ai.Occupation.ToString()}  Shedule: {ai.currentShedule.ToString()}";
			}
			if (hasHome != ai.HasHome)
			{
				hasHome = ai.HasHome;
				home.text = ai.HasHome ? "Has Home" : "Homeless";
			}
		}
	}
}