using UnityEngine;
using System;

namespace McLOD
{
	public class McLODMeshFull : McLODEntity<McLODSettingFull>
	{
		[SerializeField] MeshFilter MeshFilter;
		[SerializeField] MeshRenderer MeshRenderer;

		void Awake()
		{
			if (MeshFilter == null) MeshFilter = GetComponent<MeshFilter>();
			if (MeshRenderer == null) MeshRenderer = GetComponent<MeshRenderer>();
		}

		protected override void ApplyLODState(McLODSettingFull settings)
		{
			MeshRenderer.sharedMaterial = settings.Material;
			MeshFilter.sharedMesh = settings.Mesh;
			MeshRenderer.shadowCastingMode = settings.ShadowMode;
		}

	}
	[Serializable]
	public class McLODSettingFull : McLODSettingMeshRendering
	{
		public Material Material;
	}
}