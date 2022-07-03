using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Endciv
{
	public static class CivRandom
	{
		private static object m_SyncLock = new object();
		private static int m_Seed;
		private static Xorshift m_Rnd = new Xorshift();

		public static Color GetColor(int id)
		{
			Color col = RandomColorPool.Instance.GetColor(id);

			return col;
		}

		public static int Seed
		{
			get
			{
				lock (m_SyncLock) return m_Seed;
			}
			set
			{
				lock (m_SyncLock)
				{
					m_Seed = value;
					m_Rnd.SeedX = (uint)value;
					m_Rnd.SeedY = Xorshift.DEFAULT_VECTOR_Y;
					m_Rnd.SeedZ = Xorshift.DEFAULT_VECTOR_Z;
					m_Rnd.SeedW = Xorshift.DEFAULT_VECTOR_W;
				}
			}
		}

		/// <summary>
		/// A value of 1 returns true, 0 returns false, inbetween values are accoridng to chance
		/// </summary>
		internal static bool Chance(float chance)
		{
			if (chance >= 1) return true;
			if (chance <= 0) return false;
			return Value < chance;
		}

		/// <summary>
		/// Returns a random value between 0.0 and 1.0
		/// </summary>
		public static float Value
		{
			get
			{
				lock (m_SyncLock) return m_Rnd.Next() * Xorshift.SCALE_INC_ONE_SINGLE;
			}
		}

		public static bool ValueBool
		{
			get
			{
				lock (m_SyncLock) return (m_Rnd.Next() & 1) == 1;
			}
		}


		public static Vector2 RandomVector2()
		{
			return new Vector2(Range(-1f, 1f), Range(-1f, 1f));
		}

		public static Vector3 RandomVector3()
		{
			return new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f));
		}

		public static Vector4 RandomVector4()
		{
			return new Vector4(Range(-1f, 1f), Range(-1f, 1), Range(-1f, 1f), Range(-1f, 1f));
		}

		public static Color RandomColor()
		{
			return new Color(Range(0f, 1f), Range(0f, 1f), Range(0f, 1f), 1);
		}



		public static Vector2 InsideUnitCircle
		{
			get
			{
				lock (m_SyncLock)
				{
					Vector2 vec;
					vec.x = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.y = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					return vec;
				}
			}
		}

		public static Vector3 InsideUnitSphere
		{
			get
			{
				lock (m_SyncLock)
				{
					Vector3 vec;
					vec.x = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.y = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.z = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					return vec;
				}
			}
		}

		public static Vector2 OnUnitCircle
		{
			get
			{
				Vector2 vec;
				lock (m_SyncLock)
				{
					vec.x = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.y = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
				}
				vec.Normalize();
				return vec;
			}
		}

		public static Vector3 OnUnitSphere
		{
			get
			{
				Vector3 vec;
				lock (m_SyncLock)
				{
					vec.x = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.y = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
					vec.z = m_Rnd.Next() * (Xorshift.SCALE_INC_ONE_SINGLE * 2f) - 1f;
				}
				vec.Normalize();
				return vec;
			}
		}

		//public static Quaternion Rotation { get; }
		//public static Quaternion RotationUniform { get; }

		public static float Range(MinMax v)
		{
			return Range(v.min, v.max);
		}
		public static float GetRandom(this MinMax v)
		{
			return Range(v.min, v.max);
		}

		// min - include, max - include
		public static float Range(float min, float max)
		{
			if (min > max) throw new ArgumentOutOfRangeException();
			if (min < float.MinValue / 2 && max > float.MaxValue / 2)
			{
				lock (m_SyncLock)
					return (float)(m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_DOUBLE * ((double)max - (double)min) + (double)min);
			}
			else
			{
				lock (m_SyncLock)
					return m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_SINGLE * (max - min) + min;
			}
		}

		internal static Color RandomColor(float alpha)
		{
			Color col;
			col.r = Range(0f, 1f);
			col.g = Range(0f, 1f);
			col.b = Range(0f, 1f);
			col.a = alpha;
			return col;
		}


		public static EDirection RandomDirection(bool includeDiagonals = false)
		{
			return (EDirection)CivRandom.Range(0, includeDiagonals ? 8 : 4);
		}

		/// <summary>
		/// Returns a random number between Min and Max. The result excludes Max.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Range(int min, int max)
		{
			if (min >= max) return min;
			if (min < int.MinValue / 2 && max > int.MaxValue / 2)
			{
				lock (m_SyncLock)
					return (int)(m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_DOUBLE * ((double)max - (double)min) + (double)min);
			}
			else
			{
				lock (m_SyncLock)
					return (int)(m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_SINGLE * (max - min) + min);
			}
		}

		// max - include
		public static float Range(float max)
		{
			if (max < 0) throw new ArgumentOutOfRangeException();
			lock (m_SyncLock)
				return m_Rnd.Next() * Xorshift.SCALE_INC_ONE_SINGLE * max;
		}

		// max - exclude
		public static int Range(int max)
		{
			if (max < 0) throw new ArgumentOutOfRangeException();
			lock (m_SyncLock)
				return (int)(m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_SINGLE * max);
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			lock (m_SyncLock)
				while (--n > 0)
				{
					int k = (int)(m_Rnd.Next() * Xorshift.SCALE_EXC_ONE_SINGLE * n);
					if (n == k) continue;
					T value = list[k];
					list[k] = list[n];
					list[n] = value;
				}
		}

		public static void Shuffle<T>(this T[] array)
		{
			Shuffle((IList<T>)array);
		}

		public static T SelectRandom<T>(this IList<T> array)
		{
			if (array == null || array.Count == 0) return default(T);
			if (array.Count == 1) return array[0];
			return array[Range(0, array.Count)];
		}

		public static T SelectRandom<T>(this T[] array)
		{
			return SelectRandom((IList<T>)array);
		}

		public static TValue SelectRandom<TKey, TValue>(this IDictionary<TKey, TValue> dict)
		{
			System.Random rand = new System.Random();
			List<TValue> values = Enumerable.ToList(dict.Values);
			return values[UnityEngine.Random.Range(0, dict.Count)];
		}

		static CivRandom()
		{
			m_Seed = Environment.TickCount;
			m_Rnd = new Xorshift((uint)m_Seed);
		}
	}

	/// <summary>
	/// Xorshift 128
	/// http://de.wikipedia.org/wiki/Xorshift
	/// </summary>
	public struct Xorshift
	{
		// scale for (include) 0.0 to (exclude) 1.0
		internal const double SCALE_EXC_ONE_DOUBLE = 1d / (double)(uint.MaxValue + 1UL);
		internal const float SCALE_EXC_ONE_SINGLE = (float)SCALE_EXC_ONE_DOUBLE;

		// scale for (include) 0.0 to (include) 1.0
		internal const double SCALE_INC_ONE_DOUBLE = 1d / (double)uint.MaxValue;
		internal const float SCALE_INC_ONE_SINGLE = (float)SCALE_INC_ONE_DOUBLE;

		public const uint DEFAULT_VECTOR_X = 123456789;
		public const uint DEFAULT_VECTOR_Y = 362436069;
		public const uint DEFAULT_VECTOR_Z = 521288629;
		public const uint DEFAULT_VECTOR_W = 88675123;

		private uint m_X, m_Y, m_Z, m_W;

		public uint SeedX
		{
			get { return m_X; }
			set { m_X = value; }
		}

		public uint SeedY
		{
			get { return m_Y; }
			set { m_Y = value; }
		}

		public uint SeedZ
		{
			get { return m_Z; }
			set { m_Z = value; }
		}

		public uint SeedW
		{
			get { return m_W; }
			set { m_W = value; }
		}

		public Xorshift(uint seedX)
			: this(seedX, DEFAULT_VECTOR_Y, DEFAULT_VECTOR_Z, DEFAULT_VECTOR_W)
		{ }

		public Xorshift(uint seedX, uint seedY, uint seedZ, uint seedW)
		{
			if (seedX == 0 || seedY == 0 || seedZ == 0 || seedW == 0)
			{
				throw new ArgumentException();
			}
			m_X = seedX;
			m_Y = seedY;
			m_Z = seedZ;
			m_W = seedW;
		}

		public uint Next()
		{
			uint t = (m_X ^ (m_X << 11));
			m_X = m_Y;
			m_Y = m_Z;
			m_Z = m_W;
			m_W = (m_W ^ (m_W >> 19)) ^ (t ^ (t >> 8));
			return m_W;
		}

		// min - include, max - include
		public float Range(float min, float max)
		{
			if (min > max) throw new ArgumentOutOfRangeException();
			if (min < float.MinValue / 2 && max > float.MaxValue / 2)
			{
				return (float)(Next() * SCALE_EXC_ONE_DOUBLE * ((double)max - (double)min) + (double)min);
			}
			else
			{
				return Next() * SCALE_EXC_ONE_SINGLE * (max - min) + min;
			}
		}

		// min - include, max - exclude
		public int Range(int min, int max)
		{
			if (min >= max) throw new ArgumentOutOfRangeException();
			if (min < int.MinValue / 2 && max > int.MaxValue / 2)
			{
				return (int)(Next() * SCALE_EXC_ONE_DOUBLE * ((double)max - (double)min) + (double)min);
			}
			else
			{
				return (int)(Next() * SCALE_EXC_ONE_SINGLE * (max - min) + min);
			}
		}

		// max - include
		public float Range(float max)
		{
			if (max < 0) throw new ArgumentOutOfRangeException();
			return Next() * SCALE_INC_ONE_SINGLE * max;
		}

		// max - exclude
		public int Range(int max)
		{
			if (max < 0) throw new ArgumentOutOfRangeException();
			return (int)(Next() * SCALE_EXC_ONE_SINGLE * max);
		}

	}
}