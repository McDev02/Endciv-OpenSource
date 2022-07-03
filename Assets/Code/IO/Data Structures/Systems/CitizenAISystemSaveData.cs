using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class CitizenAISystemSaveData : ISaveable
    {
        public List<string> consumableFilter;
        public List<OccupationSettingSaveData> settings;

        public int waterConsumePortions;
        public int nutritionConsumePortions;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}