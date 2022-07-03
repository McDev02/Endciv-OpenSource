using System;

namespace Endciv
{
    [Serializable]
    public class ElectricityStorageSaveData : ISaveable
	{
		float StorageCapacityFactor;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}