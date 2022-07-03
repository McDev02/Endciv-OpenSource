using System;
using UnityEngine;
namespace Endciv
{
	public class TerrainView : MonoBehaviour
	{
		TerrainManager Manager;

		[SerializeField] Color ValueGood = Color.green;
		[SerializeField] Color ValueBad = Color.red;
		[SerializeField] Color ValueInfo = Color.blue;

		[SerializeField] Color ColorPollution = Color.red;
		[SerializeField] Color ColorWater = Color.blue;
		[SerializeField] Color ColorWaterBad = Color.red;

		public enum ELayerDataView { Base, PartitionAverage, PartitionMin, PartitionMax }
		public enum ELayerView
		{
			//Core values
			None, Occupied, Passable, Mixed, IslandID, Reserved,
			//Simulated values
			CityDensity, OpenArea, Pollution, GroundWater,
			//Terrain layers
			FertileLand, Waste
		}
		public ELayerView LayerMode;
		[SerializeField] ELayerDataView LayerDataMode;
		ELayerView oldLayerMode;
		ELayerDataView oldLayerDataMode;
		bool update;

		public Action OnLayerChanged;

		float cityDensityViewFactor = 1f / 4f;
		float pollutionViewFactor = 1f / 4f;
		float groundWaterViewFactor = 1f / 4f;

		GridMap GridMap;

		internal void ShowLayerMap(ELayerView layer)
		{
			//Editor automatically updates when value changes
			LayerMode = layer;
#if !UNITY_EDITOR
			UpdateMap();
#endif
		}

		GridMapView GridMapView;
		[SerializeField]
		public TextureBuffer TerrainSplatMap;
		TextureBuffer LayerTexture;

		private bool showGrid;
		public bool ShowGrid
		{
			get { return showGrid; }
			set
			{
				showGrid = value;
				if (showGrid) Shader.EnableKeyword("GRID_ON");
				else
					Shader.DisableKeyword("GRID_ON");
			}
		}

		//[SerializeField] MeshRenderer TestTerrainPlane;

		public Texture2D LayerTextureReff;

		bool IsReady;

		public void Setup(TerrainManager terrainManager, GridMap gridMap)
		{
			Manager = terrainManager;
			GridMap = gridMap;
			GridMapView = GridMap.View;

			GridMap.OnGridUpdate += SetDirty;

			//Test stuff View
			/* TestTerrainPlane.transform.position = new Vector3(GridMap.Width / 2f * GridMapView.TileSize, 0, GridMap.Length / 2f * GridMapView.TileSize);
             TestTerrainPlane.transform.localScale = new Vector3(GridMap.Width * GridMapView.TileSize / 10, 1, GridMap.Length * GridMapView.TileSize / 10);
             var mat = TestTerrainPlane.sharedMaterial;
 #if USE_GRIDTILE
             mat.SetTextureScale("_MainTex", new Vector2(GridMap.Width / 2, GridMap.Length / 2));
             mat.SetTextureOffset("_MainTex", new Vector2(0.25f - 1f / 64f, 0.25f + 1f / 64f));
 #else
             mat.SetTextureScale("_MainTex", new Vector2(GridMap.Width, GridMap.Length));
             mat.SetTextureOffset("_MainTex", new Vector2(1f / 64f, 1f / 64f));
 #endif
             mat.SetTextureScale("_DetailAlbedoMap", new Vector2(GridMap.Width / 20f, GridMap.Length / 20f));
 */
			LayerTexture = new TextureBuffer("LayerView", GridMap.Width, GridMap.Length, TextureFormat.ARGB32, false, true, FilterMode.Point, TextureWrapMode.Clamp);
			LayerTextureReff = LayerTexture.Texture;

			TerrainSplatMap = new TextureBuffer("TerrainSplatMap", GridMap.Width, GridMap.Length, TextureFormat.RGBA32, false, true, FilterMode.Bilinear, TextureWrapMode.Clamp);

			//Shader.SetGlobalFloat("_MapGridScale", 1f / GridMap.GridWidth);
			Shader.SetGlobalTexture("_GTEX_TerrainSplatMap", TerrainSplatMap.Texture);

			ResetTerrainTextures();
			HideLayerMap();
			oldLayerDataMode = LayerDataMode = ELayerDataView.Base;
			oldLayerMode = LayerMode = ELayerView.None;
			IsReady = true;
			ShowGrid = true;

			GridMap.OnLayersUpdated -= UpdateMap;
			GridMap.OnLayersUpdated += UpdateMap;
		}

		private void ShowLayerMap(bool debugMode)
		{
			Shader.SetGlobalTexture("_GTEX_TerrainDebugMap", LayerTexture.Texture);
			if (debugMode)
			{
				Shader.EnableKeyword("OVERLAY_DEBUG");
				Shader.DisableKeyword("OVERLAY_LAYER");
			}
			else
			{
				Shader.EnableKeyword("OVERLAY_LAYER");
				Shader.DisableKeyword("OVERLAY_DEBUG");
			}
		}
		public void ShowLayerMap(Color[] colors, bool debugMode)
		{
			LayerTexture.SetPixels(colors);
			LayerTexture.Apply();
			ShowLayerMap(debugMode);
		}
		public void HideLayerMap()
		{
			LayerMode = ELayerView.None;
			Shader.DisableKeyword("OVERLAY_DEBUG");
			Shader.DisableKeyword("OVERLAY_LAYER");
		}

#if UNITY_EDITOR
		private void Update()
		{
			if (!IsReady) return;
			if (oldLayerMode != LayerMode || oldLayerDataMode != LayerDataMode || update)
			{
				update = false;
				oldLayerDataMode = LayerDataMode;
				oldLayerMode = LayerMode;
				UpdateMap();
			}
		}
#endif

		public void SetDirty()
		{
			update = true;
		}

		public void UpdateMap()
		{
			OnLayerChanged?.Invoke();

			if (LayerMode == ELayerView.None)
			{
				HideLayerMap();
				return;
			}

			Color col = Color.black;
			float val = 0;
			int id;
			EGridOccupation occ;

			cityDensityViewFactor = 1f / GameConfig.Instance.CityDensityViewThreshold;
			pollutionViewFactor = 1f / GameConfig.Instance.PollutionViewThreshold;
			groundWaterViewFactor = 1f / GameConfig.Instance.GroundWaterViewThreshold;

			for (int x = 0; x < GridMap.Width; x++)
			{
				for (int y = 0; y < GridMap.Length; y++)
				{
					if (LayerDataMode == ELayerDataView.Base)
						col = GetMapBaseColor(x, y);
					else
						col = GetMapPartitionColor(LayerDataMode, x, y);

					LayerTexture.SetPixel(x + y * GridMap.Width, col);
				}
			}

			bool layerMode = LayerMode == ELayerView.Pollution || LayerMode == ELayerView.GroundWater;
			LayerTexture.Apply();
			ShowLayerMap(!layerMode);
		}

		Color GetMapBaseColor(int x, int y)
		{
			float val = 0;
			EGridOccupation occ;
			Color col = new Color(0, 0, 0, 0);

			switch (LayerMode)
			{
				case ELayerView.Occupied:
					occ = GridMap.Data.occupied[x, y];
					if (occ == EGridOccupation.Free)
						col = ValueGood;
					if (occ == EGridOccupation.StayFree)
						col = ValueInfo;
					if (occ == EGridOccupation.Occupied)
						col = ValueBad;
					break;
				case ELayerView.Passable:
					val = GridMap.Data.passability[x, y];
					col = Color.Lerp(ValueBad, ValueGood, val);
					break;
				case ELayerView.Mixed:
					val = GridMap.Data.passability[x, y];
					col = Color.Lerp(ValueBad, ValueGood, val);
					occ = GridMap.Data.occupied[x, y];
					if (occ == EGridOccupation.StayFree)
						col = Color.Lerp(col, ValueInfo, 0.5f);
					if (occ == EGridOccupation.Occupied)
						col = Color.Lerp(col, Color.magenta, 0.5f);
					break;
				case ELayerView.IslandID:
					if (GridMap.Grid.HasNode(x, y))
					{
						var id = GridMap.Grid.NodeLookup[x, y].GroupID;
						col = RandomColorPool.Instance.GetColor(id);
					}
					else
						col = ValueBad;
					break;
				case ELayerView.Reserved:
					col = ValueBad;
					if (!GridMap.Data.reserved[x, y])
						col.a = 0;
					break;
				case ELayerView.CityDensity:
					val = GridMap.Data.cityDensity[x, y];
					col = Color.Lerp(ValueGood, ValueBad, val * cityDensityViewFactor);
					break;
				case ELayerView.OpenArea:
					val = GridMap.Data.openArea[x, y];
					col = Color.Lerp(ValueBad, ValueGood, val / (GameConfig.Instance.OpenAreaDistance + 1));
					break;
				case ELayerView.Pollution:
					val = GridMap.Data.pollution[x, y];
					col = ColorPollution;
					col.a = val * pollutionViewFactor;
					break;
				case ELayerView.GroundWater:
					var pol = GridMap.Data.pollution[x, y] * pollutionViewFactor;
					val = 1;// GridMap.Data.groundWater[x, y];
					col = Color.Lerp(ColorWater, ColorWaterBad, pol);
					col.a = val * pollutionViewFactor;
					break;
				case ELayerView.FertileLand:
					val = GridMap.Data.fertileLand[x, y];
					col = Color.Lerp(ValueBad, ValueGood, val);
					break;
				case ELayerView.Waste:
					val = GridMap.Data.waste[x, y];
					col = Color.Lerp(ValueGood, ValueBad, val);
					break;
				default:
					break;
			}
			return col;
		}
		Color GetMapPartitionColor(ELayerDataView dataType, int x, int y)
		{
			float val = 0;
			EGridOccupation occ;
			Color col = Color.black;

			var size = 1f / GridMap.partitionSystem.PartitionSize;
			x = (int)(x * size);
			y = (int)(y * size);

			switch (LayerMode)
			{
				case ELayerView.Occupied:
					break;
				case ELayerView.Passable:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.passabilitySummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.passabilitySummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.passabilitySummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueBad, ValueGood, val);
					break;
				case ELayerView.Mixed:
					break;
				case ELayerView.IslandID:
					break;
				case ELayerView.Reserved:
					break;
				case ELayerView.CityDensity:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.cityDensitySummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.cityDensitySummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.cityDensitySummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueGood, ValueBad, val * cityDensityViewFactor);
					break;
				case ELayerView.OpenArea:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.openAreaSummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.openAreaSummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.openAreaSummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueGood, ValueBad, val * cityDensityViewFactor);
					break;
				case ELayerView.Pollution:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.pollutionSummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.pollutionSummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.pollutionSummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueGood, ValueBad, val * pollutionViewFactor);
					break;
				case ELayerView.FertileLand:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.fertileLandSummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.fertileLandSummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.fertileLandSummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueBad, ValueGood, val);
					break;
				case ELayerView.Waste:
					switch (dataType)
					{
						case ELayerDataView.PartitionAverage:
							val = GridMap.Data.wasteSummary[x, y].NodeAverage;
							break;
						case ELayerDataView.PartitionMin:
							val = GridMap.Data.wasteSummary[x, y].NodeMin;
							break;
						case ELayerDataView.PartitionMax:
							val = GridMap.Data.wasteSummary[x, y].NodeMax;
							break;
					}
					col = Color.Lerp(ValueGood, ValueBad, val);
					break;
				default:
					break;
			}
			return col;
		}

		public void ResetTerrainTextures()
		{
			TerrainSplatMap.Clear();
			UpdateTerrainSplatMap();
		}

		public void UpdateTerrainSplatMap()
		{
			Color32[] cols = new Color32[GridMap.Width * GridMap.Length];

			var d = GridMap.Data;
			for (int x = 0; x < GridMap.Width; x++)
			{
				for (int y = 0; y < GridMap.Length; y++)
				{
					TerrainSplatMap.SetPixel(x + y * GridMap.Width, new Color(
						Mathf.Sqrt(d.fertileLand[x, y]),    //Fertile Land
						0,                      //Undefined
						Mathf.Sqrt(d.waste[x, y]) * 0.92f,          //Waste
						0));                    //Undefined
				}
			}
			TerrainSplatMap.Apply();
		}
	}
}