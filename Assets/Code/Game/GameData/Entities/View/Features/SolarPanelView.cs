using UnityEngine;

namespace Endciv
{
	public class SolarPanelView : MonoBehaviour
	{
		[SerializeField] float orientationRandomness = 0.05f;
		[SerializeField] Transform rotator;
		WeatherSystem weatherSystem;

		void Start()
		{			
			weatherSystem = Main.Instance.GameManager.SystemsManager.WeatherSystem;
			UpdateView();
		}

		void UpdateView()
		{
			Vector3 random = new Vector3(CivRandom.Range(-orientationRandomness, orientationRandomness), 0, CivRandom.Range(-orientationRandomness, orientationRandomness));
			rotator.LookAt(rotator.position + weatherSystem.SolarDirection + random, Vector3.up);
		}
	}
}