using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class AudioGameController : MonoBehaviour
	{
		AudioManager audioManager;
		GameManager gameManager;
		WeatherSystem weatherSystem;
		CameraController cameraController;

		bool isActive;

		[SerializeField] AudioClip heavyRainClip;
		[SerializeField] AudioClip lightRainClip;

		public AudioManager.AudioTrackEntry heavyRainTrack;
		public AudioManager.AudioTrackEntry lightRainTrack;

		private void Awake()
		{
			heavyRainTrack.Setup("Rain", gameObject.AddComponent<AudioSource>(), true);
			heavyRainTrack.source.clip = heavyRainClip;
			lightRainTrack.Setup("Rain", gameObject.AddComponent<AudioSource>(), true);
			lightRainTrack.source.clip = lightRainClip;

		}

		public void Run(AudioManager audioManager, GameManager gameManager)
		{
			this.audioManager = audioManager;
			this.gameManager = gameManager;

			weatherSystem = gameManager.SystemsManager.WeatherSystem;
			cameraController = gameManager.CameraController;

			heavyRainTrack.Play();
			lightRainTrack.Play();

			isActive = true;
		}
		public void Stop()
		{
			isActive = false;
			heavyRainTrack.Stop();
			lightRainTrack.Stop();
		}

		// Update is called once per frame
		void Update()
		{
			if (!isActive) return;

			UpdateRain();
		}

		void UpdateRain()
		{
			float volume = 0;
			var rainfall = weatherSystem.Rainfall;
			var zoom = cameraController.Model.Zoom.CurrentRelative;
			if (rainfall > 0)
			{
				volume = (1 - 0.7f * Mathf.Pow(zoom, 2)) * Mathf.Clamp01(rainfall * 2);
			}
			float rainFade = Mathf.Clamp01((rainfall - 0.5f) * 5);
			heavyRainTrack.SetVolume(volume);
			heavyRainTrack.source.volume = rainFade;
			lightRainTrack.source.volume = 1 - rainFade;
			//thunderTrack.SetVolume(Mathf.Clamp01((World.Instance.m_WeatherSystem.Rainfall - 0.6f) * 2.5f));
		}
	}
}