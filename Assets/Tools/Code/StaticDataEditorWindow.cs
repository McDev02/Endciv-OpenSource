using UnityEngine;
using System;
using System.Collections.Generic;

namespace Endciv
{
	public class StaticDataEditorWindow : MonoBehaviour
	{
		[SerializeField] StaticDataEditorController Controller;

		internal void UpdateDataList(Dictionary<string, EntityStaticData> structureData)
		{
			throw new NotImplementedException();
		}
	}
}