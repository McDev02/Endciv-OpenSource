using System;

namespace Endciv
{
    [Serializable]
    public class BuildGraveActionSaveData : ActionSaveData, ISaveable
    {
        public int graveID;
    }
}