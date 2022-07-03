using System;

namespace Endciv
{
    [Serializable]
    public class UnitFeatureSaveData : ISaveable
    {
		public EUnitType unitType;
        public bool isVisible;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}