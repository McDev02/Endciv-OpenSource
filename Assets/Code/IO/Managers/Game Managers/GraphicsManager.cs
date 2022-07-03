using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using System;

namespace Endciv
{
	public class GraphicsManager : BaseSettingsManager<GraphicsSettingsData>
	{
		[SerializeField] GraphicSettingsPreset[] GraphicPresets;
		const int DEFAULT_SETTING = 3;
		const int CUSTOM_SETTING = 0;
		public bool ScreenDataChanged { get; private set; }
		bool waitForValidation;

		public List<MyResolution> ScreenResolutions;

		public Action OnScreenResolutionChanged;

		public struct MyResolution
		{
			public int width;
			public int height;
			public int refreshRate;

			public MyResolution(int width, int height)
			{
				this.width = width;
				this.height = height;
				this.refreshRate = 60;
			}
		}
		public override void Setup(Main main)
		{
			this.main = main;
			if (ScreenResolutions == null)
				LoadAvailableScreenResolutions();
			TmpSettings = main.saveManager.UserSettings.graphicSettings.GetCopy();
			if (TmpSettings == null)
				TmpSettings = GetTemplateData();
			ApplyTemporaryValues(false, false);
		}

		/// <summary>
		/// Default values
		/// </summary>
		public GraphicsSettingsData GetTemplateData()
		{
			if (ScreenResolutions == null)
				LoadAvailableScreenResolutions();
			var preset = GraphicPresets[DEFAULT_SETTING];
			var tmpSettings = preset.ToGraphicSettingsData();
			tmpSettings.Setting = preset.name;

			var res = ScreenResolutions[ScreenResolutions.Count - 1];
			tmpSettings.ScreenWidth = res.width;
			tmpSettings.ScreenHeight = res.height;
			tmpSettings.Fullscreen = true;

			tmpSettings.VSynch = true;
			return tmpSettings;
		}

		public int FindAndSetCurrentPreset()
		{
			for (int i = 1; i < GraphicPresets.Length; i++)
			{
				if (CompareSettings(TmpSettings, GraphicPresets[i]))
					return i;
			}
			//We assign custom preset;
			UpdateCustomPreset(TmpSettings);
			return CUSTOM_SETTING;
		}

		void UpdateCustomPreset(GraphicsSettingsData graphicSettings)
		{
			var prefab = GraphicPresets[CUSTOM_SETTING];
			prefab.AAQuality = graphicSettings.AAQuality;
			prefab.Sharpen = graphicSettings.Sharpen;

			prefab.ShadowQuality = graphicSettings.ShadowQuality;
			prefab.TextureQuality = graphicSettings.TextureQuality;

			prefab.SSAO = graphicSettings.SSAO;
			prefab.Bloom = graphicSettings.Bloom;
		}

		public bool CompareSettings(GraphicsSettingsData graphicSettings, GraphicSettingsPreset prefab)
		{
			if (TmpSettings.AAQuality != prefab.AAQuality) return false;
			if (TmpSettings.Sharpen != prefab.Sharpen) return false;
			if (TmpSettings.ShadowQuality != prefab.ShadowQuality) return false;
			if (TmpSettings.TextureQuality != prefab.TextureQuality) return false;
			if (TmpSettings.SSAO != prefab.SSAO) return false;
			if (TmpSettings.Bloom != prefab.Bloom) return false;
			return true;
		}

		public string[] GetPresetNames()
		{
			string[] names = new string[GraphicPresets.Length];
			for (int i = 0; i < GraphicPresets.Length; i++)
			{
				names[i] = GraphicPresets[i].name;
			}
			return names;
		}

		public void ApplyPrefab(int prefabID)
		{
			//Apply preset to settings
			if (prefabID != CUSTOM_SETTING)
			{
				GraphicSettingsPreset prefab = GraphicPresets[prefabID];
				TmpSettings.AAQuality = prefab.AAQuality;
				TmpSettings.Sharpen = prefab.Sharpen;

				TmpSettings.ShadowQuality = prefab.ShadowQuality;
				TmpSettings.TextureQuality = prefab.TextureQuality;

				TmpSettings.SSAO = prefab.SSAO;
				TmpSettings.Bloom = prefab.Bloom;
			}
			//Apply current settings to custom prefab
			else
				UpdateCustomPreset(TmpSettings);
		}

		public override void DiscardTemporaryValues()
		{
			//Set everything back to before changes were made
			TmpSettings.GetDataFrom(Main.Instance.saveManager.UserSettings.graphicSettings);
		}

		public override void ApplyTemporaryValues(bool checkSaftey = true, bool writeToDisk = true)
		{
			ValidateTemporaryData();

			//Todo, do only save after saftey check was made

			UpdateSettings();

			//Show Keep changes window
			if (ScreenDataChanged && checkSaftey)
			{
				waitForValidation = true;
			}
			//Apply changes directly
			else if (writeToDisk)
			{
				//Apply temporary changes and write to disk.
				Main.Instance.saveManager.UserSettings.graphicSettings.GetDataFrom(TmpSettings);
				Main.Instance.saveManager.SaveUserSettings();
			}
		}

		public override void StoreTemporaryValues()
		{
			if (!waitForValidation) return;
			//Apply temporary changes and write to disk.
			Main.Instance.saveManager.UserSettings.graphicSettings.GetDataFrom(TmpSettings);
			Main.Instance.saveManager.SaveUserSettings();
		}

		void UpdateScreenResolution()
		{
			Screen.SetResolution(TmpSettings.ScreenWidth, TmpSettings.ScreenHeight, TmpSettings.Fullscreen);
			OnScreenResolutionChanged?.Invoke();
		}
		protected override void UpdateSettings()
		{
			ScreenDataChanged = false;
			var userSettings = Main.Instance.saveManager.UserSettings.graphicSettings;
			if (TmpSettings.ScreenWidth != userSettings.ScreenWidth ||
				TmpSettings.ScreenHeight != userSettings.ScreenHeight
				|| TmpSettings.Fullscreen != userSettings.Fullscreen)
			{
				ScreenDataChanged = true;
				Screen.SetResolution(TmpSettings.ScreenWidth, TmpSettings.ScreenHeight, TmpSettings.Fullscreen);
				OnScreenResolutionChanged?.Invoke();
			}

			QualitySettings.vSyncCount = (TmpSettings.VSynch) ? 1 : 0;
			switch (TmpSettings.ShadowQuality)
			{
				case GraphicsSettingsData.EShadowQuality.Off:
					QualitySettings.shadows = ShadowQuality.Disable;
					break;
					
				case GraphicsSettingsData.EShadowQuality.Medium:
					QualitySettings.shadows = ShadowQuality.All;
					QualitySettings.shadowResolution = ShadowResolution.Medium;
					break;

				case GraphicsSettingsData.EShadowQuality.High:
					QualitySettings.shadows = ShadowQuality.All;
					QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
					break;
			}

			switch (TmpSettings.AAQuality)
			{
				case GraphicsSettingsData.EAASettings.Off:
					break;
				case GraphicsSettingsData.EAASettings.On:
					break;
			}

			if (Main.Instance.GameManager != null)
			{
				var cameraFx = Main.Instance.gameInputManager.CameraController.postProcessingSettings;
				var behaviour = cameraFx.postProcessingBehavior;

				behaviour.profile.motionBlur.enabled = TmpSettings.MotionBlur;
				//cameraFx.bloom.enabled = TmpSettings.Bloom;
				//cameraFx.ambientOcclusion.enabled = TmpSettings.SSAO;
				cameraFx.EnableSMAA(TmpSettings.AAQuality == GraphicsSettingsData.EAASettings.On);
			}

		}

		#region DataHandlingMethods

		protected override void ValidateTemporaryData()
		{
			var validRes = ScreenResolutions[GetClosestScreenResolution(TmpSettings.ScreenWidth, TmpSettings.ScreenHeight)];
			TmpSettings.ScreenWidth = validRes.width;
			TmpSettings.ScreenHeight = validRes.height;
		}

		void LoadAvailableScreenResolutions()
		{
			ScreenResolutions = new List<MyResolution>();

#if UNITY_EDITOR
			var res = Screen.currentResolution;
			ScreenResolutions.Add(new MyResolution(res.width, res.height));
#else
			var resolutions = Screen.resolutions;
			int minwidth = 1024; int minheight = 700;
			for (int i = 0; i < resolutions.Length; i++)
			{
				var res = new MyResolution(resolutions[i].width, resolutions[i].height);
				if (!ScreenResolutions.Contains(res) && res.width >= minwidth && res.width >= minheight)
					ScreenResolutions.Add(res);
			}
			if (ScreenResolutions.Count <= 0)
			{
				var res = Screen.currentResolution;
				ScreenResolutions.Add(new MyResolution(res.width, res.height));
			}
#endif
		}

		int GetClosestScreenResolution(int width, int height)
		{
			if (ScreenResolutions == null || ScreenResolutions.Count <= 0)
				return 0;

			int result = 0;
			int volume = 0;
			for (int i = 0; i < ScreenResolutions.Count; i++)
			{
				var res = ScreenResolutions[i];
				if (res.width > width || res.height > height)
					continue;

				int newvolume = res.width * res.height;
				if (newvolume >= volume)
				{
					volume = newvolume;
					result = i;
				}
			}
			return result;
		}
		#endregion
	}
}