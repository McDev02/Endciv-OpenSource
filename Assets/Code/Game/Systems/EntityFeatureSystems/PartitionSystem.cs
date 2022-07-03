using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// System for GridObjects which have a grid location and which are organized in partitions.
	/// </summary>
	public class PartitionSystem : BaseGameSystem
	{
		GridMap GridMap;
		/// <summary>
		/// How many tiles  in x and y make up one partition cell
		/// </summary>
		internal int PartitionSize { get; private set; }
		internal float PartitionHalfSize { get; private set; }
		internal int PartitionsX { get; private set; }
		internal int PartitionsY { get; private set; }
		internal int MaxPartitionX { get; private set; }
		internal int MaxPartitionY { get; private set; }
		internal float partitionSizeFactor;

		internal float PartitionFactor { get; private set; }
		Vector2i tmpID;

		List<BaseEntity>[,] partitions;
		public List<BaseEntity>[,] GetPartitions() { return partitions; }

		public PartitionSystem(GridMap gridMap, int partitionSize) : base()
		{
			PartitionSize = partitionSize;
			GridMap = gridMap;
			GridMap.partitionSystem = this;
			Run();
		}

		public void Run()
		{
			partitionSizeFactor = 1f / PartitionSize;
			PartitionHalfSize = PartitionSize / 2f;
			PartitionsX = Mathf.CeilToInt((float)GridMap.Width / PartitionSize);
			PartitionsY = Mathf.CeilToInt((float)GridMap.Length / PartitionSize);
			MaxPartitionX = PartitionsX - 1;
			MaxPartitionY = PartitionsY - 1;

			PartitionFactor = 1f / PartitionSize;
			tmpID = Vector2i.Zero;

			partitions = new List<BaseEntity>[PartitionsX, PartitionsY];
			for (int x = 0; x < PartitionsX; x++)
			{
				for (int y = 0; y < PartitionsY; y++)
				{
					partitions[x, y] = new List<BaseEntity>(32);
				}
			}
			IsRunning = true;
		}

		internal void UpdateEntity(BaseEntity entity)
		{
			SetPartitionID(entity, SamplePartitionID(entity.GetFeature<EntityFeature>().GridID));
		}
		public override void UpdateStatistics()
		{
		}

		internal void RegisterStructure(BaseEntity structure, bool updateImmediately = true)
		{
			//Prevent partition id garbage in case of structures moving around the grid manually
			//UnRegisterStructure(structure); Will not happen yet

			var positions = structure.GetFeature<GridObjectFeature>().GridObjectData.Rect.Corners;
			foreach (var position in positions)
			{
				var tempID = SamplePartitionID(position);
				//Check if rect corner exists in the same partition as another corner
				if (partitions[tempID.X, tempID.Y].Contains(structure))
					continue;
				partitions[tempID.X, tempID.Y].Add(structure);
				structure.GetFeature<GridObjectFeature>().PartitionIDs.Add(tempID);
			}
			GridMap.UpdateGridObject(structure, updateImmediately);
		}

		internal void UnRegisterStructure(BaseEntity structure, bool updateImmediately = true)
		{
			bool wasRegistered = false;
			if (structure == null)
				return;
			if (!structure.HasFeature<GridObjectFeature>())
				return;
			foreach (var id in structure.GetFeature<GridObjectFeature>().PartitionIDs)
			{
				if (partitions[id.X, id.Y].Contains(structure))
				{
					wasRegistered = true;
					partitions[id.X, id.Y].Remove(structure);
				}
			}
			if (wasRegistered)
				GridMap.UpdateGridObject(structure, updateImmediately, true);
		}

		public Vector2i SamplePartitionID(Vector2 pos)
		{
			tmpID.X = CivMath.Clamp0X((int)(pos.x * PartitionFactor), MaxPartitionX);
			tmpID.Y = CivMath.Clamp0X((int)(pos.y * PartitionFactor), MaxPartitionY);
			return tmpID;
		}
		public Vector2 GetRelativePartitionPosition(Vector2i id, Vector2 pos)
		{
			return pos * PartitionFactor - id;
		}

		private void SetPartitionID(BaseEntity entity, Vector2i tmpID)
		{
			var entityFeature = entity.GetFeature<EntityFeature>();
			if (entityFeature.PartitionID == tmpID)
				return;
			var oldID = entityFeature.PartitionID;
			if (partitions[oldID.X, oldID.Y] != null && IsInPartitionRange(oldID))
				partitions[oldID.X, oldID.Y].Remove(entity);
			if (partitions[tmpID.X, tmpID.Y] == null)
				partitions[tmpID.X, tmpID.Y] = new List<BaseEntity>();
			partitions[tmpID.X, tmpID.Y].Add(entity);
			entityFeature.PartitionID = tmpID;
		}

		public bool IsInPartitionRange(Vector2i id)
		{
			return !(id.X < 0 || id.X >= PartitionsX || id.Y < 0 || id.Y >= PartitionsY);
		}

		public override void UpdateGameLoop()
		{
		}

		Vector2i getRectID(Vector2i tileID)
		{
			tileID.X = Mathf.Clamp((int)(tileID.X * partitionSizeFactor), 0, MaxPartitionX);
			tileID.Y = Mathf.Clamp((int)(tileID.Y * partitionSizeFactor), 0, MaxPartitionY);
			return tileID;
		}

		/// <summary>
		/// Returns the 8 nearest tiles where the main tile is always in the center.
		/// </summary>
		/// <param name="partitionID">Main partition ID</param>
		/// <param name="IncludeCurrent">Defines if the main tile ID will be inside the list or not</param>
		/// <returns></returns>
		public List<Vector2i> GetAdjacentTiles(Vector2i partitionID, bool IncludeCurrent = true)
		{
			List<Vector2i> result = new List<Vector2i>(9);
			Vector2i id = partitionID;

			for (int x = -1; x <= 1; x++)
			{
				id.X = partitionID.X + x;
				for (int y = -1; y <= 1; y++)
				{
					id.Y = partitionID.Y + y;
					if (!IncludeCurrent && x == 0 && y == 0)
						continue;
					result.Add(id);
				}
			}
			return result;
		}

		/// <summary>
		/// Returns the 8 nearest tiles where the main partition is always in the center.
		/// </summary>
		/// <param name="partitionID">Main partition ID</param>
		/// <param name="IncludeCurrent">Defines if the main partition ID will be inside the list or not</param>
		/// <returns></returns>
		public List<Vector2i> GetAdjacentPartitions(Vector2i partitionID, bool IncludeCurrent = true, int partitionRadius = 1)
		{
			List<Vector2i> result = new List<Vector2i>(9);
			Vector2i id = partitionID;

			for (int x = -partitionRadius; x <= partitionRadius; x++)
			{
				id.X = partitionID.X + x;
				for (int y = -partitionRadius; y <= partitionRadius; y++)
				{
					id.Y = partitionID.Y + y;
					if (!IncludeCurrent && x == 0 && y == 0) continue;
					if (IsInPartitionRange(id)) result.Add(id);
				}
			}
			return result;
		}
		/// <summary>
		/// Returns an optimized result of the nearest Partitions of partitionID;
		/// </summary>
		/// <param name="relativePosition">Calculated with GridPartitionView.GetRelativePartitionPosition()</param>
		/// <param name="radius">The extended radius on top of the nearest partitions. 0 is default which will return up to 3 neighbours.</param>
		/// <param name="IncludeCurrent">Defines if partitionID will be inside the returned list or not</param>
		/// <returns></returns>
		public List<Vector2i> GetAdjacentPartitions(Vector2i partitionID, Vector2 relativePosition, int radius = 0, bool IncludeCurrent = true)
		{
			List<Vector2i> result = new List<Vector2i>(9);
			Vector2i id = partitionID;
			relativePosition -= new Vector2(0.5f, 0.5f);
			Vector2i off = Vector2i.One * radius;
			Vector2i min = CivMath.FloorVectorInt(relativePosition) - off;
			Vector2i max = CivMath.CeilVectorInt(relativePosition) + off;

			for (int x = min.X; x <= max.X; x++)
			{
				id.X = partitionID.X + x;
				for (int y = min.Y; y <= max.Y; y++)
				{
					id.Y = partitionID.Y + y;
					if (!IncludeCurrent && x == 0 && y == 0) continue;
					if (IsInPartitionRange(id)) result.Add(id);
				}
			}
			return result;
		}


		/// <summary>
		/// Returns all Structures within (intersecting) a rect
		/// </summary>
		internal List<BaseEntity> GetStructuresInRect(RectBounds rect, bool countEntrances = false)
		{
			List<BaseEntity> entities = new List<BaseEntity>();
			var min = getRectID(rect.Minimum);
			var max = getRectID(rect.Maximum);

			for (int x = min.X; x <= max.X; x++)
			{
				for (int y = min.Y; y <= max.Y; y++)
				{
					var objects = partitions[x, y];
					for (int i = 0; i < objects.Count; i++)
					{
						if (!objects[i].HasFeature<StructureFeature>() && !objects[i].HasFeature<ResourcePileFeature>())
							continue;
						var structure = objects[i];
						var structureRect = structure.GetFeature<GridObjectFeature>().GridObjectData.Rect;
						if (countEntrances)
						{
							for (int j = 0; j < structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints.Length; j++)
								structureRect.Merge(structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints[j]);
						}
						if (rect.Intersects(structureRect) && !entities.Contains(structure))
						{
							entities.Add(structure);
						}
					}
				}
			}

			return entities;
		}

		/// <summary>
		/// Returns all entity rects of specified Faction,
		/// contained in partitions within partition radius of partitionID.
		/// </summary>
		public RectBounds[] GetEntityRectsInRadius(Vector2i partitionID, int partitionRadius, int factionID)
		{
			RectBounds[] rects;

			List<List<BaseEntity>> entities = new List<List<BaseEntity>>();
			List<Vector2i> partitionIDs = GetAdjacentPartitions(partitionID, true, partitionRadius);
			int count = 0;
			foreach (var id in partitionIDs)
			{
				count += partitions[id.X, id.Y].Count;
				entities.Add(partitions[id.X, id.Y]);
			}
			List<RectBounds> filteredList = new List<RectBounds>(count);
			foreach (var list in entities)
			{
				foreach (var entity in list)
				{
					if (entity.factionID != factionID)
						continue;
					if (!entity.HasFeature<GridObjectFeature>())
						continue;
					filteredList.Add(entity.GetFeature<GridObjectFeature>().GridObjectData.Rect);
				}
			}
			rects = filteredList.ToArray();
			return rects;
		}

		/// <summary>
		/// Returns the rects of every structure within a certain partition radius 
		/// from every partition ID
		/// </summary>
		/// <param name="partitionRadius"></param>
		/// <returns></returns>
		public Dictionary<Vector2i, RectBounds[]> GetStructureRectGraph(int partitionRadius, int factionID)
		{
			var graph = new Dictionary<Vector2i, RectBounds[]>();
			for (int x = 0; x < partitions.GetLength(0); x++)
			{
				for (int y = 0; y < partitions.GetLength(1); y++)
				{
					var id = new Vector2i(x, y);
					var rects = GetEntityRectsInRadius(id, partitionRadius, factionID);
					graph.Add(id, rects);
				}
			}
			return graph;
		}

		/// <summary>
		/// Returns all entities approximately contained in partitions within the tile radius.
		/// </summary>
		public List<BaseEntity> GetEntitiesInRadius(Vector2i pos, int tileRadius)
		{
			int partitionRadius = (int)Mathf.Round(tileRadius / PartitionSize);
			var list = new List<BaseEntity>();
			var partitionID = SamplePartitionID(pos);
			// Rectangular distance testing to avoid square roots
			// We don't test for out of bounds coordinates 
			// as entities can never have out of bounds PartitionIDs.
			for (int i = 0; i < partitions.GetLength(0); i++)
			{
				if (Mathf.Abs(partitionID.X - i) > partitionRadius)
					continue;
				for (int j = 0; j < partitions.GetLength(1); j++)
				{
					if (Mathf.Abs(partitionID.Y - j) > partitionRadius)
						continue;
					list.AddRange(partitions[i, j]);
				}
			}
			return list;
		}

		/// <summary>
		/// Returns all entities within the tile distance.
		/// </summary>
		public List<BaseEntity> GetEntitiesInRadius(Vector2i pos, float tileRadius)
		{
			int partitionRadius = (int)Mathf.Round(tileRadius / PartitionSize);
			var list = new List<BaseEntity>();
			var partitionID = SamplePartitionID(pos);
			var entities = new List<BaseEntity>();
			for (int i = 0; i < partitions.GetLength(0); i++)
			{
				if (Mathf.Abs(partitionID.X - i) > partitionRadius)
					continue;
				for (int j = 0; j < partitions.GetLength(1); j++)
				{
					if (Mathf.Abs(partitionID.Y - j) > partitionRadius)
						continue;
					for (int k = 0; k < partitions[i, j].Count; k++)
					{
						if (Vector2i.Distance(pos, partitions[i, j][k].GetFeature<EntityFeature>().GridID) > tileRadius)
						{
							continue;
						}
						entities.Add(partitions[i, j][k]);
					}
				}
			}
			return entities;
		}

		public Vector2i[] GetGridMapIndicesAtPartition(Vector2i partitionIndex)
		{
			Vector2i[] partitionIndices = new Vector2i[PartitionSize * PartitionSize];
			int count = 0;
			for (int i = 0; i < PartitionSize; i++)
			{
				for (int j = 0; j < PartitionSize; j++)
				{
					partitionIndices[count] = new Vector2i((partitionIndex.X * PartitionSize) + j, (partitionIndex.Y * PartitionSize) + i);
					count++;
				}
			}
			return partitionIndices;
		}

	}
}