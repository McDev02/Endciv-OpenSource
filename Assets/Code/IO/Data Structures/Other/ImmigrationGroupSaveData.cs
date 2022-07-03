using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class ImmigrationGroupSaveData : ISaveable
    {
        public List<string> immigrantUIDs;
        public int timeRemaining;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}