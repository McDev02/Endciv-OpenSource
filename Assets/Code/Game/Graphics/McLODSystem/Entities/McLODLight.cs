using UnityEngine;
using System;

namespace McLOD
{
	public class McLODLight : McLODEntity<McLODSettingLight>
	{
		[SerializeField] Light LightSource;

		void Awake()
		{
			if (LightSource == null) LightSource = GetComponent<Light>();
		}

		protected override void ApplyLODState(McLODSettingLight settings)
		{
			LightSource.enabled = settings.Enabled;
			LightSource.color = settings.DebugColor;
			LightSource.shadows = settings.Shadows;
			LightSource.shadowResolution = settings.ShadowResolution;
		}

	}
	[Serializable]
	public class McLODSettingLight : McLODSetting
	{
		public bool Enabled;
		public LightShadows Shadows;
		public UnityEngine.Rendering.LightShadowResolution ShadowResolution;
	}
}