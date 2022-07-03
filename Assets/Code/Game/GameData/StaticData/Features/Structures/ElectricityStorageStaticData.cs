using System;

namespace Endciv
{
    [Serializable]
    public class ElectricityStorageStaticData : FeatureStaticData<ElectricityStorageFeature>
    {
        public int Capacity;
        public float Decay;
    }
}
