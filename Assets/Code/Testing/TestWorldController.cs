using UnityEngine;
using Endciv;

namespace Endciv.Testing
{
	public class TestWorldController : MonoBehaviour
	{
		[SerializeField] int mapSize;
		[SerializeField] CameraController camera;
		[SerializeField] GameInputManager inputManager;
		WeatherSystem weatherSystem;
		TimeManager timeManager;

		float timer;

		// Use this for initialization
		void Start()
		{
			timeManager = new TimeManager(null, null);
			weatherSystem = new WeatherSystem(timeManager, null);
			camera.SetBounds(new Rect(0, 0, mapSize, mapSize));

			camera.Setup(inputManager);
			camera.Run();
		}

		private void Update()
		{
			if (timer >= 1)
			{
				timeManager.NextGameTick();
				timer -= 1;
			}

			timeManager.SetTickProgress(timer);
			weatherSystem.UpdateGameLoop();

			timer += Time.deltaTime;
		}
	}
}