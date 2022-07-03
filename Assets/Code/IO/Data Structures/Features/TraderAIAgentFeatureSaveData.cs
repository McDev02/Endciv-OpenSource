using System;

namespace Endciv
{
    [Serializable]
    public class TraderAIAgentFeatureSaveData : AIAgentFeatureSaveData
    {
        public int state;
        public float waitCounter;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}