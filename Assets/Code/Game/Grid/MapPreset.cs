using UnityEngine;
using System;

namespace Endciv
{
    [Serializable]
    public class TerrainSurfaceSaveData : ISaveable
    {
        public string name;
        //public SerVector4 baseColor;
        public float blendEdge;
        public int textureTileSize;
        public float beauty;
        public float pollution;
        public float fertility;

        public ISaveable CollectData()
        {
            return this;
        }
    }

    [Serializable]
    public class MapPresetSaveData : ISaveable
    {
        public int erosionIteration;
        public float erosionCarryAmount;
        public TerrainSurfaceSaveData[] surfaces;
        public TerrainSurfaceSaveData snowSurface;
        public float snowNoiseTileSize;
        public TerrainSurfaceSaveData[] terrainLayers;

        public ISaveable CollectData()
        {
            return this;
        }
    }

    public class MapPreset : ScriptableObject, ISaveable, ILoadable<MapPresetSaveData>  
	{
		public Material TerrainBlendMaterial;

		public int ErosionIteration = 6;
		public float ErosionCarryAmount = 0.6f;

		public TerrainSurface[] Surfaces;
		public TerrainSurface SnowSurface;
		public Texture2D SnowNoiseTexture;
		public float SnowNoiseTileSize;

		public TerrainLayer[] Layers;
				
		[Serializable]
		public struct TerrainSurface : ISaveable, ILoadable<TerrainSurfaceSaveData>
		{
			public string Name;
			public Texture2D Texture;
			public Texture2D NormalMap;
			//public Color BaseColor;
			public float BlendEdge;
			public int TextureTileSize;
			//Layer
			public float Beauty;
			public float Pollution;
			public float Fertility;

			public float Tiling
			{
#if USE_GRIDTILE
				get { return 1f / (TextureTileSize * GridMapView.GridTileSize); }
#else
				get { return 1f / (TextureTileSize * GridMapView.TileSize); }
#endif
			}

			public ISaveable CollectData()
			{
				var data = new TerrainSurfaceSaveData();
				data.name = Name;
				//data.baseColor = new SerVector4(BaseColor.r, BaseColor.g, BaseColor.b, BaseColor.a);
				data.blendEdge = BlendEdge;
				data.textureTileSize = TextureTileSize;
				data.beauty = Beauty;
				data.pollution = Pollution;
				data.fertility = Fertility;
				return data;
			}

			public void ApplySaveData(TerrainSurfaceSaveData data)
			{
				if (data == null)
					return;
				Name = data.name;
                //BaseColor = new Color(data.baseColor.X, data.baseColor.Y, data.baseColor.Z, data.baseColor.W);
                BlendEdge = data.blendEdge;
				TextureTileSize = data.textureTileSize;
				Beauty = data.beauty;
				Pollution = data.pollution;
				Fertility = data.fertility;
			}
		}

		[Serializable]
		public struct TerrainLayer
		{
			public TerrainSurface Surface;
		}

		public MapPreset Clone()
		{
			var preset = ScriptableObject.CreateInstance<MapPreset>();
			preset.TerrainBlendMaterial = new Material(TerrainBlendMaterial);
			preset.ErosionIteration = ErosionIteration;
			preset.ErosionCarryAmount = ErosionCarryAmount;
			preset.SnowSurface = SnowSurface;
			preset.Surfaces = (TerrainSurface[])Surfaces.Clone();
			preset.SnowNoiseTexture = SnowNoiseTexture;
			preset.SnowNoiseTileSize = SnowNoiseTileSize;
			preset.Layers = (TerrainLayer[])Layers.Clone();
			return preset;
		}		

		public ISaveable CollectData()
		{
			var data = new MapPresetSaveData();
			data.erosionIteration = ErosionIteration;
			data.erosionCarryAmount = ErosionCarryAmount;
			if (Surfaces != null && Surfaces.Length > 0)
			{
				data.surfaces = new TerrainSurfaceSaveData[Surfaces.Length];
				for (int i = 0; i < Surfaces.Length; i++)
				{
					data.surfaces[i] = (TerrainSurfaceSaveData)Surfaces[i].CollectData();
				}
			}
			data.snowSurface = (TerrainSurfaceSaveData)SnowSurface.CollectData();
			data.snowNoiseTileSize = SnowNoiseTileSize;
			if (Layers != null && Layers.Length > 0)
			{
				data.terrainLayers = new TerrainSurfaceSaveData[Layers.Length];
				for (int i = 0; i < Layers.Length; i++)
				{
					data.terrainLayers[i] = (TerrainSurfaceSaveData)Layers[i].Surface.CollectData();
				}
			}
			return data;
		}

		public void ApplySaveData(MapPresetSaveData data)
		{
			if (data == null)
				return;
			ErosionIteration = data.erosionIteration;
			ErosionCarryAmount = data.erosionCarryAmount;
			if (data.surfaces != null && data.surfaces.Length > 0)
			{
				for (int i = 0; i < data.surfaces.Length; i++)
				{
					Surfaces[i].ApplySaveData(data.surfaces[i]);
				}
			}
			if (data.snowSurface != null)
			{
				SnowSurface.ApplySaveData(data.snowSurface);
			}
			SnowNoiseTileSize = data.snowNoiseTileSize;
			if (data.terrainLayers != null && data.terrainLayers.Length > 0)
			{
				for (int i = 0; i < data.terrainLayers.Length; i++)
				{
					Layers[i].Surface.ApplySaveData(data.terrainLayers[i]);
				}
			}
		}
	}
}