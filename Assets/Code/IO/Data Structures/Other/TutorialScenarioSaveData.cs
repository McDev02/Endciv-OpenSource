using System;

namespace Endciv
{
    [Serializable]
    public class TutorialScenarioSaveData : ScenarioSaveData
    {
        public new ISaveable CollectData()
        {
            return this;
        }
    }
}