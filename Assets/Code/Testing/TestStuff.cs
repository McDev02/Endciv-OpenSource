using UnityEngine;

namespace Endciv.Testing
{
	public class TestStuff : MonoBehaviour
	{
		int ArraySize = 256;
		MyClass[,] ClassArray;
		MyStruct[,] StructArray;
		// Use this for initialization

		public float RootX(float f, int order)
		{
			if (order < 1) return float.NaN;
			if (f < 0)
			{
				if (order % 2 != 1) return float.NaN;
				return -Mathf.Pow(Mathf.Abs(f), 1f / order);
			}
			else
				return Mathf.Pow(f, 1f / order);
		}

		void Start()
		{
		}

		struct MyStruct
		{
			public float value;

			public MyStruct(float val)
			{
				value = val;
			}
			public int GetSize()
			{
				int size = 0;
				size += sizeof(float);
				return size;
			}
		}
		class MyClass
		{
			public float value;

			public MyClass(float val)
			{
				value = val;
			}
		}
	}
}