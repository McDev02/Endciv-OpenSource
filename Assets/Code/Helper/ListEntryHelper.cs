using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class ListEntryHelper<T> where T : MonoBehaviour
	{
		public T prefab;
		public Transform listContainer;
		[NonSerialized]
		public List<T> rationEntries;

		public ListEntryHelper()
		{
			rationEntries = new List<T>();

			//Add existing entries
			var children = listContainer.GetComponentsInChildren<T>();
			rationEntries.AddRange(children);
		}
	}
}