using System;

namespace Endciv
{
    [Serializable]
    public class CropFeatureSaveData : ISaveable
    {
        public int cropState;
        public float growFactor;
        public float fruitGrowFactor;
        public float fruits;
        public float seeds;

        public float humidityCurrent;
        public float humidityMax;
        public float progress;

        public int variationID;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}