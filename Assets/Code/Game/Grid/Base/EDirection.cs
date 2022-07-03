using System;
using UnityEngine;
namespace Endciv
{
	public enum EDirection
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3,

		NE = 4,
		SE = 5,
		SW = 6,
		NW = 7,

		MAX = 8,
	}

    [Flags]
	public enum ESide
	{
		None = 0,

		// dont change the order
		North = 1 << 0,
		East = 1 << 1,
		South = 1 << 2,
		West = 1 << 3,

		All = North | East | South | West,
	}

	static class DirectionHelper
    {
        public readonly static Vector2i[] DirectionVectors = new Vector2i[]{
             Vector2i.Up,			//North	0
			new Vector2i(1,1),		//NE	1
			Vector2i.Right,			//East	2
			new Vector2i(1,-1),		//SE	3
	 		Vector2i.Down, 			//South	4
			new Vector2i(-1,-1),	//SW	5
			Vector2i.Left,			//West	6
			new Vector2i(-1,1),		//NW	7	
		};

        public static Vector3[] Directions = new Vector3[]
		   {
			new Vector3(0,0,1),
			new Vector3(1,0,0),
			new Vector3(0,0,-1),
			new Vector3(-1,0,0)
		  };
		public static Vector3[] DirectionsEuler = new Vector3[]
		   {
			new Vector3(0,0,0),
			new Vector3(0,90,0),
			new Vector3(0,180,0),
			new Vector3(0,270,0)
		  };

		public static EDirection RelativeTo(this EDirection parent, EDirection child)
		{
			UnityEngine.Debug.LogWarning("THis method has been changed, check for validity");
			int diff = (int)parent + (int)child;
			//if (diff >= 4)
			//	diff -= 4;
			return (EDirection)(diff % 4);
		}

		public static EDirection RotateClockwise(EDirection direction, int steps = 1)
		{
			int dir = (int)direction;
			dir += steps;
			return (EDirection)(dir % 4);
		}
		public static EDirection RotateCounterClockwise(EDirection direction, int steps = 1)
		{
			int dir = (int)direction;
			dir -= steps;
			return (EDirection)((Math.Ceiling(steps / 4f) * 4 + dir) % 4);
		}

		internal static Quaternion GetRotation(EDirection currentDirection)
		{
			return Quaternion.Euler(DirectionsEuler[(int)currentDirection]);
		}
	}

	public enum EMapSide
	{
		East,
		West,
		North,
		South
	}
}