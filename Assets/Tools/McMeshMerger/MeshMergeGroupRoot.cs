using UnityEngine;
using System.Collections.Generic;
using System;

namespace McMeshMerger
{
	[DisallowMultipleComponent]
	public class MeshMergeGroupRoot : MonoBehaviour
	{
		public string groupName;
		public string dataPath;

		public Texture2D mergedDiffuse;
		public Texture2D mergedSurface;
		public Texture2D mergedNormal;
		public Texture2D mergedGlow;

		public Material mergedMatetrial;

		public MergerToolSettings mergerToolSettings;

		public List<MaterialData> uniqueMaterials = new List<MaterialData>();
	
		public string GetPath()
		{
			return $"{dataPath}/{ groupName}/";
		}
	}
}