using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace Endciv
{
	public class GeneralSettingsManager : BaseSettingsManager<GeneralSettingsDataBase>
	{
		public enum ESaveInterval
		{
			m5,
			m10,
			m15
		}

		public bool enableEdgeScroll;
		public float mouseSensitivity;
		public bool camMoveInvertX;
		public bool camMoveInvertY;
		public float mouseRotationSensitivity;
		public bool camRotateInvertX;
		public bool camRotateInvertY;

		private ESaveInterval m_SaveInterval;
		private bool waitForValidation;
		private bool generalDataChanged;
		public Action<float> OnCanvasScaleChanged;

		public bool userHasSelectedLanguage;

		public float UIScale;
		public float UIScaleInv;

		public bool degreeFahrenheit;

		public string LanguageID = "en_us";
		public int SaveInterval
		{
			get
			{
				return (int)m_SaveInterval;
			}
			set
			{
				m_SaveInterval = (ESaveInterval)value;
			}
		}

		public override void Setup(Main main)
		{
			this.main = main;
			Debug.Log("Assigned temp settings");
			CurrentSettings = main.saveManager.UserSettings.generalSettings;
			ValidateUserSettings(CurrentSettings);
			TmpSettings = CurrentSettings.GetCopy();
			if (TmpSettings == null)
				TmpSettings = GetTemplateData();
			UpdateSettings();
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void ValidateUserSettings(GeneralSettingsDataBase data)
		{
			if (string.IsNullOrEmpty(data.language))
			{
				userHasSelectedLanguage = false;
				data.language = "en_us";
			}
			else
				userHasSelectedLanguage = true; ;
		}

		public GeneralSettingsDataBase GetTemplateData()
		{
			var tmpSettings = new GeneralSettingsDataBase();
			try
			{
				tmpSettings.Setting = "DefaultSettings";
				tmpSettings.uiScale = Screen.dpi >= 120 ? 1.5f : 1f;
				TmpSettings.language = "en_us";
				tmpSettings.saveIterationInterval = 1;
				tmpSettings.degreeFahrenheit =false;
				tmpSettings.mouseEdgeScroll = true;
				tmpSettings.mouseSensitivity = 1.5f;
				tmpSettings.mouseRotationSensitivity = 1f;

			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
			}
			return tmpSettings;
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public override void ApplyTemporaryValues(bool checkSaftey = true, bool writeToDisk = true)
		{
			ValidateTemporaryData();

			//Todo, do only save after saftey check was made

			UpdateSettings();

			//Show Keep changes window
			if (generalDataChanged && checkSaftey)
			{
				waitForValidation = true;
			}
			//Apply changes directly
			else if (writeToDisk)
			{
				//Apply temporary changes and write to disk.
				Main.Instance.saveManager.UserSettings.generalSettings.GetDataFrom(TmpSettings);
				Main.Instance.saveManager.SaveUserSettings();
			}
		}

		protected override void ValidateTemporaryData()
		{
			TmpSettings.uiScale = Mathf.Clamp(TmpSettings.uiScale, 0.25f, 2f);
			TmpSettings.mouseSensitivity = Mathf.Clamp(TmpSettings.mouseSensitivity, 0.01f, 2f);
			TmpSettings.mouseRotationSensitivity = Mathf.Clamp(TmpSettings.mouseRotationSensitivity, 0.01f, 2f);
		}

		protected override void UpdateSettings()
		{
			//if (TmpSettings.uiScale != CurrentSettings.uiScale)
			ApplyUIScaling();

			RegisterAutoSaveCallback();
			enableEdgeScroll = TmpSettings.mouseEdgeScroll;

			mouseSensitivity = Mathf.Pow(TmpSettings.mouseSensitivity, 3f);
			camMoveInvertX = TmpSettings.camMoveInvertX;
			camMoveInvertY = TmpSettings.camMoveInvertY;

			mouseRotationSensitivity = Mathf.Pow(TmpSettings.mouseRotationSensitivity, 2f);
			camRotateInvertX = TmpSettings.camRotateInvertX;
			camRotateInvertY = TmpSettings.camRotateInvertY;

			LocalizationManager.Load(TmpSettings.language);
		}

		private void ApplyUIScaling()
		{
			var canvases = FindObjectsOfType<Canvas>();
			foreach (var canvas in canvases)
			{
				canvas.scaleFactor = 1;
			}
			var scalers = FindObjectsOfType<CanvasScaler>();
			var realscale = TmpSettings.uiScale / 2f;
			foreach (var scaler in scalers)
			{
				scaler.scaleFactor = realscale;
				if (scaler.enabled)
				{
					scaler.enabled = false;
					scaler.enabled = true;
				}
			}
			if (OnCanvasScaleChanged != null)
				OnCanvasScaleChanged.Invoke(realscale);
		}

		public void RegisterAutoSaveCallback()
		{
			if (main.GameManager == null || main.GameManager.timeManager == null)
				return;
			float interval = 300f;
			if (TmpSettings.saveIterationInterval == 1)
				interval = 900f;			
			Main.Instance.GameManager.timeManager.RegisterTimedEvent(interval, AutoSaveCallback, ETimeScaleMode.UnscaledUnpaused, false);
		}

		private void AutoSaveCallback()
		{
			StartCoroutine(QuickSaveRoutine());
		}

		private IEnumerator QuickSaveRoutine()
		{
			Main.Instance.GameManager.GameGUIController.SetLoadingIcon(true);
			yield return new WaitForSeconds(1.5f);
			Main.Instance.saveManager.AutoSave();
			yield return new WaitForSeconds(1.5f);
			Main.Instance.GameManager.GameGUIController.SetLoadingIcon(false);
		}

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			ApplyUIScaling();
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}