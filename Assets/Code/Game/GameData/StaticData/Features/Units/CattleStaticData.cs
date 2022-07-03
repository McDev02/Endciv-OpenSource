using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(UnitFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class CattleStaticData : FeatureStaticData<CattleFeature>, INonStackableFeature
    {
		[StaticDataID("StaticData/SimpleEntities/Items")]
		public string ProducedItem;
        public float ProductionAmount;
		public int ProductionCapacity;
		public bool RequiresHarvesting;
		public int Leather;
    }
}
