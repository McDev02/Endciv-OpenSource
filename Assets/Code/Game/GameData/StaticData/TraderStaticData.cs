using System.Collections.Generic;
using System;

namespace Endciv
{
	public class TraderStaticData : BaseStaticData
	{
		public float mood = 1f;
		public string TraderName;

		public float tradeSpread = 1.1f;
		public float thresholdMed;
		public float thresholdBad;

		public bool acceptsCattle;
		public ResourceGenerationEntry[] tradingResources;

		public Dictionary<string, int> GenerateTradingList()
		{
			var returnList = new Dictionary<string, int>();

			for (int i = 0; i < tradingResources.Length; i++)
			{
				var resList = tradingResources[i];
				int count = UnityEngine.Random.Range((int)resList.minMax.min, (int)resList.minMax.max);
				for (int j = 0; j < count; j++)
				{
					var itm = resList.materialPool.SelectRandom();
					if (returnList.ContainsKey(itm))
						returnList[itm]++;
					else
						returnList.Add(itm, 1);
				}
			}
			return returnList;
		}

		[Serializable]
		public struct ResourceGenerationEntry
		{
			public MinMax minMax;
			[StaticDataID("StaticData", typeof(ItemFeatureStaticData))]
			public List<string> materialPool;
		}
	}
}