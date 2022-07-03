using System;

namespace Endciv
{
    [Serializable]
    public abstract class AIAgentFeatureSaveData : ISaveable
    {
        public object taskData;

        public abstract ISaveable CollectData();
    }
}