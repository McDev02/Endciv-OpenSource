using UnityEngine;

namespace Endciv
{
	public static class Logger
	{
		public static void Log(string text)
		{
#if _DEBUG || UNITY_EDITOR
			Debug.Log(text);
#endif
		}
	}
}