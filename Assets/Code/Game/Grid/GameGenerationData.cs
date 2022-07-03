using UnityEngine;
using System.Collections.Generic;
using System;

namespace Endciv
{
	[CreateAssetMenu(fileName = "GameGenerationData", menuName = "Settings/GameGenerationData", order = 1)]
	public class GameGenerationData : ScriptableObject
	{
		[SerializeField]
		public ResourceGenerationEntry[] startingResources;
		[SerializeField]
		public float[] mapResourcesFactor;

		[Serializable]
		public struct ResourceGenerationEntry
		{
			public float waterFactor;
			public float waterMin;

			public float foodFactor;
			public float foodMin;
			[StaticDataID("StaticData/SimpleEntities/Items/Food")]
			public List<string> foodPool;

			public float materialFactor;
			public float materialMin;
			[StaticDataID("StaticData/SimpleEntities/Items")]
			public List<string> materialPool;

			public float weaponsFactor;
			public float weaponsMin;
			[StaticDataID("StaticData/SimpleEntities/Items/Weapons")]
			public List<string> weaponsPool;
		}
	}
}