using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class TerrainGenerator_Wasteland : TerrainGenerator
	{
		public override void TerrainGeneration(TerrainSettings settings)
		{
			MapPreset preset = settings.Preset;

			float heightmapScale = (float)(settings.SimulationHeightmapResolution) /
								   (float)(settings.FullMapGridSize);
			float heightmapScaleInv = 1f / heightmapScale;

			heightmap = new TerrainGenerationSurfaceExtended(settings.Seed, settings.SimulationHeightmapResolution, (int)TerrainGenerationManager.ETerrainLayer.COUNT);
			loadingState.SetMessage("Rigid Noise - Pass 1");
			heightmap.AddRigidNoise(8f * heightmapScaleInv, 1f);

			loadingState.SetMessage("Rigid Noise - Pass 2");
			heightmap.AddRigidNoise(30f * heightmapScaleInv, 0.4f);

			for (int i = 0; i < preset.ErosionIteration * heightmapScale; i++)
			{
				loadingState.SetMessage("Erosion - Pass " + (i + 1));
				heightmap.ThermalErosion(0.0063f, 0.26f, preset.ErosionCarryAmount, (int)TerrainGenerationManager.ETerrainLayer.Height, (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			}

			for (int i = 0; i < 2; i++)
			{
				loadingState.SetMessage("Thermal Erosion - Pass " + (i + 1));
				heightmap.ThermalErosion(0.02f, 0.8f);
			}

			for (int i = 0; i < 2; i++)
			{
				loadingState.SetMessage("Smoothing - Pass " + (i + 1));
				heightmap.Smoothen();
			}

			loadingState.SetMessage("Heightmap adjustments ");
			//heightmap.MedianFast ( 2, 3.8f, TerrainSurface.Layer.Dirt );

			heightmap.Smoothen((int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.AddPerlinNoise(12f * heightmapScaleInv, 1f, (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.AddPerlinNoise(20f * heightmapScaleInv, 0.8f, (int)TerrainGenerationManager.ETerrainLayer.Waste);
			heightmap.Add(0.8f, (int)TerrainGenerationManager.ETerrainLayer.Waste);
			heightmap.AddRigidNoise(52f * heightmapScaleInv, -0.2f, (int)TerrainGenerationManager.ETerrainLayer.Waste);

			//Smoothen = Bad blur
			heightmap.Smoothen( (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.Smoothen( (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.Smoothen( (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.Smoothen( (int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.Multiply(0.70f, (int)TerrainGenerationManager.ETerrainLayer.FertileLand);

			//Clamp all
			heightmap.Clamp((int)TerrainGenerationManager.ETerrainLayer.FertileLand);
			heightmap.Clamp((int)TerrainGenerationManager.ETerrainLayer.Waste);
			//Flatten
			heightmap.Multiply(0.0f, (int)TerrainGenerationManager.ETerrainLayer.Height);
		}
	}
}