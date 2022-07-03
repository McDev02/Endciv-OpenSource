using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class GridObjectData
	{
		public Vector2i[] EntrancePoints = new Vector2i[0];
		[NonSerialized]
		public EDirection Direction = EDirection.North;
		[HideInInspector]
		public RectBounds Rect;
		public bool EdgeIsWall;
		public float density = 1;

		public void CopyFrom(GridObjectData other)
		{
			Direction = other.Direction;
			EntrancePoints = new Vector2i[other.EntrancePoints.Length];
			for (int i = 0; i < EntrancePoints.Length; i++)
			{
				EntrancePoints[i] = other.EntrancePoints[i];
			}
			Rect = new RectBounds(other.Rect.Minimum, other.Rect.Maximum);
			EdgeIsWall = other.EdgeIsWall;
			density = other.density;
		}

		public void Swap()
		{
			Rect.Swap(); int tmp;
			for (int i = 0; i < EntrancePoints.Length; i++)
			{
				var point = EntrancePoints[i];
				tmp = point.X;
				point.X = point.Y;
				point.Y = tmp;
				EntrancePoints[i] = point;
			}
		}
	}
}