using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class ModularMeshFactory : MonoBehaviour
	{
		public enum EOverlayMode { Layer, Multiply, Overlay }
		public enum EMaskMode { None, MaskOverlay, MaskApplication }
		[SerializeField] protected CombinedMesh latestCombinedMesh;

		public int TextureDownsampling = 0;
		const int RETURN_COUNT = 256 * 64;

		protected IEnumerator AddTexture(Color32[] colors, MeshTextureSetting setting, Texture2D texture, int textureWidth, EMaskMode alphaMask)
		{

			Color32[] cols = texture.GetPixels32();
			Color col;
			float alpha;

			float downsampleFactor = 1f / Mathf.Max(1, TextureDownsampling + 1);
			int steps = Mathf.Max(1, TextureDownsampling + 1);

			int counter = 0;
			for (int x = 0; x < texture.width; x++)
			{
				for (int y = 0; y < texture.height; y++)
				{
					var yOff = (int)((y + setting.offset.Y) * downsampleFactor) * textureWidth;
					var xOff = (int)((x + setting.offset.X) * downsampleFactor);
					int colorID = yOff + xOff;

					if (counter++ > RETURN_COUNT)
					{
						counter = 0;
						yield return null;
					}

					col = cols[y * texture.width + x];
					alpha = col.a;
					if (alphaMask == EMaskMode.MaskApplication)
						colors[colorID] = Color.Lerp(colors[colorID], col, alpha);
					else
						colors[colorID] = col;
				}
			}
		}


		protected IEnumerator AddTexture(Color32[] colors, MeshTextureSetting setting, Texture2D texture, int textureWidth, EOverlayMode mode, Color color, EMaskMode alphaMask)
		{
			float downsampleFactor = 1f / Mathf.Max(1, TextureDownsampling + 1);
			int steps = Mathf.Max(1, TextureDownsampling + 1);

			Color32[] cols = texture.GetPixels32();
			Color col;
			float alpha;
			int counter = 0;
			for (int x = 0; x < texture.width; x += steps)
			{
				for (int y = 0; y < texture.height; y += steps)
				{
					var yOff = (int)((y + setting.offset.Y) * downsampleFactor) * textureWidth;
					var xOff = (int)((x + setting.offset.X) * downsampleFactor);
					int colorID = yOff + xOff;

					if (counter++ > RETURN_COUNT)
					{
						counter = 0;
						yield return null;
					}

					col = cols[y * texture.width + x];
					alpha = col.a;
					switch (mode)
					{
						case EOverlayMode.Layer:
							break;
						case EOverlayMode.Multiply:
							if (alphaMask == EMaskMode.MaskOverlay)
								col = Color.Lerp(col, color * col, alpha);
							else
								col = color * col;
							break;
						case EOverlayMode.Overlay:
							if (alphaMask == EMaskMode.MaskOverlay)
								col = Color.Lerp(col, GetOverlayColor(color, col), alpha);
							else
								col = GetOverlayColor(color, col);
							break;
						default:
							col = colors[colorID];
							break;
					}

					if (alphaMask == EMaskMode.MaskApplication)
						colors[colorID] = Color.Lerp(colors[colorID], col, alpha);
					else
						colors[colorID] = col;
				}
			}
		}

		Color GetOverlayColor(Color lower, Color upper, bool overlayAlpha = false)
		{
			Color col = lower;
			col.r = OverlayValue(lower.r, upper.r);
			col.g = OverlayValue(lower.g, upper.g);
			col.b = OverlayValue(lower.b, upper.b);
			col.a = overlayAlpha ? OverlayValue(lower.a, upper.a) : lower.a;
			return col;
		}

		float OverlayValue(float lower, float upper)
		{
			float val;
			if (lower >= 0.5f)
			{
				val = (1 - lower) / 0.5f;
				var min = lower - (1 - lower);
				return (upper * val) + min;
			}
			else
			{

				val = lower / 0.5f;
				return upper * val;
			}
		}


		protected void ApplySkinnedData(Mesh baseMesh, Mesh skinnedMesh)
		{
			List<Matrix4x4> bindposes = new List<Matrix4x4>();
			skinnedMesh.GetBindposes(bindposes);
			baseMesh.bindposes = bindposes.ToArray();
		}

		protected void AddMesh(Mesh baseMesh, Mesh addMesh, MeshTextureSetting setting)
		{
			int totalVerts = baseMesh.vertexCount + addMesh.vertexCount;
			int totalTris = baseMesh.triangles.Length + addMesh.triangles.Length;
			int startIndex = baseMesh.vertexCount;
			int startTris = baseMesh.triangles.Length;

			int[] tris = new int[totalTris];
			List<Vector3> verts = new List<Vector3>(totalVerts);
			List<Vector3> norms = new List<Vector3>(totalVerts);
			List<Vector2> uv1s = new List<Vector2>(totalVerts);
			//List<Vector2> uv2s = new List<Vector2>(totalVerts);

			baseMesh.triangles.CopyTo(tris, 0);
			//Offset tirangle strip of new mesh to match new vertex indecies
			var addTris = addMesh.triangles;
			for (int i = 0; i < addTris.Length; i++)
			{
				tris[startTris + i] = addTris[i] + startIndex;
			}

			verts.AddRange(baseMesh.vertices);
			verts.AddRange(addMesh.vertices);
			norms.AddRange(baseMesh.normals);
			norms.AddRange(addMesh.normals);
			uv1s.AddRange(baseMesh.uv);
			Vector2 uv;
			//Add UVs and offset according to texture setting
			var addUVs = addMesh.uv;
			for (int i = 0; i < addMesh.vertexCount; i++)
			{
				uv = addUVs[i];
				uv.x = setting.offsetf.x + uv.x * setting.xScale;
				uv.y = setting.offsetf.y + uv.y * setting.yScale;
				uv1s.Add(uv);
			}
			//uv2s.AddRange(baseMesh.uv2);
			//uv2s.AddRange(addMesh.uv2);

			//Apply values
			baseMesh.SetVertices(verts);
			baseMesh.SetNormals(norms);
			baseMesh.SetUVs(0, uv1s);
			//baseMesh.SetUVs(1,uv2s);
			baseMesh.SetTriangles(tris, 0);

			baseMesh.RecalculateBounds();
		}

		protected void AddSkinnedMesh(Mesh baseMesh, Mesh addMesh, MeshTextureSetting setting)
		{
			int totalVerts = baseMesh.vertexCount + addMesh.vertexCount;
			int totalTris = baseMesh.triangles.Length + addMesh.triangles.Length;
			int startIndex = baseMesh.vertexCount;
			int startTris = baseMesh.triangles.Length;

			int[] tris = new int[totalTris];
			List<Vector3> verts = new List<Vector3>(totalVerts);
			List<Vector3> norms = new List<Vector3>(totalVerts);
			List<Vector2> uv1s = new List<Vector2>(totalVerts);
			//List<Vector2> uv2s = new List<Vector2>(totalVerts);
			List<BoneWeight> boneWeights = new List<BoneWeight>();

			baseMesh.triangles.CopyTo(tris, 0);
			//Offset tirangle strip of new mesh to match new vertex indecies
			var addTris = addMesh.triangles;
			for (int i = 0; i < addTris.Length; i++)
			{
				tris[startTris + i] = addTris[i] + startIndex;
			}

			verts.AddRange(baseMesh.vertices);
			verts.AddRange(addMesh.vertices);
			norms.AddRange(baseMesh.normals);
			norms.AddRange(addMesh.normals);
			uv1s.AddRange(baseMesh.uv);
			//Skinning
			boneWeights.AddRange(baseMesh.boneWeights);
			boneWeights.AddRange(addMesh.boneWeights);

			if (baseMesh.boneWeights.Length != baseMesh.vertexCount) Debug.LogError("Base mesh (" + baseMesh.name + ") weights do not match: " + baseMesh.boneWeights.Length + " / " + baseMesh.vertexCount);
			if (addMesh.boneWeights.Length != addMesh.vertexCount) Debug.LogError("Add mesh (" + addMesh.name + ") weights do not match: " + addMesh.boneWeights.Length + " / " + addMesh.vertexCount);

			Vector2 uv;
			//Add UVs and offset according to texture setting
			var addUVs = addMesh.uv;
			for (int i = 0; i < addMesh.vertexCount; i++)
			{
				uv = addUVs[i];
				uv.x = setting.offsetf.x + uv.x * setting.xScale;
				uv.y = setting.offsetf.y + uv.y * setting.yScale;
				uv1s.Add(uv);
			}
			//uv2s.AddRange(baseMesh.uv2);
			//uv2s.AddRange(addMesh.uv2);

			//Apply values
			baseMesh.SetVertices(verts);
			baseMesh.SetNormals(norms);
			baseMesh.SetUVs(0, uv1s);
			//baseMesh.SetUVs(1,uv2s);
			baseMesh.SetTriangles(tris, 0);

			//Skinning
			var weights = boneWeights.ToArray();
			if (weights.Length != verts.Count)
				Debug.LogError("Bone weights are out of bounds: " + weights.Length + " / " + verts.Count);
			else
				baseMesh.boneWeights = weights;

			baseMesh.RecalculateBounds();
		}

		[Serializable]
		public class CombinedMesh
		{
			public Texture2D texture;
			public Mesh mesh;
		}

		[Serializable]
		public struct MeshTextureSetting
		{
			public int width;
			public int height;

			public Vector2i offset;
			[NonSerialized] public Vector2 offsetf;
			public float xScale;
			public float yScale;

			public void Setup(int fullwidth, int fullheight)
			{
				xScale = (float)width / fullwidth;
				yScale = (float)height / fullheight;
				offsetf = new Vector2(offset.X / (float)fullwidth, offset.Y / (float)fullheight);
			}
		}
	}
}