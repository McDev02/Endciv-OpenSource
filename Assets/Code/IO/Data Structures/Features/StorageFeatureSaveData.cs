using System;

namespace Endciv
{
    [Serializable]
    public class StorageFeatureSaveData : ISaveable
    {
        public int storagePolicy;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}