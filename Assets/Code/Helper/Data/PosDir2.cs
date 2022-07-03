using UnityEngine;
namespace Endciv
{
	[System.Serializable]
	public struct PosDir2
	{
		public Vector2 Position;
		public Vector2 Direction;
	}

	[System.Serializable]
	public struct PosDirGrid
	{
		public int X { get { return Position.X; } }
		public int Y { get { return Position.Y; } }
		public Vector2i Position;
		public EDirection Direction;
	}
}