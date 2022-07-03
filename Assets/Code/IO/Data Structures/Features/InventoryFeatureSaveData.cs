using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class InventoryFeatureSaveData : ISaveable
    {
        [Serializable]
        public class Chamber
        {
            public string chamberName;
            public int chamberID;            
			public List<EntitySaveData> items;
        }

        public List<Chamber> Chambers;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}