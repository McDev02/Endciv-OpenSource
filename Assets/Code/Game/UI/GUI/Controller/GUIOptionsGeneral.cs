using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUIOptionsGeneral : GUIAnimatedPanel
	{
		GeneralSettingsManager generalSettingsManager;

		[SerializeField] Text uiScaleLbl;
		[SerializeField] Slider UIScale;
		[SerializeField] Slider MouseSensitivity;
		[SerializeField] Toggle CamMoveInvertX;
		[SerializeField] Toggle CamMoveInvertY;
		[SerializeField] Slider MouseRotationSensitivity;
		[SerializeField] Toggle CamRotateInvertX;
		[SerializeField] Toggle CamRotateInvertY;
		[SerializeField] UIDropdown UISaveInterval;
		[SerializeField] UIDropdown DegreeDropdown;

		string currentLanguageID = "en_us";
		string CurrentLanguageID
		{
			get { return currentLanguageID; }
			set
			{
				currentLanguageID = value;
				UpdateLanguageButtons();
			}
		}

		[SerializeField] LanguageButton[] languageButtons;

		const float UISliderSteps = 0.125f;
		const float UISliderMin = 0.5f;
		const float UISliderMax = 1.5f;

		[Serializable]
		public struct LanguageButton
		{
			public string langID;
			public Button button;
		}

		public void Setup(GeneralSettingsManager generalSettingsManager)
		{
			Debug.Log("Assigned manager");
			this.generalSettingsManager = generalSettingsManager;
			PrepareElements();
			UpdateUI();

			MouseSensitivity.minValue = 0.2f;
			MouseSensitivity.maxValue = 3;

			MouseRotationSensitivity.minValue = 0.1f;
			MouseRotationSensitivity.maxValue = 2f;

			UIScale.minValue = UISliderMin / 2f;
			UIScale.maxValue = UISliderMax / 2f;
		}

		private void UpdateLanguageButtons()
		{
			for (int i = 0; i < languageButtons.Length; i++)
			{
				languageButtons[i].button.interactable = languageButtons[i].langID != currentLanguageID;
			}
		}

		void PrepareElements()
		{
			var intervalNames = System.Enum.GetNames(typeof(GeneralSettingsManager.ESaveInterval));

			for (int i = 0; i < intervalNames.Length; i++)
			{
				var name = intervalNames[i];
				UISaveInterval.AddOption(name.Substring(1, name.Length - 1) + " mins", i > 0);
				i++;
			}

			DegreeDropdown.AddOption("°C");
			DegreeDropdown.AddOption("°F");
		}

		private void OnEnable()
		{
			if (generalSettingsManager == null)
				return;
			UpdateUI();
			OnUpdateSettings();
		}

		public void SnapSliderValue()
		{
			UIScale.value = Mathf.Round(UIScale.value / UISliderSteps) * UISliderSteps;
			uiScaleLbl.text = Mathf.Clamp(UIScale.value * 2, UISliderMin, UISliderMax).ToString("0.##");
			OnUpdateSettings();
		}

		public void ChangeLanguage(string lang)
		{
			CurrentLanguageID = lang;
		}

		public void OnUpdateSettings()
		{
			generalSettingsManager.UIScale = UIScale.value * 2f;
			generalSettingsManager.UIScaleInv = 1f / UIScale.value * 2f;

			generalSettingsManager.mouseSensitivity = Mathf.Pow(MouseSensitivity.value, 3f);
			generalSettingsManager.camMoveInvertX = CamMoveInvertX.isOn;
			generalSettingsManager.camMoveInvertY = CamMoveInvertY.isOn;

			generalSettingsManager.mouseRotationSensitivity = Mathf.Pow(MouseRotationSensitivity.value, 2f);
			generalSettingsManager.camRotateInvertX = CamRotateInvertX.isOn;
			generalSettingsManager.camRotateInvertY = CamRotateInvertY.isOn;

			generalSettingsManager.SaveInterval = UISaveInterval.SelectedElement;
			generalSettingsManager.degreeFahrenheit = DegreeDropdown.SelectedElement == 1;
			generalSettingsManager.LanguageID = currentLanguageID;
		}

		public void OnPresetChanged()
		{
			OnUpdateSettings();
		}

		public void DiscardValues()
		{
			//Set everything back to before changes were made
			Main.Instance.generalSettingsManager.DiscardTemporaryValues();
			OnUpdateSettings();
			UpdateUI();
		}

		public void ApplyValues()
		{
			//Apply temporary changes and write to disk.
			UpdateTempSettings();
			Main.Instance.generalSettingsManager.ApplyTemporaryValues(false);
			OnUpdateSettings();
		}

		void UpdateTempSettings()
		{
			generalSettingsManager.TmpSettings.uiScale = UIScale.value * 2f;
			generalSettingsManager.TmpSettings.language = currentLanguageID;
			generalSettingsManager.TmpSettings.mouseEdgeScroll = true;

			generalSettingsManager.TmpSettings.mouseSensitivity = MouseSensitivity.value;
			generalSettingsManager.TmpSettings.camMoveInvertX = CamMoveInvertX.isOn;
			generalSettingsManager.TmpSettings.camMoveInvertY = CamMoveInvertY.isOn;

			generalSettingsManager.TmpSettings.mouseRotationSensitivity = MouseRotationSensitivity.value;
			generalSettingsManager.TmpSettings.camRotateInvertX = CamRotateInvertX.isOn;
			generalSettingsManager.TmpSettings.camRotateInvertY = CamRotateInvertY.isOn;

			generalSettingsManager.TmpSettings.saveIterationInterval = UISaveInterval.SelectedElement;
			generalSettingsManager.TmpSettings.degreeFahrenheit = DegreeDropdown.SelectedElement == 1;
		}

		void UpdateUI()
		{
			if (generalSettingsManager.TmpSettings == null)
				return;
			UIScale.value = generalSettingsManager.TmpSettings.uiScale / 2f;
			float disp = Mathf.Clamp(UIScale.value * 2f, UISliderMin, UISliderMax);
			uiScaleLbl.text = disp.ToString("0.##");
			//Add EdgeScroll Element generalSettingsManager.TmpSettings.mouseEdgeScroll;

			var t = generalSettingsManager.TmpSettings;
			MouseSensitivity.value = t.mouseSensitivity;
			CamMoveInvertX.isOn = t.camMoveInvertX;
			CamMoveInvertY.isOn = t.camMoveInvertY;

			MouseRotationSensitivity.value = t.mouseRotationSensitivity;
			CamRotateInvertX.isOn = t.camRotateInvertX;
			CamRotateInvertY.isOn = t.camRotateInvertY;

			UISaveInterval.SelectValue(t.saveIterationInterval);
			DegreeDropdown.SelectValue(t.degreeFahrenheit ? 1 : 0);
			CurrentLanguageID = t.language;
		}
	}
}