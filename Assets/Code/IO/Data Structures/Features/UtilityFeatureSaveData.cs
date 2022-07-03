using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class UtilityFeatureSaveData : ISaveable
    {
        public List<string> occupants;
        public float condition;
        public ISaveable CollectData()
        {
            return this;
        }
    }
}