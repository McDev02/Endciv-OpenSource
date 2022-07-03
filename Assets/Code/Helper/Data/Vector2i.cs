using UnityEngine;
using System;

namespace Endciv
{
	/// <summary>
	/// Vector 2 with integer values
	/// </summary>
	[Serializable]
	public struct Vector2i : IEquatable<Vector2i>
	{
		public int X;
		public int Y;

		public static readonly Vector2i Zero = new Vector2i(0, 0);
		public static readonly Vector2i One = new Vector2i(1, 1);
		public static readonly Vector2i Left = new Vector2i(-1, 0);
		public static readonly Vector2i Right = new Vector2i(1, 0);
		public static readonly Vector2i Up = new Vector2i(0, 1);
		public static readonly Vector2i Down = new Vector2i(0, -1);
		public static readonly Vector2i Negative = new Vector2i(-1, -1);

		public Vector2i(Vector2i other)
			: this(other.X, other.Y)
		{
		}

		public Vector2i(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Normalized integer Vector. Should only be used for single axis vectors (0,n) or (n,0)
		/// </summary>
		public Vector2i Normalized
		{
			get
			{
				float mag = Magnitude;
				return new Vector2i((int)((float)X / mag), (int)((float)Y / mag));
			}
		}

		public void Normalize()
		{
			float mag = Magnitude;
			float tx = X / mag;
			float ty = Y / mag;
			X = (int)tx;
			Y = (int)ty;
		}

        public float Magnitude
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

        public int MagnitudeI
		{
			get { return (int)Math.Sqrt(X * X + Y * Y); }
		}

        public int MagnitudeSqr
		{
			get { return X * X + Y * Y; }
		}

		public bool Equals(Vector2i other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector2i)
			{
				return Equals((Vector2i)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return X + Y;
		}

		public override string ToString()
		{
			return "(" + X + ", " + Y + ")";
		}

		public static implicit operator Vector2(Vector2i other)
		{
			return new Vector2(other.X, other.Y);
		}

		public static explicit operator Vector2i(Vector2 other)
		{
			return new Vector2i((int)other.x, (int)other.y);
		}

		public static Vector2i operator *(Vector2i left, int right)
		{
			left.X *= right;
			left.Y *= right;
			return left;
		}

		public static Vector2i operator +(Vector2i left, Vector2i right)
		{
			left.X += right.X;
			left.Y += right.Y;
			return left;
		}

		public static Vector2i operator -(Vector2i left, Vector2i right)
		{
			left.X -= right.X;
			left.Y -= right.Y;
			return left;
		}

		public static bool operator ==(Vector2i left, Vector2i right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static bool operator !=(Vector2i left, Vector2i right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		public static float Distance(ref Vector2i point0, ref Vector2i point1)
		{
			int x = point0.X - point1.X;
			int y = point0.Y - point1.Y;
			return Mathf.Sqrt(x * x + y * y);
		}

		public static float Distance(Vector2i point0, Vector2i point1)
		{            
			return Distance(ref point0, ref point1);
		}

		public static float DistanceSqr(ref Vector2i point0, ref Vector2i point1)
		{
			int x = point0.X - point1.X;
			int y = point0.Y - point1.Y;
			return x * x + y * y;
		}

		public static float DistanceSqr(Vector2i point0, Vector2i point1)
		{
			return DistanceSqr(ref point0, ref point1);
		}

		public static Vector2i Max(Vector2i point0, Vector2i point1)
		{
			Vector2i v2;
			v2.X = point0.X > point1.X ? point0.X : point1.X;
			v2.Y = point0.Y > point1.Y ? point0.Y : point1.Y;
			return v2;
		}

		public static Vector2i Min(Vector2i point0, Vector2i point1)
		{
			Vector2i v2;
			v2.X = point0.X < point1.X ? point0.X : point1.X;
			v2.Y = point0.Y < point1.Y ? point0.Y : point1.Y;
			return v2;
		}

        public Vector2i Swap
		{
			get
			{
				return new Vector2i(Y, X);
			}
		}

        public int this[int index]
		{
			get
			{
				if (index == 0)
					return X;
				else if (index == 1)
					return Y;
				else
					throw new InvalidOperationException("Invalid index parameter");

			}
			set
			{
				if (index == 0)
					X = value;
				else if (index == 1)
					Y = value;
				else
					throw new InvalidOperationException("Invalid index parameter");

			}
		}
	}
}