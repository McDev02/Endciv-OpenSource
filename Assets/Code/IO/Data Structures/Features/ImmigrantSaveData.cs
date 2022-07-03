using System;

namespace Endciv
{
    [Serializable]
    public class ImmigrantSaveData : ISaveable
    {
        public int age;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}