//#define LogTime

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Endciv
{
	public partial class GridMap : MonoBehaviour, IRunLogic
	{
		public bool IsRunning { get; private set; }

		public enum EState { Idle, Processing, Applying }
		public EState currentProcessingState;

		public Action OnLayersUpdated;

		float[,] tmpCityDensity;
		float[,] tmpPollution;

		const float pollutionAdaption = 0.1f;
		const float pollutionReduction = 0.1f;
		const float pollutionWasteMinValue = 0.3f;
		const float pollutionWaste = 1.5f + pollutionWasteMinValue;

		const int GRIDMAP_UPDATE_ITERATION = 20;
		float updateGridMapCounter;
		Dictionary<BaseEntity, StructureData> structureDataLookup;
		Dictionary<Vector2i, List<StructureData>> structureRectGraph;

#if LogTime
		Stopwatch watch = new Stopwatch();
#endif

		class StructureData
		{
			public RectBounds rect;
			public float density;
			public float pollution;
		}
		public void Run()
		{
			if (IsRunning) return;
			IsRunning = true;
			structureSystem = gameManager.SystemsManager.StructureSystem;

			structureSystem.OnStructureAdded -= UpdateStructureData;
			structureSystem.OnStructureRemoved -= UpdateStructureData;
			structureSystem.OnStructureAdded += UpdateStructureData;
			structureSystem.OnStructureRemoved += UpdateStructureData;

			int simulationWidth = Mathf.CeilToInt(Width / 2f);
			int simulationLength = Mathf.CeilToInt(Length / 2f);
			tmpCityDensity = new float[simulationWidth, simulationLength];
			tmpPollution = new float[simulationWidth, simulationLength];

			var partitions = partitionSystem.GetPartitions();
			int countX = partitions.GetLength(0);
			int countY = partitions.GetLength(1);
			structureDataLookup = new Dictionary<BaseEntity, StructureData>(32);
			structureRectGraph = new Dictionary<Vector2i, List<StructureData>>(countX * countY);
			for (int x = 0; x < countX; x++)
			{
				for (int y = 0; y < countY; y++)
				{
					structureRectGraph.Add(new Vector2i(x, y), new List<StructureData>());
				}
			}

			StartCoroutine(SimuateDataMap());
		}


		//private System.Diagnostics.Stopwatch stopwatch;

		/// <summary>
		/// This needs to be optimized further
		/// </summary>
		IEnumerator SimuateDataMap()
		{
			while (IsRunning)
			{
				switch (currentProcessingState)
				{
					case EState.Idle:
						if (updateGridMapCounter <= 0)
						{
							updateGridMapCounter = GRIDMAP_UPDATE_ITERATION;
							currentProcessingState =  EState.Processing;
#if LogTime
							watch.Reset();
							watch.Start();
#endif
							GetStructureRectGraph(2, SystemsManager.MainPlayerFaction);
#if LogTime
							watch.LogRound("GetStructureRectGraph");
#endif

							Task.Run(() => { GridMapSimulation(); });
							//yield return GridMapSimulation();
							//Debug.Break();
						}
						break;
					case EState.Processing:
						break;
					case EState.Applying:
#if LogTime
						watch.Reset();
						watch.Start();
#endif
						//for (int x = 0; x < Width; x++)
						//{
						//	int xd = (int)(x / 2f);
						//	for (int y = 0; y < Length; y++)
						//	{
						//		int yd = (int)(y / 2f);
						//
						//		//TODO: Add bilinear sampling
						//		Data.pollution[x, y] = tmpPollution[xd, yd];
						//		Data.cityDensity[x, y] = tmpCityDensity[xd, yd];
						//	}
						//}
#if LogTime
						watch.LogRound("Applying");
#endif
						yield return null;
						OnLayersUpdated?.Invoke();
						currentProcessingState = EState.Idle;
						break;
				}

				updateGridMapCounter -= Main.deltaTimeSafe;

				yield return null;
			}
		}

		void GridMapSimulation()
		{
#if LogTime
			watch.Reset();
			watch.Start();
			var watch1 = new Endciv.Stopwatch();
			var watch2 = new Endciv.Stopwatch();
#endif
			int openAreaDistance = (int)Mathf.Max(0, GameConfig.Instance.OpenAreaDistance);

			float distFactor = 1f / GameConfig.Instance.CityDensityMaxDistance;
			float dist;

			//watch1.Start();
			//Grab structures from the player
			//GetStructureRectGraph(2, SystemsManager.MainPlayerFaction);
			//watch1.LogRound("GetStructureRectGraph");

			//yield return null;
			//watch1.Stop();
			//watch1.Reset();

			float cityDensity = 0;
			float pollution = 0;
			float basePollution = 0;

			Vector2i nodeID = Vector2i.Zero;
			Vector2i otherID;
			int xd, yd;
			for (int x = 0; x < Width; x++)
			{
				xd = (int)(x / 2f);
				nodeID.X = x;
				for (int y = 0; y < Length; y++)
				{
					yd = (int)(y / 2f);
					nodeID.Y = y;

					//Downsampling
					if (x % 2 == 0 && y % 2 == 0)
					{
#if LogTime
						watch1.Start();
#endif
						cityDensity = 0;
						pollution = 0;

						var partitionID = partitionSystem.SamplePartitionID(nodeID);
						var structures = structureRectGraph[partitionID];

						//Calculation for each entity
						for (int i = 0; i < structures.Count; i++)
						{
							var data = structures[i];
							dist = CivMath.DistanceFromPointToRect(nodeID, data.rect);
							dist = Mathf.Clamp01((GameConfig.Instance.CityDensityMaxDistance - dist) * distFactor);
							cityDensity += dist * data.density;
							pollution += dist * data.pollution;
						}
						pollution = Mathf.Pow(Mathf.Max(0, pollution - pollutionWasteMinValue), GameConfig.Instance.PollutionDistancePower);
						lock (Data.pollution)
						{
							basePollution = Mathf.Max(0, Data.waste[x, y] - pollutionWasteMinValue) * pollutionWaste;
							basePollution = Mathf.Max(Data.pollution[x, y], basePollution);
						}
						//Simulate adaption
						if (basePollution < pollution)
							pollution = Mathf.Min(pollution, basePollution + pollutionAdaption);
						else
							pollution = Mathf.Max(pollution, basePollution - pollutionReduction);

						cityDensity = Mathf.Pow(cityDensity, GameConfig.Instance.CityDensityDistancePower);
						tmpCityDensity[xd, yd] = Mathf.Clamp(cityDensity, 0, GameConfig.Instance.CityDensityMaxValue);
						tmpPollution[xd, yd] = Mathf.Clamp(pollution, 0, GameConfig.Instance.PollutionMaxValue);
						
						//apply here
						lock (Data.cityDensity)
						{
							Data.cityDensity[x, y] = tmpCityDensity[xd, yd];
							Data.cityDensity[x + 1, y] = tmpCityDensity[xd, yd];
							Data.cityDensity[x, y + 1] = tmpCityDensity[xd, yd];
							Data.cityDensity[x + 1, y + 1] = tmpCityDensity[xd, yd];
						}
						lock (Data.pollution)
						{
							Data.pollution[x, y] = tmpPollution[xd, yd];
							Data.pollution[x + 1, y] = tmpPollution[xd, yd];
							Data.pollution[x, y + 1] = tmpPollution[xd, yd];
							Data.pollution[x + 1, y + 1] = tmpPollution[xd, yd];
						}
#if LogTime
						watch1.Stop();
#endif
					}

					//Open Area
#if LogTime
					watch2.Start();
#endif
					dist = openAreaDistance + 1;
					for (int ix = -openAreaDistance; ix <= openAreaDistance; ix++)
					{
						for (int iy = -openAreaDistance; iy <= openAreaDistance; iy++)
						{
							otherID = new Vector2i(x + ix, y + iy);
							if (!Grid.IsInRange(otherID)) continue;
							if (Data.occupied[otherID.X, otherID.Y] != EGridOccupation.Free || Data.passability[otherID.X, otherID.Y] <= 0.1f)
								dist = Mathf.Min(dist, (otherID - nodeID).Magnitude);
						}
					}
					Data.openArea[x, y] = dist;
#if LogTime
					watch2.Stop();
#endif
					//Debug.Log("Map Simulation : " + stopwatch.Elapsed.TotalMilliseconds.ToString());
					//yield return null;
				}
			}
			currentProcessingState = EState.Applying;
#if LogTime
			Logger.Log("End City Density calculation");
			watch1.LogTotal("City Density");
			watch2.LogTotal("Open Area");
			//OnCityDensityUpdated?.Invoke();
#endif
		}


		/// <summary>
		/// Returns the rects of every structure within a certain partition radius 
		/// from every partition ID
		/// </summary>
		/// <param name="partitionRadius"></param>
		/// <returns></returns>
		public void GetStructureRectGraph(int partitionRadius, int factionID)
		{
			var structures = structureSystem.FeaturesByFaction;
			var partitions = partitionSystem.GetPartitions();

			//Todo this seems to produce too much garbadge. We could cache the data, as we now use a class we can make changes to that class.
			//foreach (var item in structureRectGraph)
			//{
			//	var list = item.Value;
			//	list.Clear();
			//}

			for (int f = 0; f < structures.Count; f++)
			{
				var count = structures[f].Count;
				for (int i = 0; i < count; i++)
				{
					//Check if this is a valid entity
					var structure = structures[f][i];
					var entityFeature = structure.Entity.GetFeature<EntityFeature>();
					var gridObject = structure.Entity.GetFeature<GridObjectFeature>();

					if (entityFeature == null || gridObject == null) continue;

					StructureData data;
					//Collect data of this structure and box it
					if (structureDataLookup.ContainsKey(structure.Entity))
						data = structureDataLookup[structure.Entity];
					else
					{
						data = new StructureData();
						data.rect = gridObject.GridObjectData.Rect;

						structureDataLookup.Add(structure.Entity, data);

						//Calculate sourrounding partitions of this structure
						RectBounds partitionRect = new RectBounds(gridObject.PartitionIDs[0]);
						for (int pi = 1; pi < gridObject.PartitionIDs.Count; pi++)
						{
							partitionRect.Insert(gridObject.PartitionIDs[pi]);
						}
						partitionRect.Extend(Mathf.Max(0, partitionRadius));
						partitionRect.Clamp(partitions.GetLength(0), partitions.GetLength(1));

						//Populate dictionary for each partition
						for (int px = partitionRect.X; px <= partitionRect.Maximum.X; px++)
						{
							for (int py = partitionRect.Y; py <= partitionRect.Maximum.Y; py++)
							{
								structureRectGraph[new Vector2i(px, py)].Add(data);
							}
						}
					}
					//Apply new data
					//data.rect = gridObject.GridObjectData.Rect;

					if (structure.Entity.HasFeature<PollutionFeature>())
						data.pollution = structure.Entity.GetFeature<PollutionFeature>().pollution;
					else
						data.pollution = 0;

					data.density = Mathf.Sqrt(data.rect.Area) * 0.05f * gridObject.GridObjectData.density;
				}
			}
		}

		void UpdateStructureData()
		{
		}

		public void Stop()
		{
			IsRunning = false;
		}

	}
}