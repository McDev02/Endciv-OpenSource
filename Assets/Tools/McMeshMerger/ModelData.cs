using UnityEngine;
using System.Collections.Generic;
using System;

namespace McMeshMerger
{
	[DisallowMultipleComponent]
	public class ModelData : MonoBehaviour
	{
		[NonSerialized] public int id;
		public Transform transform { get { return renderer.transform; } }
		[NonSerialized] public MeshFilter filter;
		[NonSerialized] public MeshRenderer renderer;
		[NonSerialized] public Mesh mesh;
		public float constructionBegin = 0;
		public float constructionEnd = 1;
		public float constructionHeight;
		public bool dissolve;
		public float randomMin = 0;
		public float randomMax = 1;

		[NonSerialized] public int materialID;
		[NonSerialized] public int mergedMeshIndicies;
	}
}