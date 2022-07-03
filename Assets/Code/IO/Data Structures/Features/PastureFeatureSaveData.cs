using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class PastureFeatureSaveData : ISaveable
    {
		public List<string> cattle;
		public List<string> reservedCattle;

		public int load;
		public float currentNutrition;
		public float currentWater;
		public float filth;

		public string cleaner;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}