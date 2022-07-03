using System;

namespace Endciv
{
    [Serializable]
    public class ActionSaveData : ISaveable
    {
        public int status;

        public virtual ISaveable CollectData()
        {
            return this;
        }
    }
}