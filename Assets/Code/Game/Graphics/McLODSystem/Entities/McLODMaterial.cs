using UnityEngine;
using System;

namespace McLOD
{
	public class McLODMaterial : McLODEntity<McLODSettingMaterial>
	{
		[SerializeField] MeshRenderer MeshRenderer;

		void Awake()
		{
			if (MeshRenderer == null)
				MeshRenderer = GetComponent<MeshRenderer>();
		}

		protected override void ApplyLODState(McLODSettingMaterial settings)
		{
			MeshRenderer.sharedMaterial = settings.Material;
		}
	}
	[Serializable]
	public class McLODSettingMaterial : McLODSetting
	{
		public Material Material;
	}
}