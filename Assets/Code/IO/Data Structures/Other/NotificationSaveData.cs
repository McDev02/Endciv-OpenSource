using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class NotificationSaveData : ISaveable
    {
        public int status;
        public string finalDescription;
        public List<NotificationConditionSaveData> triggers;
        public List<NotificationConditionSaveData> completions;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}