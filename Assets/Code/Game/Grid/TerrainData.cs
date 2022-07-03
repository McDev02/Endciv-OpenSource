using System;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Model Class for a Terrain Data (Grid vertices)
	/// </summary>
	public class TerrainData
	{
		//Values
		public float[,] Height;
		public float[,] SmoothHeight;
		public Vector3[,] Position;
		//Directions
		public Vector3[,] Normal;
		public Vector3[,] Tangent;
		public Vector3[,] Binormal;

		public int Width { get; private set; }
		public int Length { get; private set; }
		public int MaxX, MaxY;

		public TerrainData(int width, int length)
		{
			//GridData
			Height = new float[width, length];
			SmoothHeight = new float[width, length];

			Position = new Vector3[width, length];

			Normal = new Vector3[width, length];
			Tangent = new Vector3[width, length];
			Binormal = new Vector3[width, length];

			Width = width;
			Length = length;

			MaxX = width - 1;
			MaxY = length - 1;
		}

		#region Data Models
		public struct TerrainDataFull
		{
			public float Height;
			public Vector3 Normal;
			public Vector3 Tangent;
			public Vector3 Binormal;
		}
		#endregion

		#region Model Access
		public TerrainDataFull GetTerrainDataFull(int x, int y)
		{
			var data = new TerrainDataFull();

			data.Height = Height[x, y];
			data.Normal = Normal[x, y];
			data.Tangent = Tangent[x, y];
			data.Binormal = Binormal[x, y];

			return data;
		}

		public float GetHeight(int x, int y)
		{
			return Height[x, y];
		}

		public TerrainDataFull GetMapLayerBilinear(float x, float y)
		{
			var data = new TerrainDataFull();

			int aX = (int)x;
			int aY = (int)y;
			aX = aX < MaxX ? aX : MaxX;
			aY = aY < MaxY ? aY : MaxY;
			aX = aX < 0 ? 0 : aX;
			aY = aY < 0 ? 0 : aY;

			int bX = aX + 1;
			int bY = aY + 1;
			bX = bX < MaxX ? bX : MaxX;
			bY = bY < MaxY ? bY : MaxY;

			float dX = x - aX;
			float dY = y - aY;
			float dX2 = 1 - dX;
			float dY2 = 1 - dY;

			data.Height = dY * (dX * Height[aX, aY] + dX2 * Height[bX, aY]) + dY2 * (dX * Height[aX, bY] + dX2 * Height[bX, bY]);
			data.Normal = dY * (dX * Normal[aX, aY] + dX2 * Normal[bX, aY]) + dY2 * (dX * Normal[aX, bY] + dX2 * Normal[bX, bY]);
			data.Tangent = dY * (dX * Tangent[aX, aY] + dX2 * Tangent[bX, aY]) + dY2 * (dX * Tangent[aX, bY] + dX2 * Tangent[bX, bY]);
			data.Binormal = dY * (dX * Binormal[aX, aY] + dX2 * Binormal[bX, aY]) + dY2 * (dX * Binormal[aX, bY] + dX2 * Binormal[bX, bY]);

			return data;
		}
		#endregion
	}

    /// <summary>
    /// Model Class for a Terrain Data (Grid tiles)
    /// </summary>
    [Serializable]
    public class TerrainSurface : ISaveable
	{
		public float[,] fertileLand;
		public float[,] waste;

        public ISaveable CollectData()
        {
            return this;
        }
	}


	/// <summary>
	/// Only used to exchange data
	/// </summary>
    [Serializable]
	public class TerrainExchangeData : ISaveable
	{
		public TerrainSurface Surfaces;
		public float[,] Height;

        public ISaveable CollectData()
        {
            return this;
        }
	}
}