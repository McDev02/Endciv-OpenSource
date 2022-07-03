using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class ActionInput
	{
		public string actionName;
		public bool editByUser = true;
		public KeyCode[] keys;
	}
}
