using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class NpcSpawnSystemSaveData : ISaveable
    {
        public List<ImmigrationGroupSaveData> immigrationGroups;
        public float newImmigrantCounter;

        public string currentTraderUID;
        public float newTraderCounter;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}
