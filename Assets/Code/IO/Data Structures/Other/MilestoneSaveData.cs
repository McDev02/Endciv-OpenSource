using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class MilestoneSaveData : ISaveable
    {
        public int status;
        public Dictionary<string, NotificationSaveData> objectiveData;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}