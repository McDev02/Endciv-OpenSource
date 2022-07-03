using System;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// General related settings data structure.
	/// </summary>
	[Serializable]
	public class GeneralSettingsDataBase : ISaveable
	{
		public string Setting;
		public float uiScale;
		public bool mouseEdgeScroll;
		public float mouseSensitivity;
		public bool camMoveInvertX;
		public bool camMoveInvertY;
		public float mouseRotationSensitivity;
		public bool camRotateInvertX;
		public bool camRotateInvertY;
		public int saveIterationInterval;
		public bool degreeFahrenheit;

		public string language;

		public ISaveable CollectData()
		{
			if (Main.Instance.generalSettingsManager.TmpSettings == null)
				GetDataFrom(Main.Instance.generalSettingsManager.GetTemplateData());
			else
				GetDataFrom(Main.Instance.generalSettingsManager.TmpSettings);
			return this;
		}

		public GeneralSettingsDataBase() { }
		public GeneralSettingsDataBase(GeneralSettingsDataBase other)
		{
			GetDataFrom(other);
		}
		public void GetDataFrom(GeneralSettingsDataBase other)
		{
			if (other == null)
				return;
			Setting = other.Setting;
			uiScale = other.uiScale <= 0 ? 0.5f : other.uiScale;

			language = other.language;

			mouseEdgeScroll = other.mouseEdgeScroll;
			mouseSensitivity = other.mouseSensitivity <= 0 ? 1 : other.mouseSensitivity;
			camMoveInvertX = other.camMoveInvertX;
			camMoveInvertY = other.camMoveInvertY;

			mouseRotationSensitivity = other.mouseRotationSensitivity <= 0 ? 1 : other.mouseRotationSensitivity;
			camRotateInvertX = other.camRotateInvertX;
			camRotateInvertY = other.camRotateInvertY;

			saveIterationInterval = other.saveIterationInterval;
			degreeFahrenheit = other.degreeFahrenheit;
		}

		public GeneralSettingsDataBase GetCopy()
		{
			return new GeneralSettingsDataBase(this);
		}
	}
}