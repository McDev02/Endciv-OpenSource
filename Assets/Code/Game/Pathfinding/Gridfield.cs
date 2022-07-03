using System.Collections.Generic;
using System;
using UnityEngine;
namespace Endciv
{
	public class Gridfield
	{
		Vector2i[] Directions = new Vector2i[] {
		new Vector2i(1,0),
		new Vector2i(1, 1),
		new Vector2i(0, 1),
		new Vector2i(-1, 1),
		new Vector2i(- 1, 0),
		new Vector2i(- 1, -1),
		new Vector2i(0, -1),
		new Vector2i(1, -1)
	};

		const bool UseFloatingDirections = true;
		const int Not_Passable_Cost = 99;
		public int Width { get; private set; }
		public int Length { get; private set; }

		//Temporal data
		public float MaxCost { get; private set; }
		public float MaxIntegrationCost { get; private set; }
		public float[,] Cost { get; private set; }
		public float[,] IntegrationCost { get; private set; }
		public bool[,] Visited { get; private set; }

		List<Vector2i> foundOrigins;
		public bool[,] Origin { get; private set; }
		public int OriginReachedBias;
		public int OriginsReached;

#if USE_FLOWFIELDS
        public Velocity2[,] LastFlowMap { get; private set; }
        public Stack<Velocity2[,]> PooledFlowMaps { get; private set; }	//Only use if FlowMap is a class
#endif
		MapData MapData;
		RectGrid Grid;

		public Gridfield(MapData mapData, RectGrid grid)
		{
			Width = grid.Width;
			Length = grid.Length;

			MapData = mapData;
			Grid = grid;

#if USE_FLOWFIELDS
            LastFlowMap = new Velocity2[0, 0];
#endif

			Cost = new float[Width, Length];
			IntegrationCost = new float[Width, Length];
			Visited = new bool[Width, Length];
			Origin = new bool[Width, Length];
			foundOrigins = new List<Vector2i>();
		}

		internal void CalculatePath(PathfinderJob job)
		{
#if !USE_FLOWFIELDS
			job.PathfindingMode = EPathfindingMode.FirstOrigin;
#endif
			OriginsReached = job.PathfindingMode == EPathfindingMode.FirstOrigin ? 1 : job.Origin.Indecies.Length;

			OriginReachedBias = job.TargetReachedBias;

			//Setup
			ResetCacheMaps();
			job.path = new List<Vector2i>(32);
			for (int o = 0; o < job.Origin.Indecies.Length; o++)
			{
				var from = job.Origin.Indecies[o];
				for (int d = 0; d < job.Destination.Indecies.Length; d++)
				{
					if (from == job.Destination.Indecies[d])
					{
						//One origin matches one destination, early escape, also to prevent errors.
						job.path.Add(from);
						return;
					}
				}
			}
			try
			{
				DijkstraFill(job);
			}
			catch (Exception e)
			{
				throw e;
			}
			try
			{
				GeneratePath(job);
			}
			catch (Exception e)
			{
				throw e;
			}
			//Now?
		}

		private void GeneratePath(PathfinderJob job)
		{
			if (foundOrigins.Count <= 0)
				throw new Exception("No origins found!");

			if (job.Destination.Indecies.Length <= 0)
				throw new Exception("No destinations!");

			for (int o = 0; o < foundOrigins.Count; o++)
			{
				Vector2i pointer = foundOrigins[o];
				bool foundTarget = false;

				Vector2i lowestIndex = pointer;
				float lowestCost;
				int maxcount = 9999;
				while (!foundTarget)
				{
					if (maxcount-- < 0)
						throw new Exception("Max iterations reached. Pathes found: " + job.path.Count.ToString());
					lowestCost = float.MaxValue;
					for (int i = 0; i < (int)EDirection.MAX; i++)
					{
						var indx = pointer - Directions[i];
						if (!IsInRange(indx)) continue;

						var cost = IntegrationCost[indx.X, indx.Y];
						if (cost < lowestCost)
						{
							lowestCost = cost;
							lowestIndex = indx;
						}
					}
					pointer = lowestIndex;
					job.path.Add(pointer);

					for (int i = 0; i < job.Destination.Indecies.Length; i++)
					{
						if (job.Destination.Indecies[i] == lowestIndex)
							foundTarget = true;
					}
				}
			}
		}

		void DijkstraFill(PathfinderJob job)
		{
			foundOrigins.Clear();
			//Todo: Change to work with bitmap goal and origin
			Stack<Vector2i> OpenSet = new Stack<Vector2i>();
			Stack<Vector2i> OpenSet2 = new Stack<Vector2i>();

			MaxCost = 0;
			MaxIntegrationCost = 0;
			if (job.Destination.Type == Location.EDestinationType.Waypoint)
				throw new Exception("Destination is Waypoint");

			var goal = job.Destination;
			var origin = job.Origin;

			//Set goal points
			for (int i = 0; i < goal.Indecies.Length; i++)
			{
				var point = goal.Indecies[i];
				OpenSet.Push(new Vector2i(point.X, point.Y));
				Cost[point.X, point.Y] = 0;
				IntegrationCost[point.X, point.Y] = 0;
				Visited[point.X, point.Y] = true;
			}
			//Set origin points
			if (origin != null)
			{
				for (int i = 0; i < origin.Indecies.Length; i++)
				{
					var point = origin.Indecies[i];
					Origin[point.X, point.Y] = true;
				}
			}

			lock (MapData)
			{
				bool isfirst = true;
				//Why do we have two while loops?
				while (OpenSet.Count > 0)
				{
					while (OpenSet.Count > 0)
					{
						var nodeID = OpenSet.Pop();
						var node = Grid.GetNode(nodeID);

						//Start
						if (isfirst)
							DijkstraIteration(job, nodeID, 0, 0, OpenSet2);

						//for (int i = 0; i < node.Links.Count; i++)
						//{
						//	var other = node.Links[i].GetOther(node);
						//	DijkstraIteration(job, nodeID, Grid.NodeLookupID[other.ID], OpenSet2);
						//	//var power = node.Links[i].Weight;
						//}
						//Orthogonal
						DijkstraIteration(job, nodeID, 1, 0, OpenSet2);
						DijkstraIteration(job, nodeID, -1, 0, OpenSet2);
						DijkstraIteration(job, nodeID, 0, 1, OpenSet2);
						DijkstraIteration(job, nodeID, 0, -1, OpenSet2);
						//Diagonal		  		   
						DijkstraIteration(job, nodeID, 1, 1, OpenSet2);
						DijkstraIteration(job, nodeID, -1, 1, OpenSet2);
						DijkstraIteration(job, nodeID, 1, -1, OpenSet2);
						DijkstraIteration(job, nodeID, -1, -1, OpenSet2);

						isfirst = false;
					}
					var tmp = OpenSet; OpenSet = OpenSet2;
					OpenSet2 = tmp; OpenSet2.Clear();

					//Skip if we reached origin
					//Todo: Change so that we see if all or just one origin was reached
					if (OriginsReached <= 0)
					{
						OriginReachedBias--;
						if (OriginReachedBias <= 0) break;
					}
				}
				if (foundOrigins.Count <= 0)
					throw new Exception("No Origins found in Dijkstra: " + job.Origin.ToString() + "\n To:\n " + job.Destination.ToString());

				//Calculate max iteration cost
				// for (int x = 0; x < Width; x++)
				// {
				//     for (int y = 0; y < Length; y++)
				//     {
				//         if (MapData.Passability[x, y] > 0 && MaxIntegrationCost < IntegrationCost[x, y])
				//             MaxIntegrationCost = IntegrationCost[x, y];
				//     }
				// }
			}
		}

		void DijkstraIteration(PathfinderJob job, Vector2i node, int x, int y, Stack<Vector2i> set)
		{
			DijkstraIteration(job, node, new Vector2i(node.X + x, node.Y + y), set);
		}
		void DijkstraIteration(PathfinderJob job, Vector2i node, Vector2i nv, Stack<Vector2i> set)
		{
			if (IsInRange(nv))
			{
				float cost = CivMath.Lerp(Not_Passable_Cost, 1, MapData.passability[nv.X, nv.Y]);

				if (job.openAreaMinValue > 0)
					cost -= GameConfig.Instance.pathfindingOpenAreaCost * Mathf.Min(0, MapData.openArea[nv.X, nv.Y] - job.openAreaMinValue);

				//not same layer
				//if (MapData.layer[nv.X, nv.Y] != MapData.layer[node.X, node.Y])
				//	cost = Not_Passable_Cost; Removed! No longer vlaid data

				Cost[nv.X, nv.Y] = cost;
				if (cost > MaxCost) MaxCost = cost;
				var diff = nv - node;
				//Diagonal is longer
				if (diff.X != 0 && diff.Y != 0)
					cost *= CivMath.sqrt2;

				float iCost = IntegrationCost[node.X, node.Y] + cost;
				if (IntegrationCost[nv.X, nv.Y] > iCost)
				{
					IntegrationCost[nv.X, nv.Y] = iCost;
					Visited[nv.X, nv.Y] = true;
					set.Push(nv);

					if (Origin[nv.X, nv.Y])
					{
						if (!foundOrigins.Contains(nv))
						{
							foundOrigins.Add(nv);
							if (job.PathfindingMode != EPathfindingMode.SearchAll) OriginsReached--;
						}
					}
				}
				if (iCost > int.MaxValue - 2)
					throw new Exception("Cost is too big: " + iCost.ToString("0.000"));
			}
		}


#if USE_FLOWFIELDS
        internal void CalculateFlowmap(PathfinderJob job)
        {
            OriginsReached = job.PathfindingMode == EPathfindingMode.FirstOrigin ? 1 : job.Origin.Indecies.Length;
            OriginReachedBias = job.TargetReachedBias;

            //Setup
            ResetCacheMaps();
            //var flowMap = new Velocity2[Width, Length];

            DijkstraFill(job);
            try
            {
                CalculateVectors(job.flowMap);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                throw e;
            }

            LastFlowMap = job.flowMap;
        }

        internal Velocity2[,] GetNewFlowmap()
        {
            return new Velocity2[Width, Length];
        }
        
        void CalculateVectors(Velocity2[,] flowMap)
        {
            Vector2 dir = Vector2.zero;
            Vector2i nv;
            if (UseFloatingDirections)
            {
                for (int y = 0; y < Length; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        dir = Vector2.zero;
                        var basecost = IntegrationCost[x, y];
                        for (int i = 0; i < Directions.Length; i++)
                        {
                            nv = Directions[i];
                            Vector2i nid = new Vector2i(x + nv.X, y + nv.Y);
                            if (!IsInRange(nid))
                                continue;

                            Vector2 vec = new Vector2(nid.X - x, nid.Y - y).normalized;
                            float cost = IntegrationCost[nid.X, nid.Y];
                            if (cost == 0)
                            {
                                dir = vec.normalized;
                                break;
                            }
                            else
                            {
                                float c = basecost - cost;
                                if (c > 0) dir += vec * c;
                            }
                        }
                        dir.Normalize();
                        var vel = flowMap[x, y];
                        vel.X = dir.x;
                        vel.Y = dir.y;
                        flowMap[x, y] = vel;
                    }
                }
            }
            else
            {
                for (int y = 0; y < Length; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        float lowestCost = int.MaxValue;
                        Vector2 lowestVector = Vector2.zero;
                        for (int i = 0; i < Directions.Length; i++)
                        {
                            nv = Directions[i];
                            Vector2i nid = new Vector2i(x + nv.X, y + nv.Y);
                            if (!IsInRange(nid))
                                continue;

                            Vector2 vec = new Vector2(nid.X - x, nid.Y - y).normalized;
                            float cost = IntegrationCost[nid.X, nid.Y];
                            if (cost == 0)
                            {
                                lowestCost = cost;
                                lowestVector = vec;
                                break;
                            }
                            else if (lowestCost > cost)
                            {
                                lowestCost = cost;
                                lowestVector = vec;
                            }
                        }
                        dir = lowestVector.normalized;
                        var vel = flowMap[x, y];
                        vel.X = dir.x;
                        vel.Y = dir.y;
                        flowMap[x, y] = vel;
                    }
                }
            }
        }

        
#endif

		bool IsInRange(Vector2i pos)
		{
			return !(pos.X < 0 || pos.Y < 0 || pos.X >= Width || pos.Y >= Length);
		}

		private void ResetCacheMaps()
		{
			foundOrigins.Clear();
			const float val = int.MaxValue;
			for (int y = 0; y < Length; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					IntegrationCost[x, y] = val;
					Visited[x, y] = false;
					Origin[x, y] = false;
				}
			}
		}
	}
}