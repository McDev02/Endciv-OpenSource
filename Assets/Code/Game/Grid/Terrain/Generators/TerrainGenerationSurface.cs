using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{

    public class TerrainGenerationSurface
    {
        public float[,] TmpBuffer;
        public float[,] TmpBuffer2;

        public int Size;

        public int LayerCount { get; private set; }
        public List<float[,]> Data;
        public TerrainGenerationSurface(int size, int layerCount)
        {
            TmpBuffer = new float[size, size];
            TmpBuffer2 = new float[size, size];

            Size = size;
            LayerCount = layerCount;
            Data = new List<float[,]>(layerCount);
            //Heightmap
            for (int i = 0; i < layerCount; i++)
            {
                Data.Add(new float[Size, Size]);
            }
        }

        public TerrainGenerationSurface(int size, int layerCount, Texture2D texture)
            : this(size, layerCount)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    TmpBuffer[x, y] = texture.GetPixelBilinear((float)x / (float)Size, (float)y / (float)Size).r;
                }
            }

            SetLayer((int)TerrainGenerationManager.ETerrainLayer.Height, TmpBuffer);
        }

        public float[,] GetLayer(int layer)
        {
            return Data[layer];
        }

        public void SetLayer(int layer, float[,] data)
        {
			//Why tmp?
			float[,] tmp = data;
            Data[layer] = tmp;
        }

        public float SampleBilinear(Vector2 uv, int layer)
        {
            return SampleBilinear(uv.x, uv.y, layer);
        }

        public float SampleBilinear(float u, float v, int layer )
        {
            u *= (Size - 1);
            v *= (Size - 1);

            int px = (int)(u); // floor of x
            int py = (int)(v); // floor of y

            int mx = px + 1;
            if (mx >= Size) mx = Size - 1;
            int my = py + 1;
            if (my >= Size) my = Size - 1;

            float p1 = 0;
            float p2 = 0;
            float p3 = 0;
            float p4 = 0;

            // load the four neighboring point
            p1 = GetLayer(layer)[px, py];
            p2 = GetLayer(layer)[mx, py];
            p3 = GetLayer(layer)[px, my];
            p4 = GetLayer(layer)[mx, my];

            // Calculate the weights
            float wx = u - px;
            float wy = v - py;

            float w4 = wx * wy;
            float w3 = (1f - wx) * wy;
            float w2 = wx * (1f - wy);
            float w1 = (1f - wx) * (1f - wy);

            float result = w1 * p1 + w2 * p2 + w3 * p3 + w4 * p4;
            return result;
        }

        public void ConvertToTerrainHeight()
        {
            TmpBuffer = GetLayer((int)TerrainGenerationManager.ETerrainLayer.Height);

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    TmpBuffer[x, y] = (TmpBuffer[x, y] + 1) * (1f / 2f);
                }
            }
            SetLayer((int)TerrainGenerationManager.ETerrainLayer.Height, TmpBuffer);
        }
    }
}