using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class TerrainGenerationSurfaceExtended : TerrainGenerationSurface
	{
		const int HEIGHTMAP_LAYER = (int)TerrainGenerationManager.ETerrainLayer.Height;

		const float NOISE_SCALE = 1f / 512f;
		private Perlin m_Perlin;
		private float[,] m_SourceValues; //For readability, could be m_tmpBuffer2 as well

		public TerrainGenerationSurfaceExtended(int seed, int size, int layerCount)
			: base(size, layerCount)
		{
			m_Perlin = new Perlin(seed);
		}

		public TerrainGenerationSurfaceExtended(int seed, int size, int layerCount, Texture2D texture)
			: base(size, layerCount, texture)
		{
			m_Perlin = new Perlin(seed);
		}

		public void Smoothen(float strength = 1, int layer = HEIGHTMAP_LAYER)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

			TmpBuffer = GetLayer(layer);
			float[,] m_SourceValues = GetLayer(layer);

			int max = Size - 1;
			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					float total = 0.0f;
					int count = 0;
					for (int u = i - 1; u <= i + 1; u++)
					{
						for (int v = j - 1; v <= j + 1; v++)
						{
							total += m_SourceValues[
								u < 0 ? 0 : (u >= max ? max : u),
								v < 0 ? 0 : (v >= max ? max : v)
								];
							count++;
						}
					}
					TmpBuffer[i, j] = Mathf.Lerp(m_SourceValues[i, j], total / count, strength);
				}
			}

			SetLayer(layer, TmpBuffer);

			watch.Stop();
			//Debug.Log("Heightmap: ThermalErosion " + watch.Elapsed.TotalSeconds.ToString("0.0") + "sec");
		}

		public void AddPerlinNoise(float frequency, float height = 1f, int layer = HEIGHTMAP_LAYER)
		{
			float maxvalue = 0;

			TmpBuffer = GetLayer(layer);
		
			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					TmpBuffer[i, j] += height * m_Perlin.Noise(frequency * i * NOISE_SCALE, frequency * j * NOISE_SCALE);
					maxvalue = Mathf.Max(maxvalue,TmpBuffer[i, j] );
				}
			}
			SetLayer(layer, TmpBuffer);

			//Debug.Log("Add Perlin: maxvalue ("+layer.ToString()+"): "+ maxvalue.ToString());
					}

		public void AddRigidNoise(float frequency, float height = 1f, int layer = HEIGHTMAP_LAYER)
		{
			TmpBuffer = GetLayer(layer);

			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					float value = m_Perlin.Noise(frequency * i * NOISE_SCALE, frequency * j * NOISE_SCALE);

					value = 0.5f - Mathf.Abs(value);

					TmpBuffer[i, j] += height * value;
				}
			}

			SetLayer(layer, TmpBuffer);
		}

		public void ThermalErosion(float talusAngleMin, float talusAngleMax, float carry,
			int layer = HEIGHTMAP_LAYER, int outputLayer = HEIGHTMAP_LAYER)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			bool output = (layer != outputLayer);

			TmpBuffer = GetLayer(layer);
			if (output)
				TmpBuffer2 = GetLayer(outputLayer);

			for (int x = 1; x < Size - 1; x++)
			{
				for (int y = 1; y < Size - 1; y++)
				{
					float h = TmpBuffer[x, y];
					float slope;
					float max = h;
					int matchU = 0, matchV = 0;

					for (int u = -1; u <= 1; u += 2)
					{
						for (int v = -1; v <= 1; v += 2)
						{
							//if ( u != 0 && v != 0 )
							{
								float value = TmpBuffer[x + u, y + v];
								if (value > max)
								{
									max = value;
									matchU = u;
									matchV = v;
								}
							}
						}
					}

					slope = Mathf.Abs(max - h);

					float slopeMask = Mathf.Clamp01((slope - talusAngleMin) / (talusAngleMax - talusAngleMin));
					slopeMask = Mathf.Clamp01(1 - Math.Abs((slopeMask * 2) - 1));
					if (output)
						TmpBuffer2[x, y] = Mathf.Clamp01(TmpBuffer2[x, y] + 1 * slopeMask);

					slopeMask = Mathf.Sqrt(slopeMask);

					float carryAmount = carry * slopeMask * slope;

					TmpBuffer[x, y] += carryAmount;
					TmpBuffer[x + matchU, y + matchV] -= carryAmount;
				}
			}

			SetLayer(layer, TmpBuffer);
			if (output)
				SetLayer(outputLayer, TmpBuffer2);

			watch.Stop();
			//Debug.Log("Heightmap: ThermalErosion " + watch.Elapsed.TotalSeconds.ToString("0.0") + "sec");
		}

		public void ThermalErosion(float talusAngleMin, float carry)
		{
			Debug.LogWarning("UNIMPLEMENTED");
			//var watch = System.Diagnostics.Stopwatch.StartNew ();
			//
			//var heights = Heights;
			//
			//for ( int x = 1; x < m_Size - 1; x++ )
			//{
			//	for ( int y = 1; y < m_Size - 1; y++ )
			//	{
			//		float h = heights[x, y];
			//		float slope;
			//		float max = h;
			//		int matchU = 0, matchV = 0;
			//
			//		for ( int u = -1; u <= 1; u += 2 )
			//		{
			//			for ( int v = -1; v <= 1; v += 2 )
			//			{
			//				//if ( u != 0 && v != 0 )
			//				{
			//					float value = heights[x + u, y + v];
			//					if ( value > max )
			//					{
			//						max = value;
			//						matchU = u;
			//						matchV = v;
			//					}
			//				}
			//			}
			//		}
			//
			//		slope = Mathf.Abs ( max - h );
			//
			//		if ( slope >= talusAngleMin )
			//		{
			//			Dirt[x, y] += 1;
			//			heights[x, y] += carry * slope;
			//			heights[x + matchU, y + matchV] -= carry * slope;
			//		}
			//	}
			//}
			//
			//Heights = heights;
			//watch.Stop ();
			//Debug.Log ( "Heightmap: ThermalErosion " + watch.Elapsed.TotalSeconds.ToString ( "0.0" ) + "sec" );
		}

		/// <summary>
		/// Samples are currently limited to 16
		/// </summary>
		/// <param name="samples"></param>
		public void Median(int samples, int layer)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

			TmpBuffer = GetLayer(layer);
			m_SourceValues = GetLayer(layer);

			samples = Mathf.Clamp(samples, 1, 16);
			List<float> values = new List<float>((samples * 2) * (samples * 2));

			for (int i = samples; i < Size - samples; ++i)
			{
				for (int j = samples; j < Size - samples; ++j)
				{
					float value = 0;

					for (int u = -samples; u <= samples; u++)
					{
						for (int v = -samples; v <= samples; v++)
						{
							values.Add(m_SourceValues[i + u, j + v]);
						}
					}

					values.Sort();
					int halfCount = (int)(values.Count * 0.5f);
					value = values[halfCount];
					values.Clear();

					TmpBuffer[i, j] = value;
				}
			}

			SetLayer(layer, TmpBuffer);

			watch.Stop();
			//Debug.Log("Heightmap: Median " + watch.Elapsed.TotalSeconds.ToString("0.0") + "sec");
		}

		public void Multiply(float value, int layer = HEIGHTMAP_LAYER)
		{
			TmpBuffer = GetLayer(layer);
			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					TmpBuffer[i, j] *= value;
				}
			}
			SetLayer(layer, TmpBuffer);
		}

		public void Add(float value, int layer = HEIGHTMAP_LAYER)
		{
			TmpBuffer = GetLayer(layer);
			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					TmpBuffer[i, j] += value;
				}
			}
			SetLayer(layer, TmpBuffer);
		}

		public void Set(float value, int layer = HEIGHTMAP_LAYER)
		{
			TmpBuffer = GetLayer(layer);
			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					TmpBuffer[i, j] = value;
				}
			}
			SetLayer(layer, TmpBuffer);
		}

		/// <summary>
		/// Samples are currently limited to 16. Fast variant of Median, not the same result
		/// </summary>
		/// <param name="samples"></param>
		public void MedianFast(int samples, float bias, int layer)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();

			TmpBuffer = GetLayer(layer);
			m_SourceValues = GetLayer(layer);

			samples = Mathf.Clamp(samples, 1, 16);

			for (int i = samples; i < Size - samples; ++i)
			{
				for (int j = samples; j < Size - samples; ++j)
				{
					float value = 0;

					for (int u = -samples; u <= samples; u++)
					{
						for (int v = -samples; v <= samples; v++)
						{
							value += m_SourceValues[i + u, j + v];
						}
					}
					if (value >= bias)
						TmpBuffer[i, j] = 1;
					else
						TmpBuffer[i, j] = 0;
				}
			}

			SetLayer(layer, TmpBuffer);

			watch.Stop();
			//Debug.Log("Heightmap: MedianFast " + watch.Elapsed.TotalSeconds.ToString("0.0") + "sec");
		}

		internal void Clamp(int layer, float min = 0f, float max = 1f)
		{
			TmpBuffer = GetLayer(layer);
			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					TmpBuffer[i, j] = Mathf.Clamp(TmpBuffer[i, j], min, max);
				}
			}
			SetLayer(layer, TmpBuffer);
		}
	}
}