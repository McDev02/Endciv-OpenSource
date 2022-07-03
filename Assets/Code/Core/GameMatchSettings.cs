using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class GameMatchSettings : ScriptableObject
	{
		
		public bool GeneratePlayerCity;
		public float resourceDensity = 0.1f;

		public TerrainSettings terrainSettings;
	}
}