using System;

namespace Endciv
{
    [Serializable]
    public class ResourceSaveData : ISaveable
    {
        public string id;
        public int amount;

        public ResourceSaveData()
        {

        }

        public ResourceSaveData(string id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }

        public ISaveable CollectData()
        {
            return this;
        }
    }
}