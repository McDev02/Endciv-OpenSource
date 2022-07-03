using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Endciv
{
	public abstract class GUIStyle : ScriptableObject
	{
		public static bool autoUpdate;
		public bool forceUpdate;
		[NonSerialized] public bool unappliedChanges;

		public abstract void EnableAll();
		public abstract void DisableAll();

		public void ApplyChangesToAllObjects()
		{
			forceUpdate = false;
			var objects = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IGUIStyle>().ToList();

			for (int i = 0; i < objects.Count; i++)
			{
				var obj = objects[i];

				bool hasStyleType = false;
				for (int s = 0; s < obj.Styles.Count; s++)
				{
					if (obj.Styles[s] == this)
					{
						hasStyleType = true;
						break;
					}
				}
				if (hasStyleType)
					obj.UpdateStyle();
			}
		}
	}
}