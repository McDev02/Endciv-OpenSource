using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{

	public class TerrainFactory : MonoBehaviour
	{
		GridMap gridMap;
		public enum EShaderQuality { LOW, MEDIUM, HIGH }
		EShaderQuality m_ShaderQuality;

		public Texture2D m_GridTexture;
		public Texture2D m_LayerMask;
		RectanglePacker m_RectPacker;
		int m_Patches;
		int m_PatchSize;
		[SerializeField] Material terrainMaterial = null;
		[NonSerialized] public Material CurrentTerrainMaterial;
		public Material[] m_LayerMaterials = new Material[32];
		TerrainPatch[,] m_TerrainPatches;
		Transform[,] m_PatchContainer;
		Transform m_UnusedPatchContainer;
		bool m_TerrainPatchesDirty;

		Stack<TerrainMeshObject> m_MeshObjectPool;
		public GameObject m_TerrainPatchPrefab;

		[NonSerialized] public TerrainSettings Settings;

		public void Setup(GridMap gridMap)
		{
			this.gridMap = gridMap;
		}

		public void ConvertData(TerrainExchangeData terrain, bool loadSavegame = false)
		{
			TerrainData terrainData = new TerrainData(gridMap.View.VertexWidth, gridMap.View.VertexLength);

			for (int x = 0; x < gridMap.View.VertexWidth; x++)
			{
				for (int y = 0; y < gridMap.View.VertexLength; y++)
				{
					float height = terrain.Height[x, y];

					terrainData.Position[x, y] = gridMap.View.LocalToWorld(new Vector2(x, y));
					terrainData.Height[x, y] = height;
				}
			}

			for (int x = 0; x < gridMap.View.VertexWidth; x++)
			{
				for (int y = 0; y < gridMap.View.VertexLength; y++)
				{
					Vector3 pos = terrainData.Position[x, y];

					float height = terrainData.Height[x, y] * 2;
					int count = 2;
					Vector3[] positions = new Vector3[4];
					Vector2i index = new Vector2i(x, y);
					Vector2i ind;


					for (int i = 0; i < 4; i++)
					{
						ind = index + DirectionHelper.DirectionVectors[2 * i];
						if (gridMap.Grid.VertexInRange(ind))
						{
							positions[i] = terrainData.Position[ind.X, ind.Y];
							height += terrainData.Height[ind.X, ind.Y];
							count++;
						}
						else
							positions[i] = pos;
					}

					terrainData.SmoothHeight[x, y] = height / (float)count;

					Vector3 tangent = ((pos - positions[1]) + (positions[3] - pos)).normalized;
					Vector3 binormal = ((pos - positions[0]) + (positions[2] - pos)).normalized;

					const float smoothness = 0.8f;
					tangent = Vector3.Lerp(tangent, Vector3.right, smoothness).normalized;
					binormal = Vector3.Lerp(binormal, Vector3.forward, smoothness).normalized;
					Vector3 normal = Vector3.Cross(tangent, -binormal).normalized;

					terrainData.Normal[x, y] = normal;
					terrainData.Tangent[x, y] = tangent;
				}
			}

			gridMap.Data.fertileLand = new Map2D<float>(terrain.Surfaces.fertileLand);
			gridMap.Data.waste = new Map2D<float>(terrain.Surfaces.waste);
			gridMap.TerrainData = terrainData;
            gridMap.TerrainExchangeData = terrain;
            gridMap.GenerateMipmaps();
			//GridMap.ResetTerrainTextures();
		}

		public IEnumerator CreateTerrain(TerrainSettings settings, LoadingState loadingState)
		{
			var time = DateTime.Now;
			CurrentTerrainMaterial = new Material(terrainMaterial);
			Settings = settings;
			settings.Setup();
			SetupTerrain();

			m_TerrainPatches = new TerrainPatch[m_Patches, m_Patches];
			int total = m_Patches * m_Patches;
			for (int y = 0; y < m_Patches; y++)
			{
				for (int x = 0; x < m_Patches; x++)
				{
					m_TerrainPatches[x, y] = CreateTerrainPatch(x, y);
					UpdatePatch(x, y);
					if ((DateTime.Now - time).Milliseconds >=250)
					{
						time = DateTime.Now;
						loadingState.SetMessage($"Create Terrain Patch: {(x + m_Patches * y).ToString()}/{total}");
						yield return null;
					}
				}
			}

			UpdateSplatLayers();
			Shader.SetGlobalFloat("_G_WorldMapFactor", 1f / Settings.GridWorldSize);
		}

		public void SetupTerrain()
		{
			m_MeshObjectPool = new Stack<TerrainMeshObject>();
			m_RectPacker = new RectanglePacker(gridMap.Width, gridMap.Length, 8 * 4); //dataCount = Layers * directions
			m_PatchSize = TerrainSettings.PatchResolution;
			m_Patches = Settings.TerrainPatchCount;

			m_UnusedPatchContainer = new GameObject("Unused Patches").transform;
			m_UnusedPatchContainer.parent = transform;
			m_PatchContainer = new Transform[m_Patches, m_Patches];
			for (int y = 0; y < m_Patches; y++)
			{
				for (int x = 0; x < m_Patches; x++)
				{
					string txt = "Patch (" + x.ToString("00") + ":" + y.ToString("00") + ")";

					var obj = new GameObject(txt);
					obj.transform.parent = transform;
					m_PatchContainer[x, y] = obj.transform;
				}
			}

			CurrentTerrainMaterial.DisableKeyword("BLEND_ON");
			CurrentTerrainMaterial.DisableKeyword("LAYER_ON");
			SetShaderQuality(EShaderQuality.MEDIUM);
			ShowLayerOverlay(false);
		}

		public void SetShaderQuality(EShaderQuality quality)
		{
			m_ShaderQuality = quality;
			for (int i = 0; i < m_LayerMaterials.Length; i++)
			{
				if (m_LayerMaterials[i] != null)
				{
					var mat = m_LayerMaterials[i];
					mat.DisableKeyword("QUALITY_LOW");
					mat.DisableKeyword("QUALITY_MEDIUM");
					mat.DisableKeyword("QUALITY_HIGH");

					mat.EnableKeyword("QUALITY_" + quality.ToString());
					m_LayerMaterials[i] = mat;
				}
			}

			CurrentTerrainMaterial.DisableKeyword("QUALITY_LOW");
			CurrentTerrainMaterial.DisableKeyword("QUALITY_MEDIUM");
			CurrentTerrainMaterial.DisableKeyword("QUALITY_HIGH");

			CurrentTerrainMaterial.EnableKeyword("QUALITY_" + quality.ToString());
		}

		public void ShowLayerOverlay(bool enable = true)
		{
			for (int i = 0; i < m_LayerMaterials.Length; i++)
			{
				if (m_LayerMaterials[i] != null)
				{
					var mat = m_LayerMaterials[i];
					if (enable)
						mat.EnableKeyword("LAYER_ON");
					else
						mat.DisableKeyword("LAYER_ON");
					m_LayerMaterials[i] = mat;
				}
			}

			if (enable)
				CurrentTerrainMaterial.EnableKeyword("LAYER_ON");
			else
				CurrentTerrainMaterial.DisableKeyword("LAYER_ON");
		}

		public void SetTerrainDirty(Vector2i index)
		{
			int iX1 = Mathf.Clamp((int)((index.X - 0.5f) / (float)m_PatchSize), 0, m_Patches - 1);
			int iY1 = Mathf.Clamp((int)((index.Y - 0.5f) / (float)m_PatchSize), 0, m_Patches - 1);
			int iX2 = Mathf.Clamp((int)Mathf.Ceil(index.X / (float)m_PatchSize), 0, m_Patches - 1);
			int iY2 = Mathf.Clamp((int)Mathf.Ceil(index.Y / (float)m_PatchSize), 0, m_Patches - 1);

			m_TerrainPatches[iX1, iY1].Dirty = true;
			m_TerrainPatches[iX2, iY1].Dirty = true;
			m_TerrainPatches[iX1, iY2].Dirty = true;
			m_TerrainPatches[iX2, iY2].Dirty = true;
		}

		private void UpdateSplatLayers()
		{
			var preset = Settings.Preset;
			var splatLayers = preset.Surfaces;

			for (int i = 0; i < splatLayers.Length; i++)
			{
				Shader.SetGlobalTexture("_TerrainSurface" + i.ToString(), splatLayers[i].Texture);
				Shader.SetGlobalTexture("_TerrainNormal" + i.ToString(), splatLayers[i].NormalMap);
				if (i > 0)
					Shader.SetGlobalFloat("_TerrainEdge" + (i - 1).ToString(), splatLayers[i].BlendEdge);
			}

			Shader.SetGlobalVector("_TerrainTiling", new Vector4(splatLayers[0].Tiling, splatLayers[1].Tiling, splatLayers[2].Tiling, splatLayers[3].Tiling));
			Shader.SetGlobalTexture("_TerrainSnowTex", preset.SnowSurface.Texture);
			Shader.SetGlobalTexture("_TerrainSnowNormal", preset.SnowSurface.NormalMap);
			Shader.SetGlobalFloat("_TerrainSnowTiling", preset.SnowSurface.Tiling);
			Shader.SetGlobalFloat("_TerrainSnowBlend", preset.SnowSurface.BlendEdge);
			Shader.SetGlobalTexture("_GlobalSnowNoiseTex", preset.SnowNoiseTexture);
			Shader.SetGlobalFloat("_TerrainSnowNoiseTiling", 1f / (preset.SnowNoiseTileSize * GridMapView.TileSize));
		}

		public void UpdatePatches()
		{
			for (int y = 0; y < m_Patches; y++)
			{
				for (int x = 0; x < m_Patches; x++)
				{
					var patch = m_TerrainPatches[x, y];
					if (patch.Dirty)
						UpdatePatch(x, y);
				}
			}
		}

		void Update()
		{
			if (m_TerrainPatchesDirty)
			{
				m_TerrainPatchesDirty = false;
				List<RectBounds>[] meshRects;
				for (int y = 0; y < m_Patches; y++)
				{
					for (int x = 0; x < m_Patches; x++)
					{
						var patch = m_TerrainPatches[x, y];
						if (patch.Dirty)
						{
							//Debug.Log ( "Update patch [" + x + "|" + y + "]" );
							meshRects = m_RectPacker.CalculateRectangles(ref patch.Graph);
							int id = 0;
							var meshes = patch.Meshes;
							for (int i = 0; i < meshRects.Length; i++)
							{
								//Extended DataID i
								int dataID = (int)(i * (1f / 4f));
								EDirection dir = (EDirection)(i % 4);

								for (int d = 0; d < meshRects[i].Count; d++)
								{
									//if ( Main.DEBUG_VIEW )	Keep code and change to valid DEBUG_MODE flag
									//	DrawRect ( x, y, meshRects[i][d] );
									var rect = meshRects[i][d];
									rect.X += x * TerrainSettings.PatchResolution;
									rect.Y += y * TerrainSettings.PatchResolution;
									if (meshes.Count > id)
									{
										var mesh = meshes[id];
										mesh.Mesh = CreateRectangularMesh(rect, dir, mesh.Mesh);
										mesh.Rect = rect;
										mesh.Object.name = "Created";
										mesh.DataID = dataID;
										mesh.Direction = dir;
										meshes[id] = mesh;
									}
									else
									{
										TerrainMeshObject mesh;
										if (m_MeshObjectPool.Count > 0)
										{
											mesh = m_MeshObjectPool.Pop();
											mesh.Mesh = CreateRectangularMesh(rect, dir, mesh.Mesh);
											mesh.Rect = rect;
											mesh.Object.name = "Pooled";
											mesh.DataID = dataID;
											mesh.Direction = dir;
										}
										else
											mesh = CreateNewMeshRect(dataID, rect, dir);

										mesh.Object.transform.parent = m_PatchContainer[x, y];
										mesh.Object.SetActive(true);
										meshes.Add(mesh);
									}

									id++;
								}
							}

							for (int i = meshes.Count - 1; i >= id; i--)
							{
								var mesh = meshes[i];
								mesh.Object.SetActive(false);
								mesh.Object.name = "INACTIVE";
								mesh.Object.transform.parent = m_UnusedPatchContainer;
								m_MeshObjectPool.Push(mesh);
								meshes.RemoveAt(i);
							}

							patch.Meshes = meshes;
							m_TerrainPatches[x, y] = patch;
							if (id != meshes.Count)
								UnityEngine.Debug.LogError("Meshes do not match! " + id + ":" + meshes.Count);
							UpdatePatch(x, y);
						}
					}
				}
			}
		}

		void DrawRect(int x, int y, RectBounds bounds)
		{
			Vector3 start = new Vector3(m_PatchSize * x, 0, m_PatchSize * y) * 0.5f;
			Vector3 A = start + new Vector3(bounds.X, 0, bounds.Y) * 0.5f;
			Vector3 B = start + new Vector3(bounds.X, 0, bounds.Y + bounds.Length) * 0.5f;
			Vector3 C = start + new Vector3(bounds.X + bounds.Width, 0, bounds.Y + bounds.Length) * 0.5f;
			Vector3 D = start + new Vector3(bounds.X + bounds.Width, 0, bounds.Y) * 0.5f;

			UnityEngine.Debug.DrawLine(A, B, Color.yellow, 1);
			UnityEngine.Debug.DrawLine(B, C, Color.yellow, 1);
			UnityEngine.Debug.DrawLine(C, D, Color.yellow, 1);
			UnityEngine.Debug.DrawLine(D, A, Color.yellow, 1);
		}

		public int GetLayerID(int x, int y)
		{
			return (int)(GetLayerIDRaw(x, y) * (1f / 4f)); //todo: not the best solution!
		}
		public int GetLayerIDRaw(int x, int y)
		{
			int patchX = (int)((float)x / (float)m_PatchSize);
			int patchY = (int)((float)y / (float)m_PatchSize);
			int gX = x % m_PatchSize;
			int gY = y % m_PatchSize;
			var graph = m_TerrainPatches[patchX, patchY].Graph;
			return (int)(graph.Data[gX, gY]); //todo: not the best solution!
		}

		TerrainMeshObject CreateNewMeshRect(int dataID, RectBounds Rect, EDirection dir)
		{
			TerrainMeshObject meshRect = new TerrainMeshObject();

			GameObject obj = (GameObject)GameObject.Instantiate(m_TerrainPatchPrefab);
			obj.transform.parent = transform;
			obj.layer = LayerMask.NameToLayer("Terrain");
			meshRect.Object = obj;
			var mesh = CreateRectangularMesh(Rect, dir);
			meshRect.Mesh = mesh;
			meshRect.DataID = dataID;
			meshRect.Direction = dir;
			meshRect.Rect = Rect;
			meshRect.Object.GetComponent<MeshRenderer>().sharedMaterial = GetLayerMaterial(dataID);
			return meshRect;
		}

		Material GetLayerMaterial(int dataID)
		{
			if (dataID <= 0)
				return CurrentTerrainMaterial;
			var mat = m_LayerMaterials[dataID];
			if (mat != null)
				return mat;
			else
			{
				mat = NewLayerMaterial(dataID - 1);
				m_LayerMaterials[dataID] = mat;
				return mat;
			}
		}

		Material NewLayerMaterial(int dataID)
		{
			var layer = Settings.Preset.Layers[dataID];

			Material mat = new Material(Settings.Preset.TerrainBlendMaterial);
			mat.name = "LayerMaterial " + dataID.ToString("00");
			mat.SetTexture("_GridTex", m_GridTexture);
			mat.SetTexture("_LayerMask", m_LayerMask);
			mat.SetTexture("_BlendTex", layer.Surface.Texture);
			mat.SetTexture("_BlendNormal", layer.Surface.NormalMap);
			mat.SetFloat("_BlendTiling", layer.Surface.Tiling);
			mat.SetFloat("_BlendEdge", layer.Surface.BlendEdge);
			mat.EnableKeyword("BLEND_ON");

			mat.DisableKeyword("QUALITY_LOW");
			mat.DisableKeyword("QUALITY_MEDIUM");
			mat.DisableKeyword("QUALITY_HIGH");
			mat.EnableKeyword("QUALITY_" + m_ShaderQuality.ToString());

			return mat;
		}

		void UpdatePatch(int patchX, int patchY)
		{
			//int startX = patchX * TerrainSettings.m_TerrainPatchResolution;
			//int startY = patchY * TerrainSettings.m_TerrainPatchResolution;
			var patch = m_TerrainPatches[patchX, patchY];
			var meshes = patch.Meshes;
			for (int i = 0; i < meshes.Count; i++)
			{
				var meshObj = meshes[i];
				var mesh = meshObj.Mesh;
				var dir = meshObj.Direction;
				meshObj.ResetCollider();
				meshObj.Object.transform.position = ((Vector2)TransformPatchPosition(meshObj.Rect, dir)).To3D() * GridMapView.TileSize;
				meshObj.Object.transform.rotation = Quaternion.AngleAxis((int)dir * 90, Vector3.up);

				var bounds = new Bounds();
				var verts = mesh.vertices;
				var norms = mesh.normals;
				var tans = mesh.tangents;
				var matrix = meshObj.Object.transform.localToWorldMatrix;
				for (int v = 0; v < verts.Length; v++)
				{
					Vector2 pos = gridMap.View.WorldToLocal(matrix.MultiplyPoint(verts[v]).To2D()); //Use Transform_Matrix
					var data = gridMap.TerrainData.GetMapLayerBilinear(pos.x, pos.y);
					verts[v].y = data.Height;
					bounds.Encapsulate(verts[v]);
					norms[v] = data.Normal;
					tans[v] = data.Tangent;
				}

				mesh.vertices = verts;
				mesh.normals = norms;
				mesh.tangents = tans;
				mesh.bounds = bounds;
				//mesh.RecalculateBounds ();
				mesh.RecalculateNormals();
				//CalculateMeshTangent(mesh);

				mesh.name = "Mesh " + i.ToString("000");

				meshObj.Object.name = "Mesh " + i.ToString("000");

				meshObj.Mesh = mesh;
				meshObj.Object.GetComponent<MeshRenderer>().sharedMaterial = GetLayerMaterial(meshObj.DataID);
				meshes[i] = meshObj;
			}
			patch.Dirty = false;
			patch.Meshes = meshes;
			m_TerrainPatches[patchX, patchY] = patch;
		}

		Vector2i TransformPatchPosition(RectBounds rect, EDirection dir)
		{
			switch (dir)
			{
				case EDirection.North:
					return rect.BottomLeft;
				case EDirection.East:
					return rect.TopLeft;
				case EDirection.South:
					return rect.TopRight;
				case EDirection.West:
					return rect.BottomRight;
			}

			return rect.BottomLeft;
		}

		TerrainPatch CreateTerrainPatch(int px, int py)
		{
			TerrainPatch patch = new TerrainPatch();

			float tilesize = GridMapView.TileSize;
			int startX = px * m_PatchSize;
			int startY = py * m_PatchSize;

			GameObject obj = (GameObject)GameObject.Instantiate(m_TerrainPatchPrefab, new Vector3(startX * tilesize, 0, startY * tilesize), Quaternion.identity);

			obj.transform.parent = transform;
			obj.layer = LayerMask.NameToLayer("Terrain");

			var mesh = CreateRectangularMesh(startX, startY, m_PatchSize, m_PatchSize, EDirection.North);

			obj.GetComponent<MeshFilter>().mesh = mesh;
			obj.GetComponent<MeshRenderer>().sharedMaterial = CurrentTerrainMaterial;
			obj.GetComponent<MeshCollider>().sharedMesh = mesh;

			var graph = new RectGrid(m_PatchSize, m_PatchSize); // QuadraticGraph.EConnectionMode.Perpendicular);
			patch.Graph = graph;
			patch.Rect = new RectBounds(startX, startY, startX + m_PatchSize, startY + m_PatchSize);
			patch.Meshes = new List<TerrainMeshObject>();
			TerrainMeshObject meshObj = new TerrainMeshObject();
			meshObj.Object = obj;
			meshObj.DataID = 0;
			meshObj.Rect = patch.Rect;
			meshObj.Object.transform.parent = m_PatchContainer[px, py];
			patch.Meshes.Add(meshObj);
			return patch;
		}

		internal void SetTerrainSurface(Vector2i index, int id, EDirection direction, bool dirtySurface = true)
		{
			SetTerrainSurface(index.X, index.Y, id, direction, dirtySurface);
		}

		internal void SetTerrainSurface(int x, int y, int id, EDirection direction, bool dirtySurface = true)
		{
			SetTerrainSurface(x, y, id * 4 + (int)direction, dirtySurface);
		}
		internal void SetTerrainSurface(int x, int y, int id, bool dirtySurface = true)
		{
			int patchX = (int)((float)x / (float)m_PatchSize);
			int patchY = (int)((float)y / (float)m_PatchSize);
			int gX = x % m_PatchSize;
			int gY = y % m_PatchSize;
			if (m_Patches > patchX && m_Patches > patchY)
			{
				var graph = m_TerrainPatches[patchX, patchY].Graph;

				if (graph.Data[gX, gY] != id)
				{
					m_TerrainPatches[patchX, patchY].Dirty = true;
					graph.Data[gX, gY] = id;
					m_TerrainPatchesDirty = true;
					m_TerrainPatches[patchX, patchY].Graph = graph;
				}
			}
		}

		private void CalculateMeshTangent(Mesh mesh)
		{
			Vector4[] tans = new Vector4[mesh.normals.Length];
			for (int i = 0; i < mesh.normals.Length; i++)
			{
				var tan = Vector3.Cross(mesh.normals[i], Vector3.forward).normalized;
				tans[i] = new Vector4(tan.x, tan.y, tan.z, 1);
			}
			mesh.tangents = tans;
		}

		public Mesh CreateRectangularMesh(RectBounds rect, EDirection dir)
		{
			return CreateRectangularMesh(rect.X, rect.Y, rect.Width, rect.Length, dir);
		}
		public Mesh CreateRectangularMesh(RectBounds rect, EDirection dir, Mesh mesh)
		{
			return CreateRectangularMesh(rect.X, rect.Y, rect.Width, rect.Length, dir, mesh);
		}
		public Mesh CreateRectangularMesh(int startX, int startY, int width, int height, EDirection dir)
		{
			return CreateRectangularMesh(startX, startY, width, height, dir, new Mesh());
		}

		public Mesh CreateRectangularMesh(int startX, int startY, int width, int height, EDirection dir, Mesh mesh, bool diamond = false)
		{
			int indiceCount = width * height * (diamond ? 12 : 6);
			int vertexCount = (width + 1) * (height + 1) * (diamond ? 5 : 1);
			int[] tris = new int[indiceCount];
			Vector3[] verts = new Vector3[vertexCount];
			Vector3[] norm = new Vector3[vertexCount];
			Vector4[] tans = new Vector4[vertexCount];
			Vector2[] uv = new Vector2[vertexCount];

			//Vector3 worldOffset = new Vector3(startX, 0, startY);

			if (dir == EDirection.East || dir == EDirection.West)
				CivHelper.Swap<int>(ref width, ref height);

			// Quad Shape
			// todo change to Diamond Shape

			int index = 0;
			for (int y = 0; y <= height; y++)
			{
				int fullY = y + startY;
				float posY1 = y * GridMapView.TileSize;
				for (int x = 0; x <= width; x++)
				{
					int fullX = x + startX;
					float posX1 = x * GridMapView.TileSize;

					float h = gridMap.TerrainData.GetMapLayerBilinear(fullX, fullY).Height;
					verts[index] = new Vector3(posX1, h, posY1);
					uv[index] = new Vector2(posX1 + startX, posY1 + startY);
					norm[index] = Vector3.up;
					tans[index] = new Vector4(1, 0, 0, 0.5f);

					index++;
				}
			}
			int hCount2 = width + 1;
			index = 0;
			for (int y = 0; y < height; y++)
			{
				if (y % 2 == 0)
				{
					for (int x = 0; x < width; x++)
					{
						tris[index++] = (y * hCount2) + x;
						tris[index++] = ((y + 1) * hCount2) + x;
						tris[index++] = ((y + 1) * hCount2) + x + 1;

						tris[index++] = (y * hCount2) + x;
						tris[index++] = ((y + 1) * hCount2) + x + 1;
						tris[index++] = (y * hCount2) + x + 1;
					}
				}
				else
				{
					for (int x = 0; x < width; x++)
					{
						tris[index++] = (y * hCount2) + x;
						tris[index++] = ((y + 1) * hCount2) + x;
						tris[index++] = (y * hCount2) + x + 1;

						tris[index++] = ((y + 1) * hCount2) + x;
						tris[index++] = ((y + 1) * hCount2) + x + 1;
						tris[index++] = (y * hCount2) + x + 1;
					}
				}
			}
			if (mesh == null)
				mesh = new Mesh();
			else
				mesh.Clear();

			mesh.vertices = verts;
			mesh.normals = norm;
			mesh.tangents = tans;
			mesh.uv = uv;
			mesh.uv2 = uv;
			mesh.triangles = tris;

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			//CalculateMeshTangent(mesh);

			return mesh;
		}
		/*
		 * Outdated, no welded vertecies. Includes diamond shape
		Mesh CreateRectangularMesh ( int startX, int startY, int width, int height, EDirection dir, float tilesize, Mesh mesh, bool diamond = true )
		{
			List<int> tris = new List<int> ( width * height * (diamond ? 12 : 6) );
			List<Vector3> verts = new List<Vector3> ( (width + 1) * (height + 1) );
			List<Vector3> norm = new List<Vector3> ( (width + 1) * (height + 1) );
			List<Vector2> uv = new List<Vector2> ( (width + 1) * (height + 1) );

			Vector3 worldOffset = new Vector3 ( startX, 0, startY );

			if ( dir == EDirection.East || dir == EDirection.West )
				CivHelper.Swap<int> ( ref width, ref height );

			for ( int y = 0; y < height; y++ )
			{
				for ( int x = 0; x < width; x++ )
				{
					int fullX = x + startX;
					int fullY = y + startY;
					int tileID = fullY * m_Settings.m_FullMapSize + fullX;

					//todo: Not optimized, merge vertices
					float posX1 = x * tilesize;
					float posY1 = y * tilesize;
					float posX2 = (x + 1) * tilesize;
					float posY2 = (y + 1) * tilesize;


					if ( diamond )
					{
						#region Diamond Shape
						verts.Add ( new Vector3 ( posX1, 0, posY1 ) );
						verts.Add ( new Vector3 ( posX2, 0, posY1 ) );
						verts.Add ( new Vector3 ( (posX1 + posX2) * 0.5f, 0, (posY1 + posY2) * 0.5f ) );
						verts.Add ( new Vector3 ( posX1, 0, posY2 ) );
						verts.Add ( new Vector3 ( posX2, 0, posY2 ) );

						uv.Add ( (worldOffset + verts[verts.Count - 5]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 4]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 3]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 2]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 1]).To2D () );

						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );

						int count = verts.Count - 1;
						tris.Add ( count - 2 );
						tris.Add ( count - 3 );
						tris.Add ( count - 4 );

						tris.Add ( count - 1 );
						tris.Add ( count - 2 );
						tris.Add ( count - 4 );

						tris.Add ( count );
						tris.Add ( count - 2 );
						tris.Add ( count - 1 );

						tris.Add ( count );
						tris.Add ( count - 3 );
						tris.Add ( count - 2 );
						#endregion
					}
					else
					{
						#region Quad Shape
						verts.Add ( new Vector3 ( posX1, 0, posY1 ) );
						verts.Add ( new Vector3 ( posX2, 0, posY1 ) );
						verts.Add ( new Vector3 ( posX1, 0, posY2 ) );
						verts.Add ( new Vector3 ( posX2, 0, posY2 ) );

						uv.Add ( (worldOffset + verts[verts.Count - 4]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 3]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 2]).To2D () );
						uv.Add ( (worldOffset + verts[verts.Count - 1]).To2D () );

						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );
						norm.Add ( Vector3.up );

						int count = verts.Count - 1;
						tris.Add ( count - 1 );
						tris.Add ( count - 2 );
						tris.Add ( count - 3 );

						tris.Add ( count );
						tris.Add ( count - 2 );
						tris.Add ( count - 1 );
						#endregion
					}
				}
			}

			mesh.Clear ();
			mesh.vertices = verts.ToArray ();
			mesh.normals = norm.ToArray ();
			var uvData = uv.ToArray ();
			mesh.uv = uvData;
			mesh.uv2 = uvData;
			mesh.triangles = tris.ToArray ();

			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();
			Debug.LogWarning ( "Check normals correction." );

			return mesh;
		}
		*/

		struct TerrainPatch
		{
			public RectGrid Graph;
			public RectBounds Rect;
			public List<TerrainMeshObject> Meshes;
			public bool Dirty;
		}

		struct TerrainMeshObject
		{
			public int DataID;  //Content ID (terrian, road, pavement...)
			public EDirection Direction;
			public RectBounds Rect;
			public Mesh Mesh
			{
				get { return Object.GetComponent<MeshFilter>().sharedMesh; }
				set
				{
					Object.GetComponent<MeshFilter>().sharedMesh = value;
					Object.GetComponent<MeshCollider>().sharedMesh = value;
				}
			}
			public GameObject Object;
			public void ResetCollider()
			{
				Object.GetComponent<MeshCollider>().sharedMesh = null;
			}
		}
	}
}