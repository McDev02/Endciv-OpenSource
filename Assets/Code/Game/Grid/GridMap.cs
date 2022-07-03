using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class GridMapSaveData : ISaveable
	{
		public MapDataSaveData mapData;
		public TerrainExchangeData terrainExchangeData;

		public ISaveable CollectData()
		{
			return this;
		}
	}

	/// <summary>
	/// Controller of Grid Map, including Layers and Pathfinding data.
	/// </summary>
	public partial class GridMap : MonoBehaviour, ISaveable
	{
		public const float ConstructionSiteThreshold = 0.2f;
		public const float ConstructionSitePassability = 0.3f;
		public const int EdgePadding = 2;

		public const int IMPASSABLE_GROUPID = -1;
		public int Width { get; private set; }
		public int Length { get; private set; }
		public float GridWidth { get; private set; }
		public float GridLength { get; private set; }
		public Vector2 MapCenter { get; private set; }
		public Vector2i MapCenteri { get; private set; }

		public readonly static Vector2i[] Directions = new Vector2i[]{
			Vector2i.Up,			//North
		    Vector2i.Right,			//East 
		    Vector2i.Down, 			//South
		    Vector2i.Left,			//West 

		    new Vector2i(1,1),		//NE
		    new Vector2i(1,-1),		//SE
		    new Vector2i(-1,-1),	//SW
		    new Vector2i(-1,1),		//NW		
	    };

		GameManager gameManager;
		public PartitionSystem partitionSystem;
		public StructureSystem structureSystem;
		public TerrainData TerrainData;

		//Reference for Save/Load System
		public TerrainExchangeData TerrainExchangeData;

		public MapData Data { get; private set; }
		public RectGrid Grid { get; private set; }
		public GridMapView View;

		public Action OnGridUpdate;

		private void SetGridData(Vector2i id, EGridOccupation occupation, float passability)
		{
			Data.occupied[id.X, id.Y] = occupation;
			Data.passability[id.X, id.Y] = passability;
			Data.dirtyMapData = true;
		}

		private void SetGridData(RectBounds rect, EGridOccupation occupation, float passability)
		{
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					Data.occupied[x, y] = occupation;
					Data.passability[x, y] = passability;
					Data.dirtyMapData = true;
				}
			}
		}
		public void SetOccupation(Vector2i id, EGridOccupation occupation)
		{
			Data.occupied[id.X, id.Y] = occupation;
			Data.dirtyMapData = true;
		}
		public void SetOccupation(RectBounds rect, EGridOccupation occupation)
		{
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					Data.occupied[x, y] = occupation;
					Data.dirtyMapData = true;
				}
			}
		}
		public void SetReservation(Vector2i id, bool reservation)
		{
			Data.reserved[id.X, id.Y] = reservation;
			Data.dirtyMapData = true;
		}
		public void SetReservation(RectBounds rect, bool reservation)
		{
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					Data.reserved[x, y] = reservation;
					Data.dirtyMapData = true;
				}
			}
		}

		/// <summary>
		/// Value will be clamped to 0 and 1
		/// </summary>
		private void SetWasteLayer(RectBounds rect, float value)
		{
			rect.Clamp(Width, Length);
			value = Mathf.Clamp01(value);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					Data.waste[x, y] = value;
					Data.dirtySurfaces = true;
				}
			}
		}
		/// <summary>
		/// Value will be clamped to 0 and 1
		/// </summary>
		private void SetWasteLayer(Vector2i index, float value)
		{
			if (Grid.IsInRange(index))
			{
				Data.waste[index.X, index.Y] = Mathf.Clamp01(value);
				Data.dirtySurfaces = true;
			}
		}

		/// <summary>
		/// Value will be clamped to 0 and 1
		/// </summary>
		private void SetFertileLandLayer(RectBounds rect, float value)
		{
			rect.Clamp(Width, Length);
			value = Mathf.Clamp01(value);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					Data.fertileLand[x, y] = value;
					Data.dirtySurfaces = true;
				}
			}
		}
		/// <summary>
		/// Value will be clamped to 0 and 1
		/// </summary>
		private void SetFertileLandLayer(Vector2i index, float value)
		{
			if (Grid.IsInRange(index))
			{
				Data.fertileLand[index.X, index.Y] = Mathf.Clamp01(value);
				Data.dirtySurfaces = true;
			}
		}
		private float GetWasteLayer(RectBounds rect)
		{
			rect.Clamp(Width, Length);
			float value = 0;
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					value += Data.waste[x, y];
				}
			}
			return value;
		}
		private float GetWasteLayer(Vector2i index)
		{
			if (!Grid.IsInRange(index))
				return 0;
			return Data.waste[index.X, index.Y];
		}

		private float GetFertileLandLayer(RectBounds rect)
		{
			rect.Clamp(Width, Length);
			float value = 0;
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					value += Data.fertileLand[x, y];
				}
			}
			return value;
		}
		private float GetFertileLandLayer(Vector2i index)
		{
			if (!Grid.IsInRange(index))
				return 0;
			return Data.fertileLand[index.X, index.Y];
		}

		public void SetSize(int width, int length)
		{
			Width = width;
			Length = length;
		}

		public void CreateEmpty(GameManager gameManager, int width, int length)
		{
			Width = width;
			Length = length;
			GridWidth = width * GridMapView.GridTileFactor;
			GridLength = length * GridMapView.GridTileFactor;
			MapCenter = View.LocalToWorld(new Vector2(width / 2f, length / 2f));
			MapCenteri = View.SampleTileWorld(MapCenter);

			Setup(gameManager);
			ResetMap();
			RecalculateGrid();
			Grid.ClearGroups();
		}

		public void UpdateGridObject(BaseEntity structure, bool updateImmediately = false, bool remove = false)
		{
			var gridObject = structure.GetFeature<GridObjectFeature>();
			var rect = gridObject.GridObjectData.Rect;

			//Extend so we also check sourroundings.
			rect.Extend(2);
			rect.Clamp(Width, Length);
			//We use an even bigger rect to sample intersections, this is due to temporary GridObject behavior. If we don't do this we risk leaks in walls.
			var rectForIntersections = rect.Extended(2);
			rectForIntersections.Clamp(Width, Length);

			var intersections = partitionSystem.GetStructuresInRect(rectForIntersections);

			if (intersections.Contains(structure))
			{
				if (remove) intersections.Remove(structure);
			}
			else
			{
				if (!remove) intersections.Add(structure);
			}

			for (int x = rect.Minimum.X; x <= rect.Maximum.X; x++)
			{
				for (int y = rect.Minimum.Y; y <= rect.Maximum.Y; y++)
				{
					UpdateGridTileData(new Vector2i(x, y), intersections);
				}
			}

			//Flood fill
			if (updateImmediately)
				RecalculateGrid(rect);
		}

		private void UpdateGridTileData(Vector2i pos, List<BaseEntity> structures)
		{
			//Begin with empty data
			var data = Data.GetGridData(pos);
			data.Reset();

			//For each intersecting object we take the min/max of each value at that point
			for (int i = 0; i < structures.Count; i++)
			{
				var structure = structures[i];
				var gridObject = structures[i].GetFeature<GridObjectFeature>();
				var rect = gridObject.GridObjectData.Rect;
				if (gridObject.GridObjectData.EdgeIsWall)
					rect.Extend(-1);
				//GridObject intersects with this location
				if (rect.Contains(pos))
				{
					bool isConstructionSite = false;
					if (structure.HasFeature<ConstructionFeature>())
					{
						var construction = structure.GetFeature<ConstructionFeature>();
						if (construction.ConstructionState == ConstructionSystem.EConstructionState.Construction && construction.ConstructionProgress < ConstructionSiteThreshold)
							isConstructionSite = true;
					}


					//Always occupied
					data.Occupied = EGridOccupation.Occupied;
					if (isConstructionSite)
					{
						data.Passability = Mathf.Min(data.Passability, ConstructionSitePassability);
						data.FactionID = Mathf.Max(data.FactionID, structure.factionID);
					}
					else
					{
						data.Passability = Mathf.Min(data.Passability, gridObject.StaticData.Passability);
						data.FactionID = Mathf.Max(data.FactionID, structure.factionID);
					}
				}
				//Make outside edge not passable if it is a wall
				else if (gridObject.GridObjectData.EdgeIsWall)
				{
					rect = gridObject.GridObjectData.Rect;
					if (rect.Contains(pos))
					{
						bool isConstructionSite = false;

						if (structure.HasFeature<ConstructionFeature>())
						{
							var construction = structure.GetFeature<ConstructionFeature>();
							if (construction.ConstructionState == ConstructionSystem.EConstructionState.Construction && construction.ConstructionProgress < ConstructionSiteThreshold)
								isConstructionSite = true;
						}

						if (isConstructionSite)
						{
							data.Passability = Mathf.Min(data.Passability, ConstructionSitePassability);
							data.FactionID = Mathf.Max(data.FactionID, structure.factionID);
						}
						else
						{
							data.Passability = Mathf.Min(data.Passability, 0);
							data.FactionID = Mathf.Max(data.FactionID, structure.factionID);
						}
					}
				}

				//Entrance points				
				var entrances = gridObject.GridObjectData.EntrancePoints;
				for (int e = 0; e < entrances.Length; e++)
				{
					if (pos != entrances[e]) continue;
					if (data.Occupied < EGridOccupation.StayFree)
						data.Occupied = EGridOccupation.StayFree;
				}

			}
			//Apply new data to map
			Data.SetGridData(pos, data);
		}

		private void Setup(GameManager gameManager)
		{
			Grid = new RectGrid(Width, Length);
			Data = new MapData(Width, Length, partitionSystem.PartitionSize);
			TerrainData = new TerrainData(Width, Length);
			GenerateMipmaps();
			View.Run(this, Grid);

			this.gameManager = gameManager;
		}


		public void ResetMap()
		{
			Debug.Log("ResetMap()");
			for (int y = 0; y < Length; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					Data.occupied[x, y] = EGridOccupation.Free;
					Data.passability[x, y] = 1;

					Data.fertility[x, y] = 0;
					Data.pollution[x, y] = 0;
					Data.waste[x, y] = 0;
				}
			}
			Data.dirtyMapData = true;
		}

		public void RecalculateNodes(RectBounds rect)
		{
			//Update Nodes
			Node node;
			for (int x = rect.X; x <= rect.Maximum.X; x++)
			{
				for (int y = rect.Y; y <= rect.Maximum.Y; y++)
				{
					if (!Grid.HasNode(x, y))
						node = Grid.AddNode(x, y, View.GetTileWorldPosition(new Vector2i(x, y)).To3D());
					else node = Grid.NodeLookup[x, y];

					var impassable = Data.passability[x, y] < 0.01f;
					if (impassable)
					{
						node.GroupID = IMPASSABLE_GROUPID;
						node.LayerID = IMPASSABLE_GROUPID;
					}
					else
					{
						node.LayerID = 0;// Data.layer[x, y];
					}
				}
			}
		}
		public void RecalculateLinks(RectBounds rect)
		{
			//Update Links
			Node node;
			Vector2i nodeID = new Vector2i();
			for (int x = rect.X; x <= rect.Maximum.X; x++)
			{
				for (int y = rect.Y; y <= rect.Maximum.Y; y++)
				{
					if (!Grid.HasNode(x, y)) continue;
					nodeID.X = x; nodeID.Y = y;
					node = Grid.NodeLookup[x, y];

					UpdateNodelink(node, nodeID, Directions[(int)EDirection.East]);
					UpdateNodelink(node, nodeID, Directions[(int)EDirection.SE]);
					UpdateNodelink(node, nodeID, Directions[(int)EDirection.South]);
					UpdateNodelink(node, nodeID, Directions[(int)EDirection.SW]);
				}
			}
		}

		void UpdateNodelink(Node node, Vector2i nodeID, Vector2i direction)
		{
			Vector2i nextID = nodeID + direction;
			if (Grid.IsInRange(nextID))
			{
				var other = Grid.NodeLookup[nextID.X, nextID.Y];
				if (other != null && other.LayerID == node.LayerID) Grid.CreateLink(node, other);
				else Grid.RemoveLink(node, other);
			}
		}

		public void RecalculateGrid(bool updateGroups = false)
		{
			RecalculateGrid(new RectBounds(0, 0, Width, Length), updateGroups);
		}

		public void RecalculateGrid(RectBounds rect, bool updateGroups = true)
		{
			//UnityEngine.Debug.Log("RecalculateGrid( " + rect.ToSizeString() + " )");
			//var watch = new Stopwatch();
			//watch.Start();
			RecalculateNodes(rect);
			//watch.LogRound("RecalculateNodes");
			//Extend rect to match all links as we only look for South to East in RecalculateLinks()
			var rect2 = rect.Extended(1); rect2.Clamp(Width, Length);
			RecalculateLinks(rect2);
			//watch.LogRound("RecalculateLinks");

			//UpdateIslands
			if (updateGroups)
			{
				var rect3 = CalculateAdjacentRect(rect);
				Grid.FloodFill(rect3);
				//watch.LogRound("FloodFill");
			}

			var debug = GetComponent<GridDebugController>();
			if (debug != null)
				debug.SetGrid(Grid);
			// watch.LogRound("SetupDebug");
			// watch.LogTotal("Total");

			//Update listeners
			OnGridUpdate?.Invoke();
		}

		/// <summary>
		/// Calculates a growing rect for adjacent Impassable Objects
		/// </summary>
		RectBounds CalculateAdjacentRect(RectBounds rect)
		{
			List<Node> visited;
			for (int x = rect.X; x <= rect.Maximum.X; x++)
			{
				for (int y = rect.Y; y <= rect.Maximum.Y; y++)
				{
					if (Data.passability[x, y] <= 0)
						return Grid.FloodFillFindRect(IMPASSABLE_GROUPID, new Vector2i(x, y), out visited);
				}
			}

			return rect;
		}

		public int GetGroupID(Vector2i id)
		{
			var node = Grid.NodeLookup[id.X, id.Y];
			return node.GroupID;
		}

		public bool IsOccupied(Vector2i id, bool CheckIfMustStayFree = true)
		{
			var min = CheckIfMustStayFree ? EGridOccupation.StayFree : EGridOccupation.Occupied;
			if (!Grid.IsInRange(id)) return true;
			return Data.occupied[id.X, id.Y] >= min;
		}
		public bool IsReserved(Vector2i id)
		{
			if (!Grid.IsInRange(id)) return true;
			return Data.reserved[id.X, id.Y];
		}

		public bool IsStayFree(Vector2i id)
		{
			if (!Grid.IsInRange(id)) return true;
			return Data.occupied[id.X, id.Y] == EGridOccupation.StayFree;
		}

		public bool IsOccupied(RectBounds rect, bool CheckIfMustStayFree = true)
		{
			var min = CheckIfMustStayFree ? EGridOccupation.StayFree : EGridOccupation.Occupied;
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					if (Data.occupied[x, y] >= min)
					{
						return true;
					}
				}
			}
			return false;
		}
		public bool IsReserved(RectBounds rect)
		{
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					if (Grid.IsInRange(x, y) && Data.reserved[x, y])
					{
						return true;
					}
				}
			}
			return false;
		}
		public bool IsStayFree(RectBounds rect)
		{
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					if (Data.occupied[x, y] == EGridOccupation.StayFree)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsPassable(Vector2i id)
		{
			if (!Grid.IsInRange(id))
			{
				return false;
			}
			return Data.passability[id.X, id.Y] > 0;
		}
		public bool IsPassableAndNotOccupied(Vector2i id)
		{
			if (!Grid.IsInRange(id))
			{
				return false;
			}
			return Data.passability[id.X, id.Y] > 0 && Data.occupied[id.X, id.Y] != EGridOccupation.Occupied;
		}

		public bool IsPassable(RectBounds rect)
		{
			rect.Clamp(Width, Length);
			for (int y = rect.Y; y < rect.Y + rect.Length; y++)
			{
				for (int x = rect.X; x < rect.X + rect.Width; x++)
				{
					if (Data.passability[x, y] <= 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool FindRandomEmptyEdgePoint(out Vector2i v)
		{
			v = Vector2i.Zero;
			EDirection dir;
			for (int i = 0; i < 100; i++)
			{
				dir = CivRandom.RandomDirection(false);
				switch (dir)
				{
					case EDirection.North:
						v.Y = Length - 1;
						v.X = CivRandom.Range(0, Width);
						break;
					case EDirection.East:
						v.X = Length - 1;
						v.Y = CivRandom.Range(0, Length);
						break;
					case EDirection.South:
						v.Y = 0;
						v.X = CivRandom.Range(0, Width);
						break;
					case EDirection.West:
						v.X = 0;
						v.Y = CivRandom.Range(0, Length);
						break;
				}
				//TODO: Check for walkability and island ID
				if (IsPassable(v))
					return true;
			}
			return false;
		}
		public bool FindClosestEmptyTile(Vector2i startPos, float maxDistance, out Vector2i tile)
		{
			tile = new Vector2i(startPos.X, startPos.Y);

			//Spiral check algorithm starting at startPos
			float radius = 0;
			++tile.X;
			radius += 2;
			while (Vector2i.Distance(startPos, tile) <= maxDistance)
			{
				for (int c = tile.Y; tile.Y < c + radius; tile.Y++)
				{
					if (IsPassableAndNotOccupied(tile))
					{
						return true;
					}

				}
				--tile.Y;
				--tile.X;
				for (int c = tile.X; tile.X > c - radius + 1; tile.X--)
				{
					if (IsPassableAndNotOccupied(tile))
					{
						return true;
					}

				}
				if (IsPassableAndNotOccupied(tile))
					return true;
				--tile.Y;
				for (int c = tile.Y; tile.Y > c - radius; tile.Y--)
				{
					if (IsPassableAndNotOccupied(tile))
						return true;
				}
				++tile.X;
				++tile.Y;
				for (int c = tile.X; tile.X < c + radius; tile.X++)
				{
					if (IsPassableAndNotOccupied(tile))
						return true;
				}
				radius += 2;
			}
			Debug.LogError("No passable path found!");
			tile = default(Vector2i);
			return false;
		}

		/// <summary>
		/// Find random position in whole map
		/// </summary>
		public Vector2i GetRandomPosition(bool noPadding = false)
		{
			if (noPadding)
				return new Vector2i(CivRandom.Range(0, Width), CivRandom.Range(0, Length));
			else
				return new Vector2i(CivRandom.Range(EdgePadding, Width - EdgePadding), CivRandom.Range(EdgePadding, Length - EdgePadding));
		}
		/// <summary>
		/// Find random passable position in whole map
		/// </summary>
		public bool GetRandomPassablePosition(out Vector2i v)
		{
			v = Vector2i.Zero;
			for (int i = 0; i < 1000; i++)
			{
				v = GetRandomPosition(false);
				if (IsPassable(v)) return true;
			}
			return false;
		}

		public bool GetPossitionNearPlayerTown(out Vector2i v)
		{
			v = Vector2i.Zero;
			//Get possition near town
			for (int i = 0; i < 500; i++)
			{
				v = GetRandomPosition(true);
				if (IsPassable(v) && Data.cityDensity[v.X, v.Y] > 0f && Data.cityDensity[v.X, v.Y] < 0.5f)
					return true;
			}
			//Even closer
			for (int i = 0; i < 1000; i++)
			{
				v = GetRandomPosition(true);
				if (IsPassable(v) && Data.cityDensity[v.X, v.Y] > 0f && Data.cityDensity[v.X, v.Y] < 1f)
					return true;
			}
			//Else find in town place
			for (int i = 0; i < 500; i++)
			{
				v = GetRandomPosition(true);
				if (IsPassable(v) && Data.cityDensity[v.X, v.Y] > 0f)
					return true;
			}
			//Else find any passable position
			for (int i = 0; i < 1000; i++)
			{
				v = GetRandomPosition(true);
				if (IsPassable(v))
					return true;
			}
			return false;
		}

		public bool GetRandomPositionAroundPoint(Vector2i point, out Vector2i position)
		{
			position = default(Vector2i);
			for(int i = 2; i < Grid.Width; i++)
			{
				var neighbors = GetNeighborsAtRange(point, i);
				var rnd = new System.Random();
				neighbors.Shuffle();
				for(int j  = 0; j < neighbors.Length; j++)
				{
					if (neighbors[j] == null)
						continue;
					position = neighbors[j].Value;
					return true;
				}				
			}
			return false;
		}

		private Vector2i?[] GetNeighborsAtRange(Vector2i center, int range)
		{
			var neighbors = new Vector2i?[range * 8];
			if (range <= 0)
				return neighbors;
			int side = (range * 2) + 1;
			int half = (int)Mathf.Floor(side / 2f);
			int index = 0;
			Vector2i point;
			for(int x = -half; x <= half; x++)
			{
				//Left or Right columns
				if(x == -half || x == half)
				{
					for(int y = -half + 1; y <= half - 1; y++)
					{
						point = center + new Vector2i(x, y);
						if (IsPassable(point) && IsTileInGrid(point) && !IsOccupied(point))
							neighbors[index] = point;
						else
							neighbors[index] = null;
						index++;
					}					
				}
				//Up row
				point = center + new Vector2i(x, -half);
				if (IsPassable(point) && IsTileInGrid(point) && !IsOccupied(point))
					neighbors[index] = point;
				else
					neighbors[index] = null;
				index++;

				//Down row
				point = center + new Vector2i(x, half);
				if (IsPassable(point) && IsTileInGrid(point) && !IsOccupied(point))
					neighbors[index] = point;
				else
					neighbors[index] = null;
				index++;
			}
			return neighbors;
		}

		public bool GetRandomPassablePositionOnEdge(out Vector2i v)
		{
			v = Vector2i.Zero;
			for (int i = 0; i < 1000; i++)
			{
				var dir = (EDirection)UnityEngine.Random.Range(0, 4);
				switch (dir)
				{
					case EDirection.North:
						v = new Vector2i(CivRandom.Range(0, Width), Length - 1);
						break;
					case EDirection.East:
						v = new Vector2i(Width - 1, CivRandom.Range(0, Length));
						break;
					case EDirection.South:
						v = new Vector2i(CivRandom.Range(0, Width), 0);
						break;
					case EDirection.West:
						v = new Vector2i(0, CivRandom.Range(0, Length));
						break;
				}

				if (IsPassable(v)) return true;

			}
			return false;
		}

		public bool GetRandomPassablePositionsOnEdge(int positionCount, out Vector2i[] v)
		{
			v = new Vector2i[positionCount];
			int half = (int)Mathf.Ceil(positionCount / 2f);
			int center = 0;
			for (int i = 0; i < 1000; i++)
			{
				var dir = (EDirection)UnityEngine.Random.Range(0, 4);
				switch (dir)
				{
					case EDirection.North:
						center = CivRandom.Range(half, Width - half);
						for (int j = 0; j < positionCount; j++)
						{
							v[j] = new Vector2i(center + j - half, Length - 1);
						}
						break;
					case EDirection.East:
						center = CivRandom.Range(half, Length - half);
						for (int j = 0; j < positionCount; j++)
						{
							v[j] = new Vector2i(Width - 1, center + j - half);
						}
						break;
					case EDirection.South:
						center = CivRandom.Range(half, Width - half);
						for (int j = 0; j < positionCount; j++)
						{
							v[j] = new Vector2i(center + j - half, 0);
						}
						break;
					case EDirection.West:
						center = CivRandom.Range(half, Length - half);
						for (int j = 0; j < positionCount; j++)
						{
							v[j] = new Vector2i(0, center + j - half);
						}
						break;
				}
				bool isPassable = true;
				for (int j = 0; j < positionCount; j++)
				{
					if (!IsPassable(v[j]))
					{
						isPassable = false;
						break;
					}
				}
				if (isPassable)
					return true;

			}
			return false;
		}

		public Vector2i[] FindClosestEmptyTiles(Vector2i startPos, float maxDistance, int tileNumber)
		{
			var tiles = new List<Vector2i>();
			Vector2i tile = new Vector2i(startPos.X, startPos.Y);

			//Spiral check algorithm starting at startPos
			float radius = 0;
			++tile.X;
			radius += 2;

			while (Vector2i.Distance(startPos, tile) <= maxDistance && tiles.Count < tileNumber)
			{
				for (int c = tile.Y; tile.Y < c + radius; tile.Y++)
				{
					if (!IsOccupied(tile, true))
					{
						tiles.Add(new Vector2i(tile.X, tile.Y));
						if (tiles.Count >= tileNumber)
						{
							return tiles.ToArray();
						}
					}

				}
				--tile.Y;
				--tile.X;
				for (int c = tile.X; tile.X > c - radius + 1; tile.X--)
				{
					if (!IsOccupied(tile, true))
					{
						tiles.Add(new Vector2i(tile.X, tile.Y));
						if (tiles.Count >= tileNumber)
						{
							return tiles.ToArray();
						}
					}

				}
				if (!IsOccupied(tile))
					tiles.Add(new Vector2i(tile.X, tile.Y));
				--tile.Y;
				for (int c = tile.Y; tile.Y > c - radius; tile.Y--)
				{
					if (!IsOccupied(tile, true))
					{
						tiles.Add(new Vector2i(tile.X, tile.Y));
						if (tiles.Count >= tileNumber)
						{
							return tiles.ToArray();
						}
					}

				}
				++tile.X;
				++tile.Y;
				for (int c = tile.X; tile.X < c + radius; tile.X++)
				{
					if (!IsOccupied(tile, true))
					{
						tiles.Add(new Vector2i(tile.X, tile.Y));
						if (tiles.Count >= tileNumber)
						{
							return tiles.ToArray();
						}
					}

				}
				radius += 2;
			}
			return tiles.ToArray();
		}

		public bool IsTileInGrid(Vector2i tile)
		{
			if (tile.X >= 0 && tile.X < Width && tile.Y >= 0 && tile.Y < Length)
				return true;
			return false;
		}


		void LateUpdate()
		{
			if (Data == null)
				return;
			if (!partitionSystem.IsRunning)
				return;

			//Grid Data
			if (Data.passabilityChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.passability.ToArray(), Data.passabilitySummary, Data.passabilityChangedIndices);
			}

			//Map Layers
			if (Data.beautyChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.beauty.ToArray(), Data.beautySummary, Data.beautyChangedIndices);
			}
			if (Data.pollutionChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.pollution.ToArray(), Data.pollutionSummary, Data.pollutionChangedIndices);
			}
			if (Data.fertilityChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.fertility.ToArray(), Data.fertilitySummary, Data.fertilityChangedIndices);
			}
			if (Data.cityDensityChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.cityDensity.ToArray(), Data.cityDensitySummary, Data.cityDensityChangedIndices);
			}

			//Terrain surface
			if (Data.wasteChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.waste.ToArray(), Data.wasteSummary, Data.wasteChangedIndices);
			}

			if (Data.fertileLandChangedIndices.Count > 0)
			{
				UpdateMipMaps(Data.fertileLand.ToArray(), Data.fertileLandSummary, Data.fertileLandChangedIndices);
			}

			if (Data.dirtyMapData)
			{
				OnGridUpdate?.Invoke();
				Data.dirtyMapData = false;
			}
		}

		//Gets changed indices and constructs kernels to apply mipmapping to specified data structure
		void UpdateMipMaps(float[,] source, MapData.PartitionSummary[,] destination, Stack<Vector2i> indices)
		{
			//Debug.Log("UpdateMipMaps");
			HashSet<Vector2i> partitionIndices = new HashSet<Vector2i>();
			for (int i = 0; i < indices.Count; i++)
			{
				partitionIndices.Add(partitionSystem.SamplePartitionID(indices.Pop()));
			}
			indices.Clear();
			if (partitionIndices.Count <= 0)
				return;
			int size = partitionSystem.PartitionSize;
			foreach (var index in partitionIndices)
			{
				var kernel = new RectInt(index.X * size, index.Y * size, size, size);
				UpdateMipMaps(source, destination, kernel, index);
			}
		}

		//Applies kernel to perform convolution on specified partition index
		void UpdateMipMaps(float[,] source, MapData.PartitionSummary[,] destination, RectInt kernel, Vector2i partitionIndex)
		{
			float min = float.MaxValue;
			float max = -float.MaxValue;
			for (int i = 0; i < kernel.width; i++)
			{
				for (int j = 0; j < kernel.height; j++)
				{
					int x = kernel.x + i;
					int y = kernel.y + j;
					if (source[x, y] < min)
						min = source[x, y];
					if (source[x, y] > max)
						max = source[x, y];
				}
			}
			destination[partitionIndex.X, partitionIndex.Y].NodeMin = min;
			destination[partitionIndex.X, partitionIndex.Y].NodeMax = max;
			destination[partitionIndex.X, partitionIndex.Y].NodeAverage = (min + max) / 2f;
		}

		//Calculates Mipmaps on the entire map
		public void GenerateMipmaps()
		{
			int size = partitionSystem.PartitionSize;
			for (int i = 0; i <= partitionSystem.MaxPartitionX; i++)
			{
				for (int j = 0; j <= partitionSystem.MaxPartitionY; j++)
				{
					var kernel = new RectInt(i * size, j * size, size, size);
					UpdateMipMaps(Data.beauty.ToArray(), Data.beautySummary, kernel, new Vector2i(i, j));
					UpdateMipMaps(Data.passability.ToArray(), Data.passabilitySummary, kernel, new Vector2i(i, j));
					UpdateMipMaps(Data.pollution.ToArray(), Data.pollutionSummary, kernel, new Vector2i(i, j));
					UpdateMipMaps(Data.fertility.ToArray(), Data.fertilitySummary, kernel, new Vector2i(i, j));
					UpdateMipMaps(Data.waste.ToArray(), Data.wasteSummary, kernel, new Vector2i(i, j));
					UpdateMipMaps(Data.fertileLand.ToArray(), Data.fertileLandSummary, kernel, new Vector2i(i, j));
				}
			}
		}

		public ISaveable CollectData()
		{
			var data = new GridMapSaveData();
			data.mapData = (MapDataSaveData)Data.CollectData();
			data.terrainExchangeData = TerrainExchangeData;
			return data;
		}
	}
}