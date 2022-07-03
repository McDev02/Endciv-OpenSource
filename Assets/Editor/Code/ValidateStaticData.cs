using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;

namespace Endciv.Editor
{
	[InitializeOnLoad]
	public class ValidateStaticData
	{
		[DidReloadScripts]
		private static void OnScriptsReloaded()
		{
			var entities = Resources.LoadAll<EntityStaticData>("StaticData");
			foreach(var entity in entities)
			{
				var types = EntityStaticDataEditor.GetMissingTypes(entity);
				if (types != null && types.Count > 0)
				{
					Debug.LogError("Entity " + entity.ID + " has Required Features missing!");
				}
			}
		}		
	}
}