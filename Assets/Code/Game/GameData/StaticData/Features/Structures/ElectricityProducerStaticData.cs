using System;

namespace Endciv
{
    [Serializable]
    public class ElectricityProducerStaticData : FeatureStaticData<ElectricityProducerFeature>
    {
        public int Production;
    }
}
