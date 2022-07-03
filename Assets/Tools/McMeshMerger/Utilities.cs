using System;
using System.Collections.Generic;
using UnityEngine;

namespace McMeshMerger
{

	[Serializable]
	public class MergerToolSettings
	{
		public int minTexturCellSize = 8;

		public int targetWidth = 512;
		public int targetHeight = 512;

		public bool createDiffuseTexture = true;
		public bool includeAlphaChannel = true;
		public bool createSpecularTexture = true;
		public bool createNormalTexture = true;
		public bool createGlowTexture = false;
	}

	public class MaterialData
	{
		public int id;
		public Material material;
		public int width;
		public int height;
		public int bakeWidth { get { return Mathf.Max(1, (int)(textureScale * width)); } }
		public int bakeHeight { get { return Mathf.Max(1, (int)(textureScale * height)); } }

		public Texture2D diffuseTex;
		public Texture2D surfaceTex;
		public Texture2D normalTex;
		public Texture2D glowTex;

		public float textureScale = 1;
		public Rect rect;
		public Rect rectRelative;
		public bool hasTexture;

		public MaterialData(int materialID, Material mat, int minWidth, int minHeight)
		{
			id = materialID;
			material = mat;

			diffuseTex = (Texture2D)mat.GetTexture(Utilities.MAP_DIFF);
			surfaceTex = (Texture2D)mat.GetTexture(Utilities.MAP_SURF);
			normalTex = (Texture2D)mat.GetTexture(Utilities.MAP_NORM);
			glowTex = (Texture2D)mat.GetTexture(Utilities.MAP_GLOW);
			textureScale = 1;

			hasTexture = diffuseTex != null || surfaceTex != null || normalTex != null || glowTex != null;
			if (!hasTexture)
			{
				width = minWidth;
				height = minHeight;
			}
			else
			{
				var biggest = Utilities.GetBiggestTexture(new Texture2D[] { diffuseTex, surfaceTex, normalTex, glowTex });
				width = biggest.width;
				height = biggest.height;
			}
			rect = new Rect(0, 0, width, height);
			rectRelative = new Rect(0, 0, 1, 1);
		}
	}


	public static class Utilities
	{
		public const string MAP_DIFF = "_MainTex";
		public const string MAP_SURF = "_SurfaceMap";
		public const string MAP_NORM = "_BumpMap";
		public const string MAP_GLOW = "_EmissionMap";

		/// <summary>
		/// Returns the objects which are highest in the hirachy of all selected ones. If you select one parent and all its children, it will return the parent only.
		/// </summary>
		public static List<GameObject> SelectRootObjects(GameObject[] list)
		{

			List<GameObject> roots = new List<GameObject>(list.Length);
			//Copy list
			for (int i = 0; i < list.Length; i++)
			{
				roots.Add(list[i]);
			}

			//Sort out all children
			for (int d = 0; d < list.Length; d++)
			{
				for (int i = 0; i < roots.Count; i++)
				{
					var parent = roots[i].transform.parent;
					if (parent == null) continue;

					if (list[d].transform == parent)
					{
						roots.RemoveAt(i);
						i--;
					}
				}
			}

			return roots;
		}

		public static List<Transform> SelectChildren(Transform obj)
		{
			List<Transform> list = new List<Transform>();

			for (int i = 0; i < obj.childCount; i++)
			{
				list.Add(obj.GetChild(i));
			}

			return list;
		}

		public static T GetBiggestTexture<T>(T[] textures) where T : Texture
		{
			T texture = null;
			int biggestSize = 0;
			for (int i = 0; i < textures.Length; i++)
			{
				if (textures[i] == null) continue;
				var size = textures[i].width * textures[i].height;
				if (size > biggestSize)
				{
					biggestSize = size;
					texture = textures[i];
				}
			}
			return texture;
		}
	}
}
