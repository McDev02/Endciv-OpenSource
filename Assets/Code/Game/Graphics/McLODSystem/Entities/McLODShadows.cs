using UnityEngine;
using System;

namespace McLOD
{
	public class McLODShadows : McLODEntity<McLODSettingShadows>
	{
		[SerializeField] MeshRenderer MeshRenderer;

		void Awake()
		{
			if (MeshRenderer == null) MeshRenderer = GetComponentInChildren<MeshRenderer>();
		}

		protected override void ApplyLODState(McLODSettingShadows settings)
		{
			MeshRenderer.shadowCastingMode = settings.ShadowMode;
			MeshRenderer.enabled = !settings.cull;
		}

	}
	[Serializable]
	public class McLODSettingShadows : McLODSetting
	{
		public UnityEngine.Rendering.ShadowCastingMode ShadowMode;
	}
}