using System;

namespace Endciv
{
    [Serializable]
    public class ElectricityProducerSaveData : ISaveable
    {
        public ISaveable CollectData()
        {
            return this;
        }
    }
}