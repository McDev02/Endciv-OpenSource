using System;

namespace Endciv
{
    [Serializable]
    public class ProductionFeatureSaveData : ISaveable
    {
        public EntitySaveData[] recipes;
        public string[] workers;
        public string[] activeWorkers;
        public string[] transporters;

        public string structureUID;

        public ISaveable CollectData()
        {
            return this;
        }
    }

}