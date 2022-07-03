using System;

namespace Endciv
{
    [Serializable]
    public class StructureFeatureSaveData : ISaveable
    {
        public ISaveable CollectData()
        {
            return this;
        }
    }
}