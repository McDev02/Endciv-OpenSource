using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// Simple Vector2 class
	/// </summary>
	[System.Serializable]
	public struct Velocity2 : System.IEquatable<Velocity2>
	{
		public float X;
		public float Y;


		public Velocity2(Velocity2 other)
			: this(other.X, other.Y)
		{
		}

		public Velocity2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public bool Equals(Velocity2 other)
		{
			return X == other.X && Y == other.Y;
		}

		public static explicit operator Vector2(Velocity2 v)
		{
			return new Vector2(v.X, v.Y);
		}
		public static explicit operator Vector3(Velocity2 v)
		{
			return new Vector3(v.X, 0, v.Y);
		}
	}
}