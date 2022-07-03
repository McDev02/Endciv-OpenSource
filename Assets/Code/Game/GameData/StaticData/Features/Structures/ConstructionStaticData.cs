using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class ConstructionStaticData : FeatureStaticData<ConstructionFeature>
    {
        public string Category;
        public bool ShowInConstructionMenu = true;

        public ResourceStack[] Cost;
        public ResourceStack[] DemolitionReturn { get { return Cost; } }
        public float MaxConstructionPoints;
        public int MaxWorkers;
    }
}

