using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Endciv
{
	public class AudioManager : BaseSettingsManager<AudioSettingsData>
	{

		[SerializeField] AudioGameController audioGameController;

		public enum EAmbientMode { None, Processing, MainMenu, Game }
		EAmbientMode m_CurrentMode;
		Coroutine m_DeferredClipRoutine;
		private bool audioDataChanged;
		private bool waitForValidation;

		struct DeferredClip
		{
			public AudioClip Clip;
			public float Delay;

			public DeferredClip(AudioClip clip, float delay)
			{
				Clip = clip;
				Delay = delay;
			}
		}
		List<DeferredClip> m_DeferredList = new List<DeferredClip>();

		public AudioMixer masterMixer;
		public AudioMixer natureMixer;
		public AudioMixer weatherMixer;

		public AudioClip mainMenuMusic;
		public SoundPool gameMusic;

		public AudioTrackEntry m_MasterTrack;

		public AudioTrackEntry m_MusicTrack;
		public AudioTrackEntry m_SoundsTrack;
		public AudioTrackEntry m_UITrack;

		[Serializable]
		public struct AudioTrackEntry
		{
			const int MINDB = -80;

			string m_Name;
			string m_VolumeName;

			public AudioSource source;
			public AudioMixerGroup mixerGroup;
			public float targetVolume;

			public string Name
			{
				get { return m_Name; }
			}

			public float Volume
			{
				get
				{
					float value = 0;
					var mixer = mixerGroup.audioMixer;
					if (!string.IsNullOrEmpty(m_VolumeName))
						mixer.GetFloat(m_VolumeName, out value);
					value = value <= MINDB ? 0 : Mathf.Pow(10, value * (1f / 20f));
					return value;
				}
			}

			public void SetVolume(float value, bool setTargetVolume = true)
			{
				if (setTargetVolume)
					targetVolume = value;
				float db = value > 0 ? 20f * Mathf.Log10(value) : MINDB;
				var mixer = mixerGroup.audioMixer;
				if (!string.IsNullOrEmpty(m_VolumeName))
					mixer.SetFloat(m_VolumeName, db);
			}

			internal void Setup(string name, AudioSource source, bool loop)
			{
				m_Name = name;
				m_VolumeName = name + "Volume";
				this.source = source;
				this.source.loop = loop;
				this.source.outputAudioMixerGroup = mixerGroup;
			}

			internal void StoreValue()
			{
				targetVolume = Volume;
			}

			internal void Stop()
			{
				StoreValue();
				if (source != null) source.Stop();
				if(!string.IsNullOrEmpty(m_VolumeName))
					mixerGroup.audioMixer.SetFloat(m_VolumeName, MINDB);
			}

			internal void Play()
			{
				source.Play();
				//SetVolume(targetVolume);
			}
		}

		[SerializeField]
		SoundPool[] soundPools;

		float GetDecibel(float f)
		{
			return 20 * Mathf.Log10(f);
		}

		public float MasterVolume
		{
			get { return m_MasterTrack.Volume; }
			set { m_MasterTrack.SetVolume(value); }
		}
		public float SoundVolume
		{
			get { return m_SoundsTrack.Volume; }
			set { m_SoundsTrack.SetVolume(value); }
		}
		public float MusicVolume
		{
			get { return m_MusicTrack.Volume; }
			set { m_MusicTrack.SetVolume(value); }
		}
		public float SpeechVolume
		{
			get { return m_UITrack.Volume; }
			set { m_UITrack.SetVolume(value); }
		}

		public override void Setup(Main main)
		{
			this.main = main;
			TmpSettings = null;
			if (main.saveManager.UserSettings != null && main.saveManager.UserSettings.audioSettings != null)
				TmpSettings = main.saveManager.UserSettings.audioSettings.GetCopy();
			if (TmpSettings == null)
				TmpSettings = GetTemplateData();
			//No existing graphics settings were found, default to full volume
			if (string.IsNullOrEmpty(TmpSettings.Setting))
			{

			}
			else
			{
				TmpSettings.Setting = "custom";
			}
		}

		void Update()
		{
			switch (m_CurrentMode)
			{
				case EAmbientMode.MainMenu:
					break;
				case EAmbientMode.Game:
					if (!m_MusicTrack.source.isPlaying)
					{
						var clip = gameMusic.sounds[0].Sound.SelectRandom<AudioClip>();
						m_MusicTrack.source.clip = clip;
						m_MusicTrack.source.Play();
					}
					break;
				default:
					break;
			}
		}

		public AudioSettingsData GetTemplateData()
		{
			AudioSettingsData tmpSettings = new AudioSettingsData();
			tmpSettings.Setting = "FullVolume";
			tmpSettings.totalVolume = 1f;
			tmpSettings.musicVolume = 1f;
			tmpSettings.soundVolume = 1f;
			tmpSettings.uiVolume = 1f;
			return tmpSettings;
		}

		public override void ApplyTemporaryValues(bool checkSaftey = true, bool writeToDisk = true)
		{
			ValidateTemporaryData();

			//Todo, do only save after saftey check was made

			UpdateSettings();

			//Show Keep changes window
			if (audioDataChanged && checkSaftey)
			{
				waitForValidation = true;
			}
			//Apply changes directly
			else if (writeToDisk)
			{
				//Apply temporary changes and write to disk.
				Main.Instance.saveManager.UserSettings.audioSettings.GetDataFrom(TmpSettings);
				Main.Instance.saveManager.SaveUserSettings();
			}
		}

		protected override void ValidateTemporaryData()
		{
			TmpSettings.musicVolume = Mathf.Clamp(TmpSettings.musicVolume, 0f, 1f);
			TmpSettings.soundVolume = Mathf.Clamp(TmpSettings.soundVolume, 0f, 1f);
			TmpSettings.totalVolume = Mathf.Clamp(TmpSettings.totalVolume, 0f, 1f);
			TmpSettings.uiVolume = Mathf.Clamp(TmpSettings.uiVolume, 0f, 1f);
		}

		protected override void UpdateSettings()
		{
			if (TmpSettings == null)
				return;
			MasterVolume = TmpSettings.totalVolume;
			SoundVolume = TmpSettings.soundVolume;
			MusicVolume = TmpSettings.musicVolume;
			SpeechVolume = TmpSettings.uiVolume;
		}

		protected override void Awake()
		{
			m_MasterTrack.Setup("Master", gameObject.AddComponent<AudioSource>(), true);
			m_MusicTrack.Setup("Music", gameObject.AddComponent<AudioSource>(), true);
			m_SoundsTrack.Setup("Sounds", gameObject.AddComponent<AudioSource>(), false);
			m_UITrack.Setup("UI", gameObject.AddComponent<AudioSource>(), false);

			UpdateSettings();
			base.Awake();

			//Weather
			//Nature
			SetState(EAmbientMode.None);
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			UpdateSettings();
		}

		/// <summary>
		/// Time in Seconds
		/// </summary>
		IEnumerator FadeOutTrack(AudioTrackEntry track, float time)
		{
			float rate = 1f / time;
			float delta = 0f;
			float startVolume = track.Volume;
			while (delta < 1f)
			{
				delta += Main.unscaledDeltaTimeSafe * rate;
				track.SetVolume(Mathf.Lerp(startVolume, 0f, delta), false);
				yield return null;
			}
			track.source.Stop();
		}

		/// <summary>
		/// Time in Seconds
		/// </summary>
		IEnumerator FadeInTrack(AudioTrackEntry track, float time)
		{
			float rate = 1f / time;
			float delta = 0f;
			float endVolume = track.targetVolume;
			track.source.Play();
			while (delta < 1f)
			{
				delta += Main.unscaledDeltaTimeSafe * rate;
				track.SetVolume(Mathf.Lerp(0f, endVolume, delta), false);
				yield return null;
			}
		}

		IEnumerator PlayDeferredClips()
		{
			while (true)
			{
				if (m_DeferredList.Count > 0)
				{
					var clip = m_DeferredList[0];
					m_DeferredList.RemoveAt(0);
					m_SoundsTrack.source.PlayOneShot(clip.Clip);

					yield return StartCoroutine(UnscaledWaitForSeconds(clip.Delay));
				}
				else
				{
					StopCoroutine(m_DeferredClipRoutine);
					m_DeferredClipRoutine = null;
					break;
				}
			}
			yield return null;
		}

		IEnumerator UnscaledWaitForSeconds(float seconds)
		{
			float t = 0f;
			while (t < seconds)
			{
				t += Time.unscaledDeltaTime;
				yield return null;
			}
		}

		// private void Update()
		// {
		//     if (m_CurrentMode == EAmbientMode.Game)
		//     {
		//         float volume = 0;
		//         //if (World.Instance.IsReady && World.Instance.m_WeatherSystem.Rainfall > 0)
		//         //{
		//         //    volume = (1 - 0.7f * Mathf.Pow(CivInput.Instance.MainPlayerCamera.Zoom, 2)) * Mathf.Lerp(1, World.Instance.m_WeatherSystem.Rainfall, 0.5f);
		//         //}
		//         float rainFade = 0;// Mathf.Clamp01((World.Instance.m_WeatherSystem.Rainfall - 0.6f) * 12);
		//         m_RainTrack.SetVolume(volume);
		//         m_RainTrack.m_Source.volume = rainFade;
		//         m_LightRainTrack.m_Source.volume = 1 - rainFade;
		//         //m_ThunderTrack.SetVolume(Mathf.Clamp01((World.Instance.m_WeatherSystem.Rainfall - 0.6f) * 2.5f));
		//         m_ThunderTrack.SetVolume(0);
		//     }
		// }

		public void PlaySound(string key)
		{
			AudioClip clip = GetSoundClip(key);
			if (clip == null)
				return;

			m_SoundsTrack.source.PlayOneShot(clip);
		}
		public void PlaySoundDeferred(string key, float delayFactor = 1)
		{
			AudioClip clip = GetSoundClip(key);
			if (clip == null)
				return;
			m_DeferredList.Add(new DeferredClip(clip, clip.length * delayFactor));

			if (m_DeferredClipRoutine == null)
				m_DeferredClipRoutine = StartCoroutine(PlayDeferredClips());
		}
		public void PlayRandomSound(string key)
		{
			AudioClip clip = GetSoundClip(key);
			if (clip == null)
				return;

			m_SoundsTrack.source.PlayOneShot(clip);
		}

		public void PlaySound(string key, AudioSource source)
		{
			AudioClip clip = GetSoundClip(key);
			if (clip == null)
				return;

			source.PlayOneShot(clip);
		}

		private AudioClip GetSoundClip(string key)
		{
			for (int s = 0; s < soundPools.Length; s++)
			{
				var soundPool = soundPools[s];
				for (int i = 0; i < soundPool.sounds.Count; i++)
				{
					if (soundPool.sounds[i].Key == key) return soundPool.sounds[i].Sound.SelectRandom<AudioClip>();
				}
			}
			return null;
		}

		internal void SetState(EAmbientMode mode)
		{
			m_CurrentMode = mode;

			switch (mode)
			{
				case EAmbientMode.None:
					audioGameController.Stop();
					m_MusicTrack.Stop();
					break;
				case EAmbientMode.Processing:
					audioGameController.Stop();
					StartCoroutine(FadeOutTrack(m_MusicTrack, 0.3f));
					break;
				case EAmbientMode.MainMenu:
					audioGameController.Stop();
					m_MusicTrack.source.loop = true;
					m_MusicTrack.source.clip = mainMenuMusic;
					m_MusicTrack.source.volume = 0.6f;
					StartCoroutine(FadeInTrack(m_MusicTrack, 2));
					break;
				case EAmbientMode.Game:
					m_MusicTrack.source.loop = false;
					var clip = gameMusic.sounds[0].Sound.SelectRandom<AudioClip>();
					m_MusicTrack.source.clip = clip;
					m_MusicTrack.source.volume = 0.35f;
					StartCoroutine(FadeInTrack(m_MusicTrack, 1));
					audioGameController.Run(this, main.GameManager);
					break;
				default:
					break;
			}
		}
	}
}