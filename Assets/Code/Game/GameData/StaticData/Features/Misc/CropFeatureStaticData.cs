using UnityEngine;

namespace Endciv
{
    public class CropFeatureStaticData : FeatureStaticData<CropFeature>,
        IFeatureViewContainer
    {
        public float seeds;
        /// <summary>
        /// Water consumption per day
        /// </summary>
        public float waterConsumption;
        public float waterMaxValue;

        /// <summary>
        /// Grow time in Days
        /// </summary>
        public int growTime;
        public MinMaxi fruitAmount;
        [SerializeField] [StaticDataID("StaticData/SimpleEntities/Items")] public string fruit;

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
