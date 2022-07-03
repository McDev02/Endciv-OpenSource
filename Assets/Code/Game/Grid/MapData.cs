using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Endciv
{
	[Serializable]
	public class MapDataSaveData : ISaveable
	{
		public float[,] fertileLand;
		public float[,] waste;

		public ISaveable CollectData()
		{
			return this;
		}
	}

	public enum EGridOccupation { Free, StayFree, Occupied }

	/// <summary>
	/// Model Class for a Rectangular Grid (Cell points)
	/// </summary>
	public class MapData : ISaveable, ILoadable<MapDataSaveData>
	{
		//Grid Data (Defined by objects)
		public EGridOccupation[,] occupied;
		public Map2D<float> passability;
		//maybe we need a float per ID (Density)
		public Map2D<int> factionID;
		//Used for planning mode
		public bool[,] reserved;	

		public bool dirtyMapData;

		//MapLayer Data (derrived on runtime)
		public Map2D<float> beauty;
		public Map2D<float> pollution;
		public Map2D<float> groundWater;
		public Map2D<float> fertility;
		public Map2D<float> cityDensity;
		public Map2D<float> openArea;
		//Todo add new layers to summary

		//Terrain surfaces
		public Map2D<float> fertileLand;
		public Map2D<float> waste;

		#region Mipmaps
		public PartitionSummary[,] passabilitySummary;
		public Stack<Vector2i> passabilityChangedIndices;

		//MapLayer Data (derrived on runtime)
		public PartitionSummary[,] beautySummary;
		public Stack<Vector2i> beautyChangedIndices;

		public PartitionSummary[,] pollutionSummary;
		public Stack<Vector2i> pollutionChangedIndices;

		public PartitionSummary[,] groundWaterSummary;
		public Stack<Vector2i> groundWaterChangedIndices;

		public PartitionSummary[,] fertilitySummary;
		public Stack<Vector2i> fertilityChangedIndices;

		public PartitionSummary[,] cityDensitySummary;
		public Stack<Vector2i> cityDensityChangedIndices;

		public PartitionSummary[,] openAreaSummary;
		public Stack<Vector2i> openAreaChangedIndices;

		//Terrain surfaces
		public PartitionSummary[,] fertileLandSummary;
		public Stack<Vector2i> fertileLandChangedIndices;

		public PartitionSummary[,] wasteSummary;
		public Stack<Vector2i> wasteChangedIndices;
		#endregion

		public bool dirtySurfaces;

		public int Width { get; private set; }
		public int Length { get; private set; }

		public MapData(int width, int length, int kernelSize)
		{
			//GridData
			occupied = new EGridOccupation[width, length];
			passability = new Map2D<float>(width, length);
			factionID = new Map2D<int>(width, length);
			passabilityChangedIndices = new Stack<Vector2i>();
			passability.PropertyChanged += (Vector2i index, float value) =>
			{
				passabilityChangedIndices.Push(index);
			};
			reserved = new bool[width, length];

			//MapLayer
			beauty = new Map2D<float>(width, length);
			beautyChangedIndices = new Stack<Vector2i>();
			beauty.PropertyChanged += (Vector2i index, float value) =>
			{
				beautyChangedIndices.Push(index);
			};
			pollution = new Map2D<float>(width, length);
			pollutionChangedIndices = new Stack<Vector2i>();
			pollution.PropertyChanged += (Vector2i index, float value) =>
			{
				pollutionChangedIndices.Push(index);
			};
			groundWater = new Map2D<float>(width, length);
			groundWaterChangedIndices = new Stack<Vector2i>();
			groundWater.PropertyChanged += (Vector2i index, float value) =>
			{
				groundWaterChangedIndices.Push(index);
			};
			fertility = new Map2D<float>(width, length);
			fertilityChangedIndices = new Stack<Vector2i>();
			fertility.PropertyChanged += (Vector2i index, float value) =>
			{
				fertilityChangedIndices.Push(index);
			};
			cityDensity = new Map2D<float>(width, length);
			cityDensityChangedIndices = new Stack<Vector2i>();
			cityDensity.PropertyChanged += (Vector2i index, float value) =>
			{
				cityDensityChangedIndices.Push(index);
			};
			openArea = new Map2D<float>(width, length);
			openAreaChangedIndices = new Stack<Vector2i>();
			openArea.PropertyChanged += (Vector2i index, float value) =>
			{
				openAreaChangedIndices.Push(index);
			};

			//Terrain layer
			waste = new Map2D<float>(width, length);
			wasteChangedIndices = new Stack<Vector2i>();
			waste.PropertyChanged += (Vector2i index, float value) =>
			{
				wasteChangedIndices.Push(index);
			};
			fertileLand = new Map2D<float>(width, length);
			fertileLandChangedIndices = new Stack<Vector2i>();
			fertileLand.PropertyChanged += (Vector2i index, float value) =>
			{
				fertileLandChangedIndices.Push(index);
			};

			//MipMaps
			int mipWidth = width / kernelSize;
			int mipLength = length / kernelSize;

			passabilitySummary = new PartitionSummary[mipWidth, mipLength];
			beautySummary = new PartitionSummary[mipWidth, mipLength];
			pollutionSummary = new PartitionSummary[mipWidth, mipLength];
	groundWaterSummary = new PartitionSummary[mipWidth, mipLength];
			fertilitySummary = new PartitionSummary[mipWidth, mipLength];
			cityDensitySummary = new PartitionSummary[mipWidth, mipLength];
			openAreaSummary = new PartitionSummary[mipWidth, mipLength];
			fertileLandSummary = new PartitionSummary[mipWidth, mipLength];
			wasteSummary = new PartitionSummary[mipWidth, mipLength];

			Width = width;
			Length = length;
		}

		#region Data Models
		public struct GridData
		{
			public EGridOccupation Occupied;
			public float Passability;
			//maybe we need a float per ID (Density)
			public int FactionID;

			public void Reset()
			{
				Occupied = EGridOccupation.Free;
				Passability = 1;
				FactionID = SystemsManager.NoFaction;
			}
		}

		public struct MapLayer
		{
			public float Beauty;
			public float Waste;
			public float Pollution;
			public float Fertility;
		}

		public struct PartitionSummary
		{
			public float NodeAverage;
			public float NodeMin;
			public float NodeMax;
		}
		#endregion

		#region Model Access
		public GridData GetGridData(Vector2i pos)
		{
			return GetGridData(pos.X, pos.Y);
		}
		public GridData GetGridData(int x, int y)
		{
			var data = new GridData();

			data.Occupied = occupied[x, y];
			data.Passability = passability[x, y];
			data.FactionID = factionID[x, y];

			return data;
		}
		public void SetGridData(Vector2i pos, GridData data)
		{
			SetGridData(pos.X, pos.Y, data);
		}
		public void SetGridData(int x, int y, GridData data)
		{
			occupied[x, y] = data.Occupied;
			passability[x, y] = data.Passability;
			factionID[x, y] = data.FactionID;
			dirtyMapData = true;
		}

		public MapLayer GetMapLayer(int x, int y)
		{
			var data = new MapLayer();

			data.Waste = waste[x, y];
			data.Pollution = pollution[x, y];
			data.Fertility = fertility[x, y];

			return data;
		}

		public MapLayer GetMapLayerBilinear(float x, float y)
		{
			var data = new MapLayer();

			int minX = (int)x;
			int minY = (int)y;
			int maxX = minX + 1;
			int maxY = minY + 1;
			float dX = x - minX;
			float dY = y - minY;
			float dX2 = 1 - dX;
			float dY2 = 1 - dY;

			data.Waste = dY * (dX * waste[minX, minY] + dX2 * waste[maxX, minY]) + dY2 * (dX * waste[minX, maxY] + dX2 * waste[maxX, maxY]);
			data.Pollution = dY * (dX * pollution[minX, minY] + dX2 * pollution[maxX, minY]) + dY2 * (dX * pollution[minX, maxY] + dX2 * pollution[maxX, maxY]);
			data.Fertility = dY * (dX * fertility[minX, minY] + dX2 * fertility[maxX, minY]) + dY2 * (dX * fertility[minX, maxY] + dX2 * fertility[maxX, maxY]);

			return data;
		}
		#endregion

		public ISaveable CollectData()
		{
			var data = new MapDataSaveData();
			data.fertileLand = fertileLand.ToArray();
			data.waste = waste.ToArray();
			return data;
		}

		public void ApplySaveData(MapDataSaveData data)
		{
			if (data == null)
				return;
			fertileLand = new Map2D<float>(data.fertileLand);
			waste = new Map2D<float>(data.waste);
		}
	}
}