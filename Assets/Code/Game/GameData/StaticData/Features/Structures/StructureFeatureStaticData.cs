using UnityEngine;

namespace Endciv
{
    [RequireFeature(typeof(EntityFeatureStaticData))]
    [EntityCategory("Structures")]
    public class StructureFeatureStaticData : FeatureStaticData<StructureFeature>, 
		IFeatureViewContainer
    {
        public ETechnologyType providingTech;
        public ETechnologyType[] requiredTech;
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
	}

}
