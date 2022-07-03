using System;
namespace Endciv
{

	#region Geometry
	//Vector Float
	[Serializable]
	public class SerVector2
	{
		public float X;
		public float Y;
		public UnityEngine.Vector2 ToVector2() { return new UnityEngine.Vector2(X, Y); }
	}
	[Serializable]
	public class SerVector3
	{
		public float X;
		public float Y;
		public float Z;
		public SerVector3() { }
		public SerVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		public UnityEngine.Vector3 ToVector3() { return new UnityEngine.Vector3(X, Y, Z); }
		public static SerVector3 FromVector3(UnityEngine.Vector3 v)
		{
			return new SerVector3(v.x, v.y, v.z);
		}
	}

	[Serializable]
	public class SerVector4
	{
		public float X;
		public float Y;
		public float Z;
		public float W;
		public SerVector4() { }
		public SerVector4(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		public UnityEngine.Vector4 ToVector4()
		{
			return new UnityEngine.Vector4(X, Y, Z, W);
		}
		public UnityEngine.Quaternion ToQuaternion()
		{
			return new UnityEngine.Quaternion(X, Y, Z, W);
		}
		public static SerVector4 FromVector4(UnityEngine.Vector4 v)
		{
			return new SerVector4(v.x, v.y, v.z, v.w);
		}
		public static SerVector4 FromQuaternion(UnityEngine.Quaternion q)
		{
			return new SerVector4(q.x, q.y, q.z, q.w);
		}

	}

	//Vector Integer
	[Serializable]
	public class SerVector2i
	{
		public int X;
		public int Y;
		public SerVector2i() { }
		public SerVector2i(int x, int y)
		{
			X = x;
			Y = y;
		}
		public SerVector2i(Vector2i vec)
		{
			X = vec.X;
			Y = vec.Y;
		}

		internal Vector2i ToVector2i()
		{
			return new Vector2i(X, Y);
		}
	}
	[Serializable]
	public class SerVector3i
	{
		public int X;
		public int Y;
		public int Z;

	}

	[Serializable]
	public class SerVector4i
	{
		public int X;
		public int Y;
		public int Z;
		public int W;
	}
	#endregion

	public static class VectorExtensionMethods
	{
		public static SerVector2 ToSerVector2(this UnityEngine.Vector2 vec)
		{
			var result = new SerVector2();
			result.X = vec.x;
			result.Y = vec.y;
			return result;
		}

		public static SerVector2[] ToSerVector2Array(this UnityEngine.Vector2[] arr)
		{
			if (arr == null)
				return new SerVector2[0];

			SerVector2[] result = new SerVector2[arr.Length];
			for (int i = 0; i < arr.Length; i++)
			{
				result[i] = arr[i].ToSerVector2();
			}
			return result;
		}

		public static UnityEngine.Vector2[] ToVector2Array(this SerVector2[] arr)
		{
			UnityEngine.Vector2[] result = new UnityEngine.Vector2[arr.Length];
			for (int i = 0; i < arr.Length; i++)
			{
				result[i] = arr[i].ToVector2();
			}
			return result;
		}
	}
}