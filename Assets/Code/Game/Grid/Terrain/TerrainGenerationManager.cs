using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class TerrainGenerationManager : MonoBehaviour
	{
		public enum EMapType
		{
			Wasteland,
			Flat
		}

		public enum ETerrainLayer
		{
			Height,
			FertileLand,
			Dirt,
			Waste,
			COUNT
		}

		[SerializeField] private EMapType m_MapType;

		private TerrainExchangeData m_Data;
		public bool m_GenerationFlag { get; private set; }

		public IEnumerator Generate(TerrainSettings settings, LoadingState loadingState)
		{
			m_GenerationFlag = false;
			TerrainGenerator Generator = null;
			loadingState.SetMessage("Generate Terrain");

			switch (m_MapType)
			{
				case EMapType.Wasteland:
					Generator = new TerrainGenerator_Wasteland();
					break;
				case EMapType.Flat:
					Generator = new TerrainGenerator_Flat();
					break;
			}

			Generator.loadingState = loadingState;
			Generator.TerrainGeneration(settings);

			loadingState.SetMessage("Terrain generated");
			//Generator.CreateTerrainTexture ( settings );
			var surface = Generator.heightmap;

			m_Data = new TerrainExchangeData();
			m_Data.Surfaces = DownsampleTerrainLayer(surface, settings);
			m_Data.Height = ConvertHeightToGridSpace(surface, settings);

			m_GenerationFlag = true;
			yield return null;
		}

		public TerrainExchangeData GetExchangeData()
		{
			return m_Data;
		}

		private TerrainSurface DownsampleTerrainLayer(TerrainGenerationSurface surface, TerrainSettings settings)
		{
			int size = settings.FullMapSize;
			float oneBySize = 1f / (float)size;
			TerrainSurface layers = new TerrainSurface();
			for (int l = 0; l < surface.LayerCount; l++)
			{
				float maxvalue = 0;
				var layer = new float[size, size];
				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						Vector2 uv = new Vector2(x * oneBySize, y * oneBySize);

						layer[x, y] = surface.SampleBilinear(uv, l );
						maxvalue = Mathf.Max(maxvalue, layer[x, y]);
					}
				}
				switch ((ETerrainLayer)l)
				{
					case ETerrainLayer.FertileLand:
						layers.fertileLand = layer;
						break;
					case ETerrainLayer.Dirt:
						break;
					case ETerrainLayer.Waste:
						layers.waste = layer;
						break;
				}
			}
			return layers;
		}

		public float[,] ConvertHeightToGridSpace(TerrainGenerationSurface surface, TerrainSettings settings)
		{
			//var world = World.Instance;
			int size = settings.FullMapSize + 1;
			float[,] heightmap = new float[size, size];

			float oneBySize = 1f / (float)size;
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					heightmap[x, y] = surface.SampleBilinear(x * oneBySize, y * oneBySize, (int)ETerrainLayer.Height);
				}
			}
			return heightmap;
		}
	}
}