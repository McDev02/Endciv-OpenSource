using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class HousingFeatureSaveData : ISaveable
    {
        public List<string> occupants;
        public string[] workers;
        public bool hasRestocked;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}