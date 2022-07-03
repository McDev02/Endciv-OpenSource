using System;

namespace Endciv
{
    [Serializable]
    public class CitizenAIAgentFeatureSaveData : AIAgentFeatureSaveData
    {
        public int occupation;
        public SheduleAIData lastSheduleAIData;
        public float foodVariationNeed;
        public float foodQualityNeed;
        public float toiletNeed;
        public float cleaningNeed;
		
        public override ISaveable CollectData()
        {
            return this;
        }
    }
}