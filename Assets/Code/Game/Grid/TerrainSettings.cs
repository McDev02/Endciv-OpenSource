using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class TerrainSettingsSaveData : ISaveable
	{
		public MapPresetSaveData mapPreset;
		public int seed;
		public int terrainPatchCount;
		public int fullMapSize;
		public int fullMapGridSize;
		public float gridSizeWorld;
		public int safePadding;

		public ISaveable CollectData()
		{
			return this;
		}
	}

	[Serializable]
	public class TerrainSettings : ISaveable, ILoadable<TerrainSettingsSaveData>
	{
		public enum EMapSize
		{
			Mini,
			Small,
			Medium,
			Big,
			Huge
		}

		//Constant values
		public const int BaseTerrainSize = 16;

		/// <summary>
		/// Terrain patch resolution.
		/// </summary>
		public const int PatchResolution = BaseTerrainSize * 4;

		/// <summary>
		/// BorderSize describes the amount of Tiles for the border in each direction
		/// </summary>
		public const int BorderSize = 2;

		//Properties        
		[SerializeField] public MapPreset Preset;

		public int Seed;

		[NonSerialized] public int TerrainPatchCount;

		/// <summary>
		/// Map size of one axis
		/// </summary>
		public int FullMapSize;
		/// <summary>
		/// Map size of one axis in Grid
		/// </summary>
		public int FullMapGridSize;
		/// <summary>
		/// Map size with all tiles in World Space
		/// </summary>
		[NonSerialized] public float GridWorldSize;

		public int safePadding = 3;

		public int SimulationHeightmapResolution { get; private set; }

		public void ApplyMapSize(EMapSize size)
		{
			switch (size)
			{
				case EMapSize.Mini:
					FullMapSize = PatchResolution * 2;
					break;
				case EMapSize.Small:
					FullMapSize = PatchResolution * 3;
					break;
				case EMapSize.Medium:
					FullMapSize = PatchResolution * 4;
					break;
				case EMapSize.Big:
					FullMapSize = PatchResolution * 6;
					break;
				case EMapSize.Huge:
					FullMapSize = PatchResolution * 8;
					break;
				default:
					break;
			}
			FullMapGridSize = FullMapSize;
#if USE_GRIDTILE
			FullMapSize *= 2;
			SimulationHeightmapResolution = FullMapSize;
#else
			SimulationHeightmapResolution = FullMapSize * 2;
#endif
		}

		public void Setup()
		{
			//Derrivatives
			TerrainPatchCount = Mathf.CeilToInt((float)FullMapSize / (float)PatchResolution);
			GridWorldSize = FullMapGridSize * GridMapView.GridTileSize;
		}

		public TerrainSettings Clone()
		{
			var settings = new TerrainSettings();
			settings.Preset = Preset.Clone();
			settings.Seed = Seed;
			settings.TerrainPatchCount = TerrainPatchCount;
			settings.FullMapSize = FullMapSize;
			settings.FullMapGridSize = FullMapGridSize;
			settings.GridWorldSize = GridWorldSize;
			settings.safePadding = safePadding;
			return settings;
		}

		public ISaveable CollectData()
		{
			var data = new TerrainSettingsSaveData();
			data.mapPreset = (MapPresetSaveData)Preset.CollectData();
			data.seed = Seed;
			data.terrainPatchCount = TerrainPatchCount;
			data.fullMapSize = FullMapSize;
			data.fullMapGridSize = FullMapGridSize;
			data.gridSizeWorld = GridWorldSize;
			data.safePadding = safePadding;
			return data;
		}

		public void ApplySaveData(TerrainSettingsSaveData data)
		{
			if (data == null)
				return;
			if (data.mapPreset != null)
			{
				Preset.ApplySaveData(data.mapPreset);
			}
			Seed = data.seed;
			TerrainPatchCount = data.terrainPatchCount;
			FullMapSize = data.fullMapSize;
			FullMapGridSize = data.fullMapGridSize;
			GridWorldSize = data.gridSizeWorld;
			safePadding = data.safePadding;
		}
	}
}