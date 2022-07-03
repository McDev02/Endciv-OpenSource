using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(EntityFeatureStaticData))]
    public class InventoryStaticData : FeatureStaticData<InventoryFeature>
    {
        public int MaxCapacity;
        public float FoodDecay;
        public float FoodDecayUnpowered;
    }
}
