using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class ScenarioSaveData : ISaveable
    {
        public Dictionary<string, MilestoneSaveData> milestoneData;
        public int currentMilestoneID;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}