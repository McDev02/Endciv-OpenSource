using System;
using System.Collections.Generic;
using UnityEngine;

namespace McMeshMerger
{
	public class MeshMerger
	{
		static List<int> tris = new List<int>();
		static List<Vector3> verts = new List<Vector3>();
		static List<Vector3> norms = new List<Vector3>();
		static List<Vector3> tans = new List<Vector3>();
		static List<Vector2> uv1s = new List<Vector2>();
		//static List<Vector2> uv2s = new List<Vector2>(totalVerts);
		static List<Color> cols = new List<Color>();

		public static Mesh MergeMeshes(MeshMergeRoot root, List<MaterialData> materials)
		{
			var baseMesh = root.mergedMesh;
			if (baseMesh == null)
				baseMesh = new Mesh();

			baseMesh.name = root.name;

			int vertexIndex = 0;

			tris.Clear();
			verts.Clear();
			norms.Clear();
			tans.Clear();
			uv1s.Clear();
			//uv2s.Clear();
			cols.Clear();

			var rootMatx = root.transform.worldToLocalMatrix;

			for (int i = 0; i < root.models.Count; i++)
			{
				var model = root.models[i];
				var addMesh = model.mesh;
				var material = materials[model.materialID];
				var matx = model.transform.localToWorldMatrix;

				var addTris = addMesh.GetTriangles(0);
				for (int t = 0; t < addTris.Length; t++)
					tris.Add(vertexIndex + addTris[t]);

				List<Vector3> addVerts = new List<Vector3>(addMesh.vertexCount);
				List<Vector3> addNorms = new List<Vector3>(addMesh.vertexCount);
				List<Vector3> addTans = new List<Vector3>(addMesh.vertexCount);
				addMesh.GetVertices(addVerts);
				addMesh.GetNormals(addNorms);
				//Transform vertecies
				for (int v = 0; v < addVerts.Count; v++)
				{
					verts.Add(rootMatx.MultiplyPoint(matx.MultiplyPoint(addVerts[v])));
					norms.Add(rootMatx.MultiplyVector(matx.MultiplyVector(addNorms[v])));
				}

				model.mergedMeshIndicies = addVerts.Count;
				Color vColor = GetColor(model);

				vertexIndex += addMesh.vertexCount;
				Vector2 uv;
				//Add UVs and offset according to texture setting
				var addUVs = addMesh.uv;
				var rect = material.rectRelative;
				for (int v = 0; v < addMesh.vertexCount; v++)
				{
					uv = addUVs[v];
					//Stick UVs between 0-1 This is required, it means that there should always be a padding in the UV, otherwise artifacts can occur, as values of 1 will result in 0
					uv.x = (uv.x + 10) % 1;
					uv.y = (uv.y + 10) % 1;

					uv.x = rect.min.x + uv.x * rect.width;
					uv.y = rect.min.y + uv.y * rect.height;
					uv1s.Add(uv);
					cols.Add(vColor);
				}
				//uv2s.AddRange(addMesh.uv2);
			}

			//Apply values
			baseMesh.SetVertices(verts);
			baseMesh.SetNormals(norms);
			//baseMesh.SetTangents(tans);
			baseMesh.SetUVs(0, uv1s);
			//baseMesh.SetUVs(1,uv2s);
			baseMesh.SetColors(cols);

			baseMesh.SetTriangles(tris, 0);

			baseMesh.RecalculateBounds();
			baseMesh.RecalculateTangents();

			return baseMesh;
		}
		/// <summary>
		/// this method only works when no change was made to the mesh or the order and content of the list models
		/// </summary>
		public static void MeshUpdateVertexColor(Mesh mesh, List<ModelData> models)
		{
			cols.Clear();

			for (int i = 0; i < models.Count; i++)
			{
				var model = models[i];
				var col = GetColor(model);
				for (int j = 0; j < model.mergedMeshIndicies; j++)
				{
					cols.Add(col);
				}
			}

			mesh.SetColors(cols);
		}

		static Color GetColor(ModelData m)
		{
			var begin = (m.constructionBegin);
			var diff = Mathf.Max(0.01f, m.constructionEnd - begin);
			//End Offset							Scale			Height				Dissolve
			return new Color(1f - m.constructionEnd, 1f / diff, m.constructionHeight, m.dissolve ? 1 : 0);
		}
	}
}