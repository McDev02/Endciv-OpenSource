using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Endciv
{
	public class NotificationValues : ResourceSingleton<NotificationValues>
	{
		[SerializeField] public List<string> names = new List<string>();
		[SerializeField] public List<EValueType> values = new List<EValueType>();

		public bool AddValue(string name, EValueType value)
		{
#if UNITY_EDITOR
			if (values.Count != names.Count)
			{
				Debug.LogError("Names and Values lists do not match!");
				return false;
			}
#endif
			if (names.Contains(name))
				return false;

			names.Add(name);
			values.Add(value);
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
#endif
			return true;

		}
		public bool DoValuesMatch()
		{
			return values.Count == names.Count;
		}
		public bool RemoveValue(int id)
		{
#if UNITY_EDITOR
			if (values.Count != names.Count)
			{
				Debug.LogError("Names and Values lists do not match!");
				return false;
			}
#endif
			if (id < 0 || id >= names.Count || id >= values.Count)
				return false;

			names.RemoveAt(id);
			values.RemoveAt(id);
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
#endif
			return true;

		}

		public bool UpdateValue(int id, string newKey, EValueType newValue)
		{
			if (id < 0 || id >= names.Count || id >= values.Count)
				return false;
			if (names[id] != newKey && names.Contains(newKey))
				return false;

			names[id] = newKey;
			values[id] = newValue;
			return true;
		}

		public bool ValueExists(int id)
		{
			return id >= 0 && id < names.Count && id < values.Count;
		}
	}
}