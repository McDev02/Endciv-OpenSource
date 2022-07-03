using UnityEngine;
using System;

namespace McLOD
{
	public class McLODMesh : McLODEntity<McLODSettingMesh>
	{
		[SerializeField] MeshFilter MeshFilter;
		[SerializeField] MeshRenderer MeshRenderer;

		void Awake()
		{
			if (MeshFilter == null) MeshFilter = GetComponent<MeshFilter>();
			if (MeshRenderer == null) MeshRenderer = GetComponent<MeshRenderer>();
		}

		protected override void ApplyLODState(McLODSettingMesh settings)
		{
			MeshFilter.sharedMesh = settings.Mesh;
		}
	}
	[Serializable]
	public class McLODSettingMesh : McLODSetting
	{
		public Mesh Mesh;
	}
}