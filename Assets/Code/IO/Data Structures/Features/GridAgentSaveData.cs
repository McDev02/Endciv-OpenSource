using System;

namespace Endciv
{
    [Serializable]
    public class GridAgentSaveData : ISaveable
    {
        public LocationSaveData destination;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}