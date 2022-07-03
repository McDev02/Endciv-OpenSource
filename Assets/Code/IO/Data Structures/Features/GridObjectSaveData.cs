using System;

namespace Endciv
{
    [Serializable]
    public class GridObjectSaveData : ISaveable
    {
        public int direction;
        public GridObjectData gridObjectData;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}