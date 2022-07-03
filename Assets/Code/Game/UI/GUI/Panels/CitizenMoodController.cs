using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class CitizenMoodController : MonoBehaviour
	{
		[SerializeField] GUIProgressBar healthBar;
		[SerializeField] GUIProgressBar thirstBar;
		[SerializeField] GUIProgressBar hungerBar;
		[SerializeField] GUIProgressBar hygieneBar;
		[SerializeField] GUIProgressBar stressBar;
		//[SerializeField] GUIProgressBar homeBar;

		[SerializeField] Color colorGood;
		[SerializeField] Color colorNeutral;
		[SerializeField] Color colorBad;


		public void UpdateData()
		{
			var ts = GameStatistics.MainTownStatistics;

			UpdateBar(healthBar, ts.averageNeedHealth);
			UpdateBar(thirstBar, ts.averageNeedThirst);
			UpdateBar(hungerBar, ts.averageNeedHunger);
			UpdateBar(hygieneBar, ts.averageNeedCleaning);
			UpdateBar(stressBar, ts.averageNeedStress);
		}

		string NeedToText(string title, float need)
		{
			return $"{title}: { Mathf.Round(need * 100).ToString()}";
		}

		void UpdateBar(GUIProgressBar bar, float need, bool isNeed = true)
		{
			if (!isNeed)
				need = need * 2 - 1;

			if (need > 0)
				bar.progressBar.color = Color.Lerp(colorNeutral, colorGood, need);
			else
				bar.progressBar.color = Color.Lerp(colorNeutral, colorBad, -need);

			bar.Value = (need + 1) * 0.5f;
		}
	}
}