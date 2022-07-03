using System.Collections;
using UnityEngine;

namespace Endciv
{
    public class TerrainGenerator_Flat : TerrainGenerator
    {
        public override void TerrainGeneration(TerrainSettings settings)
        {
			//MapPreset preset = settings.Preset;

			//float heightmapScale = (float) (settings.m_SimulationHeightmapResolution - 1)/
			//					   (float) (settings.m_FullMapSize);

            heightmap = new TerrainGenerationSurfaceExtended(settings.Seed, settings.SimulationHeightmapResolution, (int)TerrainGenerationManager.ETerrainLayer.COUNT);
        }
    }
}