using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class ExpeditionFeatureStaticData : FeatureStaticData<ExpeditionFeature>,
		IFeatureViewContainer
	{
		public GameObject viewPrefab;

		public float deathChance;
		public MinMax deathRatio;
		public MinMax lootCarryFactor;
		[SerializeField]
		public ResourceGenerationEntry[] lootPool;		

		[Serializable]
		public struct ResourceGenerationEntry
		{
			public float probability;
			[StaticDataID("StaticData/SimpleEntities/Items")]
			public List<string> foodPool;
		}

		public GameObject GetFeatureViewInstance(int variationID = -1)
		{
			return Instantiate(viewPrefab);
		}

		public int GetNextViewID(int currentID)
		{
			return 0;
		}

		public int GetRandomViewID()
		{
			return 0;
		}
	}
}