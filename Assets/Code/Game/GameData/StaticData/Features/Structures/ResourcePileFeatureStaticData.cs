using System;
using UnityEngine;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(EntityFeatureStaticData))]
    [EntityCategory("Resource Piles")]
    public class ResourcePileFeatureStaticData : FeatureStaticData<ResourcePileFeature>, 
		IFeatureViewContainer
    {    
        public ResourcePileEntry[] Resources;

		public GameObject[] views;

		public GameObject GetFeatureViewInstance(int variationID = -1)
		{
			if (views == null || views.Length <= 0)
				return null;
			return Instantiate(views[CivRandom.Range(0, views.Length)]);
		}

		public int GetNextViewID(int currentID)
		{
			if (views == null || views.Length <= 0)
				return -1;
			currentID++;
			if (currentID >= views.Length)
				currentID = 0;
			return currentID;
		}

		public int GetRandomViewID()
		{
			if (views == null || views.Length <= 0)
				return -1;
			return CivRandom.Range(0, views.Length);
		}

		[Serializable]
        public struct ResourcePileEntry
        {
            [StaticDataID("StaticData/SimpleEntities/Items")]
            public string resourceID;
            public int minAmount;
            public int maxAmount;
        }
    }
}