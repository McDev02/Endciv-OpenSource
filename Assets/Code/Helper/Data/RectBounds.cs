using System;
using UnityEngine;
using UnityEngine.Serialization;
namespace Endciv
{
	public enum ERectBoundsEdge
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	[Serializable]
	public struct RectBounds : IEquatable<RectBounds>
	{
		[SerializeField]
		[FormerlySerializedAs("X")]
		int m_X;
		[SerializeField]
		[FormerlySerializedAs("Y")]
		int m_Y;

		[SerializeField]
		[FormerlySerializedAs("Width")]
		int m_Width;
		[SerializeField]
		[FormerlySerializedAs("Length")]
		int m_Length;

		public int Width
		{
			get { return m_Width; }
			set
			{
				m_Width = value;
				Maximum.X = m_X + m_Width - 1;
				HalfWidth = Width / 2f;
			}
		}
		public int Length
		{
			get { return m_Length; }
			set
			{
				m_Length = value;
				Maximum.Y = m_Y + m_Length - 1;
				HalfLength = Length / 2f;
			}
		}
		public float HalfWidth { get; private set; }
		public float HalfLength { get; private set; }

		public int X
		{
			get { return m_X; }
			set
			{
				m_X = value;
				Minimum.X = m_X;
				Maximum.X = m_X + Width - 1;
			}
		}
		public int Y
		{
			get { return m_Y; }
			set
			{
				m_Y = value;
				Minimum.Y = m_Y;
				Maximum.Y = m_Y + Length - 1;
			}
		}

		internal Vector2i[] ToArray()
		{
			var vecs = new Vector2i[Area];
			for (int y = 0; y < Length; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					vecs[x + y * Width] = new Vector2i(X + x, Y + y);
				}
			}
			return vecs;
		}

		public int Top
		{
			get { return Y + Length - 1; }
		}
		public int Right
		{
			get { return X + Width - 1; }
		}
		public int Bottom
		{
			get { return Y; }
		}
		public int Left
		{
			get { return X; }
		}

		[HideInInspector]
		public Vector2i Minimum;
		[HideInInspector]
		public Vector2i Maximum;

		public Vector2i TopLeft { get { return new Vector2i(Minimum.X, Maximum.Y); } }
		public Vector2i TopRight { get { return Maximum; } }

		internal Vector2i RandomPosition()
		{
			int x = CivRandom.Range(X, X + Width);
			int y = CivRandom.Range(Y, Y + Length);
			return new Vector2i(x, y);
		}

		public Vector2i BottomRight { get { return new Vector2i(Maximum.X, Minimum.Y); } }
		public Vector2i BottomLeft { get { return Minimum; } }

		public void Translate(Vector2i offset)
		{
			X += offset.X;
			Y += offset.Y;
		}

		public void Translate(int X, int Y)
		{
			Translate(new Vector2i(X, Y));
		}

		public Vector2i Size
		{
			get { return new Vector2i(Width, Length); }
			set { Width = value.X; Length = value.Y; }
		}

		internal void Set(int x, int y, int sizeX, int sizeY)
		{
			X = x;
			Y = y;
			Width = sizeX;
			Length = sizeY;
		}

		public int Area { get { return Width * Length; } }

		public Vector2 Center
		{
			get { return new Vector2(Minimum.X + Width * 0.5f, Minimum.Y + Length * 0.5f); }
		}
		public Vector2i Centeri
		{
			get { return new Vector2i((int)(Minimum.X + Width * 0.5f), (int)(Minimum.Y + Length * 0.5f)); }
		}

		public Vector2i[] Corners
		{
			get
			{
				return new Vector2i[] { TopLeft, TopRight, BottomRight, BottomLeft };
			}
		}

		public RectBounds(Vector2i tile)
		{
			m_X = tile.X;
			m_Y = tile.Y;
			m_Width = m_Length = 1;
			HalfWidth = m_Width / 2f;
			HalfLength = m_Length / 2f;

			Minimum.X = m_X;
			Maximum.X = m_X + m_Width - 1;
			Minimum.Y = m_Y;
			Maximum.Y = m_Y + m_Length - 1;

		}

		public RectBounds(int x, int y, int width, int length)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Length = length;
			HalfWidth = m_Width / 2f;
			HalfLength = m_Length / 2f;

			Minimum.X = m_X;
			Maximum.X = m_X + m_Width - 1;
			Minimum.Y = m_Y;
			Maximum.Y = m_Y + m_Length - 1;
		}

		public RectBounds(Vector2i a, Vector2i b)
		{
			m_X = Mathf.Min(a.X, b.X);
			m_Y = Mathf.Min(a.Y, b.Y);
			m_Width = Mathf.Max(a.X, b.X) - m_X + 1;
			m_Length = Mathf.Max(a.Y, b.Y) - m_Y + 1;
			HalfWidth = m_Width / 2f;
			HalfLength = m_Length / 2f;

			Minimum.X = m_X;
			Maximum.X = m_X + m_Width - 1;
			Minimum.Y = m_Y;
			Maximum.Y = m_Y + m_Length - 1;
		}

		public Vector2i GetEdge(ERectBoundsEdge edge)
		{
			switch (edge)
			{
				// top left
				case ERectBoundsEdge.TopLeft:
					return new Vector2i(Minimum.X, Maximum.Y);
				// top right
				case ERectBoundsEdge.TopRight:
					return Maximum;
				// bottom left
				case ERectBoundsEdge.BottomLeft:
					return Minimum;
				// bottom right
				case ERectBoundsEdge.BottomRight:
					return new Vector2i(Maximum.X, Minimum.Y);

				default: throw new ArgumentOutOfRangeException();
			}
		}

		public void Insert(Vector2i point)
		{
			X = Mathf.Min(Minimum.X, point.X);
			Y = Mathf.Min(Minimum.Y, point.Y);
			if (point.X - X >= Width) Width = point.X - X + 1;
			if (point.Y - Y >= Length) Length = point.Y - Y + 1;
		}

		internal string ToSizeString()
		{
			return "Rect: Width: " + Width + " Length: " + Length;
		}

		/// <summary>
		/// Clamps the Rect on X and Y to a Quader from 0 to size
		/// </summary>
		/// <param name="size"></param>
		public void Clamp(int width, int length)
		{
			Vector2i max = Maximum;
			X = Mathf.Clamp(Minimum.X, 0, width - 1);
			Y = Mathf.Clamp(Minimum.Y, 0, length - 1);
			max.X = Mathf.Clamp(max.X, 0, width - 1);
			max.Y = Mathf.Clamp(max.Y, 0, length - 1);
			Width = max.X - X + 1;
			Length = max.Y - Y + 1;
		}
		/// <summary>
		/// Clamps the Rect on X and Y to a Quader from 0 to size
		/// </summary>
		/// <param name="size"></param>
		public void Clamp(int size)
		{
			Clamp(size, size);
		}

		public void Extend(int size)
		{
			X -= size;
			Y -= size;
			Width += 2 * size;
			Length += 2 * size;
		}

		public void Extend(int top, int right, int bottom, int left)
		{
			X -= left;
			Y -= bottom;
			Width += right + left;
			Length += top + bottom;
		}
		/// <summary>
		/// Returns a copy of the rect extended by specified amount
		/// </summary>
		public RectBounds Extended(int v)
		{
			return new RectBounds(Minimum.X - v, Minimum.Y - v, Width + 2 * v, Length + 2 * v);
		}
		public void Merge(Vector2i min, Vector2i max)
		{
			Vector2i max1 = Maximum;
			X = Mathf.Min(X, min.X);
			Y = Mathf.Min(Y, min.Y);
			max1.X = Mathf.Max(max1.X, max.X);
			max1.Y = Mathf.Max(max1.Y, max.Y);
			Width = max1.X - X + 1;
			Length = max1.Y - Y + 1;
		}

		public void Merge(Vector2i point)
		{
			Merge(point, point);
		}
		public void Merge(RectBounds rect)
		{
			Merge(rect.Minimum, rect.Maximum);
		}

		public RectBounds RotateWorldSpace(EDirection direction)
		{
			return Rotate(direction, Vector2i.Zero);
		}

		public RectBounds RotateOnRoot(EDirection direction)
		{
			return Rotate(direction, Minimum);
		}

		public void Swap()
		{
			var size = Size;
			var tmp = size.X;
			size.X = size.Y;
			size.Y = tmp;
			Size = size;
		}

		public RectBounds Rotate(EDirection direction, Vector2i root)
		{
			Vector2i Min = Minimum - root;
			Vector2i Max = Maximum - root;

			Vector2i MinN = Minimum - root;
			Vector2i MaxN = Maximum - root;

			if (direction == EDirection.East)
			{
				MinN = new Vector2i(Min.Y, -Max.X); MaxN = new Vector2i(Max.Y, -Min.X);
			}
			if (direction == EDirection.South)
			{
				MinN = new Vector2i(-Max.X, -Max.Y); MaxN = new Vector2i(-Min.X, -Min.Y);
			}
			if (direction == EDirection.West)
			{
				MinN = new Vector2i(-Max.Y, Min.X); MaxN = new Vector2i(-Min.Y, Max.X);
			}

			return new RectBounds(MinN + root, MaxN + root);
		}

		public bool Intersects(RectBounds other)
		{
			return Intersects(this, other);
		}

		public bool Overlaps(RectBounds other)
		{
			return Overlaps(this, other);
		}

		public bool Contains(RectBounds other)
		{
			return Contains(this, other);
		}

		public bool Contains(Vector2i point)
		{
			return ContainsVertex(this, point);
		}
		public bool Contains(int x, int y)
		{
			return ContainsVertex(this, x, y);
		}

		public bool ContainsVertex(Vector2i point)
		{
			return ContainsVertex(this, point);
		}


		public bool Equals(RectBounds other)
		{
			return X == other.X
				&& Y == other.Y
				&& Width == other.Width
				&& Length == other.Length;
		}

		public override bool Equals(object obj)
		{
			if (obj is RectBounds)
			{
				return Equals((RectBounds)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Minimum.GetHashCode() + Maximum.GetHashCode();
		}

		public override string ToString()
		{
			return "(Minimum: " + Minimum + ", Maximum: " + Maximum + ")";
		}

		public static implicit operator Rect(RectBounds bounds)
		{
			Rect rect = new Rect();
			rect.min = bounds.Minimum;
			rect.max = bounds.Maximum;
			return rect;
		}

		public static explicit operator RectBounds(Rect rect)
		{
			Vector2i min = (Vector2i)rect.min;
			Vector2i max = (Vector2i)rect.max;
			return new RectBounds(min, max);
		}

		public static bool operator ==(RectBounds left, RectBounds right)
		{
			return left.X == right.X
				&& left.Y == right.Y
				&& left.Width == right.Width
				&& left.Length == right.Length;
		}

		public static bool operator !=(RectBounds left, RectBounds right)
		{
			return left.X != right.X
				|| left.Y != right.Y
				|| left.Width != right.Width
				|| left.Length != right.Length;
		}

		// create a rect based on two indices
		public static RectBounds CreateRectBounds(Vector2i point0, Vector2i point1)
		{
			return new RectBounds(Vector2i.Min(point0, point1), Vector2i.Max(point0, point1));
		}

		public static RectBounds MaskOut(RectBounds source, RectBounds mask)
		{
			return new RectBounds(Vector2i.Max(source.Minimum, mask.Minimum), Vector2i.Min(source.Maximum, mask.Maximum));
		}

		public static void Extends(RectBounds rect, ESide side)
		{
			int top = ((int)side >> 0) & 1; // north
			int right = ((int)side >> 1) & 1; // east
			int bottom = ((int)side >> 2) & 1; // south
			int left = ((int)side >> 3) & 1; // west
			rect.Extend(top, right, bottom, left);
		}
		/// <summary>
		/// Returns true if at least one cell is shared by each rects
		/// </summary>
		public static bool Intersects(RectBounds rect0, RectBounds rect1)
		{
			return ContainsVertex(rect0, rect1.Minimum) ||
				ContainsVertex(rect0, rect1.Maximum) ||
				ContainsVertex(rect0, rect1.TopLeft) ||
				ContainsVertex(rect0, rect1.BottomRight)
				||
				ContainsVertex(rect1, rect0.Minimum) ||
				ContainsVertex(rect1, rect0.Maximum) ||
				ContainsVertex(rect1, rect0.TopLeft) ||
				ContainsVertex(rect1, rect0.BottomRight);
		}

		/// <summary>
		/// Returns true if rect1 is completely inside rect0, but also smaller than one unit at each border
		/// </summary>
		public static bool Overlaps(RectBounds rect0, RectBounds rect1)
		{
			return rect0.Maximum.X > rect1.Minimum.X
				&& rect0.Minimum.X < rect1.Maximum.X
				&& rect0.Maximum.Y > rect1.Minimum.Y
				&& rect0.Minimum.Y < rect1.Maximum.Y;
		}

		/// <summary>
		/// Returns true if rect1 is completely inside rect0
		/// </summary>
		public static bool Contains(RectBounds rect0, RectBounds rect1)
		{
			return rect0.Minimum.X <= rect1.Minimum.X
				&& rect0.Minimum.Y <= rect1.Minimum.Y
				&& rect0.Maximum.X >= rect1.Maximum.X
				&& rect0.Maximum.Y >= rect1.Maximum.Y;
		}

		public static bool ContainsPoint(RectBounds rect, Vector2i point)
		{
			//if (rect.Maximum.Y == point.Y || rect.Maximum.X == point.X)
			//	Debug.LogError("This equation has been removed below, please check if this method is still correct!");

			return rect.Minimum.X <= point.X
				&& rect.Minimum.Y <= point.Y
				&& rect.Maximum.X > point.X
				&& rect.Maximum.Y > point.Y;
		}
		public static bool ContainsVertex(RectBounds rect, Vector2i point)
		{
			return rect.Minimum.X <= point.X
				&& rect.Minimum.Y <= point.Y
				&& rect.Maximum.X >= point.X
				&& rect.Maximum.Y >= point.Y;
		}
		public static bool ContainsVertex(RectBounds rect, int x, int y)
		{
			return rect.Minimum.X <= x
				&& rect.Minimum.Y <= y
				&& rect.Maximum.X >= x
				&& rect.Maximum.Y >= y;
		}
		/// <summary>
		/// Return the width or edge depending on the index
		/// It's used to make easy link to Vector2i where v2i[0]=x and v2i[1]=y;
		/// </summary>
		/// <param name="index">array index</param>
		/// <returns></returns>
		public int this[int index]
		{
			get
			{
				if (index == 0)
					return Width;
				else
					return Length;
			}

			set
			{
				if (index == 0)
					Width = value;
				else
					Length = value;
			}

		}

		internal Rect ToRect()
		{
			return new Rect(X, Y, Width, Length);
		}
	}
}