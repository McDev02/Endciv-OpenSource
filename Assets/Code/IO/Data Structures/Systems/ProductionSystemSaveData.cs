using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class ProductionSystemSaveData : ISaveable
    {
        public EntitySaveData[] globalOrders;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}