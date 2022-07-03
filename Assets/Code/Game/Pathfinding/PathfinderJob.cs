using System;
using System.Collections.Generic;

namespace Endciv
{
	public enum EPathfindingMode { SearchAll, AllOrigins, FirstOrigin }
	public class PathfinderJob
	{
		public EPathfindingMode PathfindingMode;
		public Location Destination;
		public Location Origin;
		public int TargetReachedBias;
		public float openAreaMinValue;

#if USE_FLOWFIELDS
		public Velocity2[,] flowMap;
#else
		public List<Vector2i> path;
#endif

		public bool IsReady;

		public PathfinderJob()
		{
			IsReady = false;
			path = new List<Vector2i>(32);
		}
#if USE_FLOWFIELDS
        public PathfinderJob(Velocity2[,] flowMap)
        {
            this.flowMap = flowMap;
        }
#endif

		public PathfinderJob(Location goal, Location origin, EPathfindingMode mode, int targetReachedBias, float openAreaMinValue = 0)
		{
			IsReady = false;
			Destination = goal;
			Origin = origin;
			PathfindingMode = mode;
			TargetReachedBias = targetReachedBias;
			this.openAreaMinValue = openAreaMinValue;
		}

		public int ElapsedMilliseconds { get; internal set; }

		internal void Reset(Location goal, Location origin, EPathfindingMode mode, int targetReachedBias, float openAreaMinValue = 0)
		{
			IsReady = false;
			Destination = goal;
			Origin = origin;
			PathfindingMode = mode;
			TargetReachedBias = (int)(openAreaMinValue * 6) + targetReachedBias;
			this.openAreaMinValue = openAreaMinValue;
		}

		internal void Reset(Location destination, Location origin, EPathfindingMode pathfindingMode, object pathfindingTargetReachedBias, float openAreaMinValue)
		{
			throw new NotImplementedException();
		}
	}
}