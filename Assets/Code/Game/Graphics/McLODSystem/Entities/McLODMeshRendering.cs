using UnityEngine;
using System;

namespace McLOD
{
	public class McLODMeshRendering : McLODEntity<McLODSettingMeshRendering>
	{
		[SerializeField] MeshFilter MeshFilter;
		[SerializeField] MeshRenderer MeshRenderer;

		void Awake()
		{
			if (MeshFilter == null) MeshFilter = GetComponent<MeshFilter>();
			if (MeshRenderer == null) MeshRenderer = GetComponent<MeshRenderer>();
		}

		protected override void ApplyLODState(McLODSettingMeshRendering settings)
		{
			MeshFilter.sharedMesh = settings.Mesh;
			MeshRenderer.shadowCastingMode = settings.ShadowMode;
			MeshRenderer.enabled = settings.cull;
		}

	}
	[Serializable]
	public class McLODSettingMeshRendering : McLODSettingMesh
	{
		public UnityEngine.Rendering.ShadowCastingMode ShadowMode;
	}
}