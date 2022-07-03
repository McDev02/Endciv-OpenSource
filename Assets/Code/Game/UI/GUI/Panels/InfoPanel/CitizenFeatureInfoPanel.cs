using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class CitizenFeatureInfoPanel : BaseFeatureInfoPanel
	{
		public Text occupation;
		public Text genderAge;
		public Text citizenSince;

		public Image homeImage;

		public GUIProgressBar moodBar;

		public GUIProgressBar healthMoodBar;
		public GUIProgressBar thirstBar;
		public GUIProgressBar hungerBar;
		public GUIProgressBar hygieneBar;
		public GUIProgressBar stressBar;
		public GUIProgressBar vitalityBar;
		public GUIProgressBar homeBar;

		protected override void Awake()
		{
			base.Awake();
		}

		public override void UpdateData()
		{
			base.UpdateData();
			if (entity == null)
				return;
			if (!entity.HasFeature<CitizenAIAgentFeature>())
			{
				OnClose();
				return;
			}
			var agent = entity.GetFeature<CitizenAIAgentFeature>();
			var being = entity.GetFeature<LivingBeingFeature>();

			occupation.text = $"{agent.Occupation.ToString()}\n{agent.currentShedule.ToString()}";

			thirstBar.Value = being.Thirst.Progress;
			hungerBar.Value = being.Hunger.Progress;

			var gender = LocalizationManager.GetText($"#Unit/Properties/{being.gender.ToString()}");
			genderAge.text = $"{gender}, { being.age.ToString()}";
			var joinedSince = entity.systemsManager.timeManager.GetDaysSince(entity.GetFeature<EntityFeature>().BornTimeTick);
			citizenSince.text = $"{joinedSince} {(joinedSince == 1 ? "Day" : "Days")}";

			//Update home image
			var col = Color.white;
			if (agent.HasHome)
			{
				homeImage.sprite = ResourceManager.Instance.GetIcon(agent.Home.Entity.StaticData.ID, EResourceIconType.Building);
				col.a = 1;
				homeImage.rectTransform.sizeDelta = homeImage.sprite.rect.size;
			}
			else
			{
				col.a = 0;
				homeImage.sprite = null;
			}
			homeImage.color = col;

			//Update progress bars
			UpdateBar(moodBar, agent.mood);
			UpdateBar(healthMoodBar, agent.HealthNeed.Mood);
			UpdateBar(thirstBar, agent.ThirstNeed.Mood, being.Thirst.Progress);
			UpdateBar(hungerBar, agent.HungerNeed.Mood, being.Hunger.Progress);
			UpdateBar(hygieneBar, agent.CleaningNeed.Mood);
			UpdateBar(stressBar, agent.StressNeed.Mood);

			var vitality = being.vitality.Mood.NegativeToZero();
			UpdateBar(vitalityBar, vitality, false);
			UpdateBar(homeBar, agent.HomehNeed.Mood);
		}

		string NeedToText(string title, EntityNeed need)
		{
			return $"{title}: { need.Value.ToString("0.00")} Mood: { need.Mood.ToString("0.00")}";
		}
	}
}