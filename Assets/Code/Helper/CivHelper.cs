using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Endciv
{
	public delegate void ListenerCallInteger(int value);
	public delegate void ListenerCallFloat(float value);
	public delegate void ListenerCallObject(object caller);
	public static class CivHelper
	{
		#region String Converter

		public static void ToStringDecimal( this float value, int decimals, StringBuilder builder)
		{
			var digit = (int)value;
			decimals = (int)((value - digit) * Mathf.Pow(10, decimals));

			builder.Append(digit);
			builder.Append(".");
			builder.Append(decimals);
		}

		public static string ToStringDecimal(this float value, int decimals)
		{
			var digit = (int)value;
			decimals = (int)((value - digit) * Mathf.Pow(10, decimals));

			return $"{digit}.{decimals}";
		}

		public static string FirstLetterUppercase(this string input)
		{
			if (input.Length <= 0) return input;
			return $"{input[0].ToString().ToUpper()}{input.Substring(1)}";
		}

		public static string GetTypeName(System.Type type)
		{
			var namespaces = type.ToString().Split('.');
			return namespaces[namespaces.Length - 1];
		}

		internal static string GetTimeString(float timeInMinutes)
		{
			string txt = "";
			int hours = (int)(timeInMinutes * (1f / 60));
			int minutes = (int)(timeInMinutes - hours * 60);

			if (hours > 0)
			{
				txt += hours.ToString() + "h";
				if (minutes > 0)
					txt += " " + minutes.ToString("00") + "m";
			}
			else
			{
				if (minutes > 0)
					txt += minutes.ToString("00") + "min";
			}
			return txt;
		}
		internal static string GetTimeStringDoubledot(float timeInMinutes)
		{
			string txt = "";
			int hours = (int)(timeInMinutes * (1f / 60));
			int minutes = (int)(timeInMinutes - hours * 60);

			return $"{hours}:{minutes.ToString("00")}";
		}

		internal static string GetTimeStringRounded(float timeInMinutes, int roundToMinutes)
		{
			string txt = "";
			int hours = (int)(timeInMinutes * (1f / 60));
			int minutes = (int)(timeInMinutes - hours * 60);

			if (hours > 0)
			{
				txt += hours.ToString() + "h";
				if (minutes > 0)
					txt += " " + minutes.ToString("00") + "m";
			}
			else
			{
				if (minutes > 0)
					txt += minutes.ToString("00") + "min";
			}
			return txt;
		}

		internal static string GetWaterString(int amount)
		{
			float water = amount * GameConfig.WaterPortion;
			//if (water < 1f) return $"{(int)(water * 100)}ml";
			if (water < 10f) return $"{water.ToString("0.##") }l";
			else return $"{water.ToString("0") }l";
		}

		#endregion String Converter

		#region Collections
		public static List<T> CollectAll<T>(this List<List<T>> list)
		{
			List<T> all = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				all.AddRange(list[i]);
			}
			return all;
		}
		#endregion

		public static ESide RotateSide(this ESide side, EDirection rotation)
		{
			var rot = (int)side << (int)rotation;
			var outRot = rot & (int)ESide.All;

			return (ESide)((rot >> 4) | outRot) & ESide.All;
		}

		public static T[,] RotateDataMap<T>(this T[,] data, EDirection rotation)
		{
			if (data == null)
			{
				return default(T[,]);
			}
			int LengtX = data.GetLength(0);
			int LengtY = data.GetLength(1);
			T[,] returnData = data;

			switch (rotation)
			{
				case EDirection.North:
					returnData = data;
					break;
				case EDirection.East:
					returnData = new T[LengtY, LengtX];
					for (int y = 0; y < LengtY; y++)
					{
						for (int x = 0; x < LengtX; x++)
						{
							returnData[y, (LengtX - 1) - x] = data[x, y];
						}
					}
					break;
				case EDirection.South:
					returnData = new T[LengtX, LengtY];
					for (int y = 0; y < LengtY; y++)
					{
						for (int x = 0; x < LengtX; x++)
						{
							returnData[(LengtX - 1) - x, (LengtY - 1) - y] = data[x, y];
						}
					}
					break;
				case EDirection.West:
					returnData = new T[LengtY, LengtX];
					for (int y = 0; y < LengtY; y++)
					{
						for (int x = 0; x < LengtX; x++)
						{
							returnData[(LengtY - 1) - y, x] = data[x, y];
						}
					}
					break;
				default:
					break;
			}

			return returnData;
		}

		/*
 * Reimplement this
public static GridPrefabData RotateGridPrefabMap(ref GridPrefabData data, EDirection rotation)
{
	int LengtX = data.Width;
	int LengtY = data.Length;

	GridPrefabData newdata;

	switch (rotation)
	{
		case EDirection.North:
			newdata = new GridPrefabData(LengtX, LengtY, Vector3.zero);
			for (int y = 0; y < LengtY; y++)
			{
				for (int x = 0; x < LengtX; x++)
				{
					GridPrefabData.SwapGridData(ref data, x, y, ref newdata, x, y);
				}
			}
			break;
		case EDirection.East:
			newdata = new GridPrefabData(LengtY, LengtX, Vector3.zero);
			for (int y = 0; y < LengtY; y++)
			{
				for (int x = 0; x < LengtX; x++)
				{
					GridPrefabData.SwapGridData(ref data, x, y, ref newdata, y, (LengtX - 1) - x);
				}
			}
			break;
		case EDirection.South:
			newdata = new GridPrefabData(LengtX, LengtY, Vector3.zero);
			for (int y = 0; y < LengtY; y++)
			{
				for (int x = 0; x < LengtX; x++)
				{
					GridPrefabData.SwapGridData(ref data, x, y, ref newdata, (LengtX - 1) - x, (LengtY - 1) - y);
				}
			}
			break;
		case EDirection.West:
			newdata = new GridPrefabData(LengtY, LengtX, Vector3.zero);
			for (int y = 0; y < LengtY; y++)
			{
				for (int x = 0; x < LengtX; x++)
				{
					GridPrefabData.SwapGridData(ref data, x, y, ref newdata, (LengtY - 1) - y, x);
				}
			}
			break;
		default:
			newdata = data;
			break;
	}
	return newdata;
}*/
		public static Quaternion ToQuaternion(this EDirection dir)
		{
			Quaternion rotation = Quaternion.identity;
			switch (dir)
			{
				case EDirection.North:
					//rotation = Quaternion.Euler(0, 00, 0);
					break;
				case EDirection.East:
					rotation = Quaternion.Euler(0, 90, 0);
					break;
				case EDirection.South:
					rotation = Quaternion.Euler(0, 180, 0);
					break;
				case EDirection.West:
					rotation = Quaternion.Euler(0, 270, 0);
					break;
			}
			return rotation;
		}

		public static Vector3 ToVector3(this EDirection dir)
		{
			Vector3 rotation = Vector3.forward;
			switch (dir)
			{
				case EDirection.East:
					rotation = Vector3.right;
					break;
				case EDirection.South:
					rotation = Vector3.back;
					break;
				case EDirection.West:
					rotation = Vector3.left;
					break;
			}
			return rotation;
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}

		public static int BoolListToBitmask(List<bool> list)
		{
			int mask = 0;
			for (int i = 0; i < list.Count; i++)
			{
				mask |= list[i] ? 1 << i : 0;
			}
			return mask;
		}

		public static List<bool> BitmaskToBoolList(int mask, int maxLayers = 64)
		{
			List<bool> boolList = new List<bool>();
			for (int i = 0; i < maxLayers; i++)
			{
				boolList.Add((mask & (1 << i)) != 0);
			}
			return boolList;
		}

		public static T Instantiate<T>(T original)
				where T : UnityEngine.Object
		{
			if (original == null)
			{
				throw new System.ArgumentNullException("original");
			}
			return UnityEngine.Object.Instantiate(original);
		}

		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation)
			where T : UnityEngine.Object
		{
			if (original == null)
			{
				throw new System.ArgumentNullException("original");
			}
			return UnityEngine.Object.Instantiate(original, position, rotation);
		}



		#region Gizmos
		public static void GizmosDrawRect(RectBounds rect, Color col, float padding = 0, float height = 0)
		{
			GizmosDrawRect(new Vector3(rect.X, 0, rect.Y), new Vector3(rect.Maximum.X, 0, rect.Maximum.Y), col, padding, height);
		}
		public static void GizmosDrawRect(Vector3 from, Vector3 to, Color col, float padding = 0, float height = 0)
		{
			Vector3 A = new Vector3(from.x + padding, height, from.y + padding);
			Vector3 B = new Vector3(from.x + padding, height, to.y - padding);
			Vector3 C = new Vector3(to.x - padding, height, to.y - padding);
			Vector3 D = new Vector3(to.x - padding, height, from.y + padding);

			Gizmos.color = col;
			Gizmos.DrawLine(A, B); Gizmos.DrawLine(B, C);
			Gizmos.DrawLine(C, D); Gizmos.DrawLine(D, A);
		}

		internal static List<T> Merge<T>(List<T> a, List<T> b)
		{
			var mergedList = new List<T>();
			for (int i = 0; i < a.Count; i++)
			{
				if (!mergedList.Contains(a[i]))
					mergedList.Add(a[i]);
			}
			for (int i = 0; i < b.Count; i++)
			{
				if (!mergedList.Contains(b[i]))
					mergedList.Add(b[i]);
			}
			return mergedList;
		}
		internal static T[] Merge<T>(T[] a, T[] b)
		{
			var mergedList = new List<T>();
			for (int i = 0; i < a.Length; i++)
			{
				if (!mergedList.Contains(a[i]))
					mergedList.Add(a[i]);
			}
			for (int i = 0; i < b.Length; i++)
			{
				if (!mergedList.Contains(b[i]))
					mergedList.Add(b[i]);
			}
			return mergedList.ToArray();
		}
		#endregion
	}


}