using UnityEngine;
using System.Collections.Generic;
using System;

namespace McMeshMerger
{
	[DisallowMultipleComponent]
	public class MeshMergeRoot : MonoBehaviour
	{
		public string meshName;
		public List<ModelData> models;
		public Mesh mergedMesh;
		public float constructionFade = 1;
		public float constructionHeight = 1;
	}
}