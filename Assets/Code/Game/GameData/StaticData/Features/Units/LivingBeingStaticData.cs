using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(UnitFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class LivingBeingStaticData : FeatureStaticData<LivingBeingFeature>
    {
        /// <summary>
        /// Consumption per Day
        /// </summary>
        public AgeStatFloat consumeHunger;
        public AgeStatFloat maxHunger;

        /// <summary>
        /// Consumption per Day
        /// </summary>
        public AgeStatFloat consumeThirst;
        public AgeStatFloat maxThirst;

        public AgeStatInt innards;
        public AgeStatInt meat;

        public AgeStatFloat exhaustionStateThreshold;
        public AgeStatFloat immobilizeStateThreshold;

		public MinMax ThirstUrgencyThreshold;
        public MinMax HungerUrgencyThreshold;
        public MinMax StressUrgencyThreshold;
        public MinMax HealthUrgencyThreshold;

        public float startingAge;
        public float adulthood;
        public float lifeExpectancy;
    }

}
