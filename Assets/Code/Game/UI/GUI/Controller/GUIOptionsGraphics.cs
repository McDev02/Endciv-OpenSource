using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUIOptionsGraphics : GUIAnimatedPanel
	{
		GraphicsManager graphicsManager;
		private string[] presetNames;
		private string[] shadowPresetNames;
		[SerializeField] UIDropdown Resolution;
		[SerializeField] UIDropdown Presets;
		[SerializeField] Toggle Fullscreen;
		[SerializeField] Toggle VSynch;
		[SerializeField] UIDropdown AA;
		[SerializeField] Toggle SSAO;
		[SerializeField] Toggle MotionBlur;
		[SerializeField] Toggle Bloom;

		[SerializeField] UIDropdown ShadowQuality;

		private int defaultGraphicsSettings = 2;

		public void Setup(GraphicsManager graphicsManager)
		{
			this.graphicsManager = graphicsManager;

			PrepareElements();
			UpdateUI();

			OnUpdateSettings();
		}

		void OnUpdateSettings()
		{
			var tmpGraphicsSettings = graphicsManager.TmpSettings;
			if (tmpGraphicsSettings == null)
				return;

			//Update dropdown selection based on Preset Name
			Presets.SelectValue(Main.Instance.graphicsManager.FindAndSetCurrentPreset());
		}

		public void OnPresetChanged()
		{
			Main.Instance.graphicsManager.ApplyPrefab(Presets.SelectedElement);
			OnUpdateSettings();
		}

		public void DiscardValues()
		{
			//Set everything back to before changes were made
			Main.Instance.graphicsManager.DiscardTemporaryValues();
			OnUpdateSettings();
			UpdateUI();
		}

		public void ApplyValues()
		{
			//Apply temporary changes and write to disk.
			UpdateTempSettings();
			Main.Instance.graphicsManager.ApplyTemporaryValues(false);
			OnUpdateSettings();
		}

		void PrepareElements()
		{
			//Resolutions
			if (graphicsManager.ScreenResolutions.Count <= 1)
			{
				Resolution.AddOption("EDITOR");
				for (int i = 0; i < 11; i++)
					Resolution.AddOption("Option " + i);
			}
			else
			{
				for (int i = 0; i < graphicsManager.ScreenResolutions.Count; i++)
				{
					var res = graphicsManager.ScreenResolutions[i];

					Resolution.AddOption($"{res.width} x {res.height}");
				}
			}

			//Presets
			presetNames = Main.Instance.graphicsManager.GetPresetNames();
			for (int i = 0; i < presetNames.Length; i++)
			{
				Presets.AddOption(presetNames[i]);
			}

			shadowPresetNames = System.Enum.GetNames(typeof(GraphicsSettingsData.EShadowQuality));
			for (int i = 0; i < shadowPresetNames.Length; i++)
			{
				ShadowQuality.AddOption(shadowPresetNames[i]);
			}

			shadowPresetNames = System.Enum.GetNames(typeof(GraphicsSettingsData.EAASettings));
			for (int i = 0; i < shadowPresetNames.Length; i++)
			{
				AA.AddOption(shadowPresetNames[i]);
			}

		}

		void UpdateTempSettings()
		{
			GraphicsManager.MyResolution resolution = new GraphicsManager.MyResolution(0, 0);
			int resIndex = -1;
			if (graphicsManager.ScreenResolutions != null && graphicsManager.ScreenResolutions.Count > 0)
			{
				if (Resolution.SelectedElement >= graphicsManager.ScreenResolutions.Count || Resolution.SelectedElement < 0)
				{
					resIndex = 0;
				}
				else
				{
					resIndex = Resolution.SelectedElement;
				}
			}
			if (resIndex > -1)
				resolution = graphicsManager.ScreenResolutions[resIndex];
			graphicsManager.TmpSettings.ScreenWidth = resolution.width;
			graphicsManager.TmpSettings.ScreenHeight = resolution.height;
			graphicsManager.TmpSettings.Fullscreen = Fullscreen.isOn;
			graphicsManager.TmpSettings.ShadowQuality = (GraphicsSettingsData.EShadowQuality)ShadowQuality.SelectedElement;
			graphicsManager.TmpSettings.VSynch = VSynch.isOn;
			graphicsManager.TmpSettings.AAQuality = (GraphicsSettingsData.EAASettings)AA.SelectedElement;
			graphicsManager.TmpSettings.SSAO = SSAO.isOn;
			graphicsManager.TmpSettings.MotionBlur = MotionBlur.isOn; ;
			graphicsManager.TmpSettings.Bloom = Bloom.isOn;
			//graphicsManager.TmpSettings.UIScale = UIScale.value;            
		}

		void UpdateUI()
		{
			int index = 0;
			var res = graphicsManager.ScreenResolutions.Where(x => x.width == graphicsManager.TmpSettings.ScreenWidth && x.height == graphicsManager.TmpSettings.ScreenHeight).ToArray();
			if (res.Length > 0)
			{
				index = graphicsManager.ScreenResolutions.IndexOf(res[0]);
			}
			Resolution.SelectValue(index);
			Fullscreen.isOn = graphicsManager.TmpSettings.Fullscreen;
			ShadowQuality.SelectValue((int)graphicsManager.TmpSettings.ShadowQuality);
			VSynch.isOn = graphicsManager.TmpSettings.VSynch;
			AA.SelectValue((int)graphicsManager.TmpSettings.AAQuality);
			SSAO.isOn = graphicsManager.TmpSettings.SSAO;
			MotionBlur.isOn = graphicsManager.TmpSettings.MotionBlur;
			Bloom.isOn = graphicsManager.TmpSettings.Bloom;
			//UIScale.value = graphicsManager.TmpSettings.UIScale;

		}
	}
}