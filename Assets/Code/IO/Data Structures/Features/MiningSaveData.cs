using System;

namespace Endciv
{
    [Serializable]
    public class MiningSaveData : ISaveable
    {
        public float collectedResource;
        public float resourceCollectionRate;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}