using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class NotificationSystemSaveData : ISaveable
    {
        public Dictionary<string, NotificationSaveData> notifications;
        public List<ScenarioSaveData> scenarios;
        public Dictionary<string, object> notificationVariables;
        public int currentScenarioID;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}