using System;

namespace Endciv
{
    [Serializable]
    public class PowerSourceElectricitySaveData : ISaveable
    {
        public float userDefinedPriority;
        public bool hasConsumed;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}