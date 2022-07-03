using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class AgricultureSystemSaveData : ISaveable
    {
        public Dictionary<string, float> seeds;
        public List<string> workerUIDs;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}