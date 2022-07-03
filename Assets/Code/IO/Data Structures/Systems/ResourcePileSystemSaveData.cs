using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class ResourcePileSystemSaveData : ISaveable
    {
        public List<string> workerIDs;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}