using System;

namespace Endciv
{
    [Serializable]
    public class AnimalAIAgentFeatureSaveData : AIAgentFeatureSaveData
    {
        public override ISaveable CollectData()
        {
            return this;
        }
    }
}