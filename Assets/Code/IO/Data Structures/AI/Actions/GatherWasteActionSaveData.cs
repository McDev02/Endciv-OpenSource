using System;

namespace Endciv
{
    [Serializable]
    public class GatherWasteActionSaveData : ActionSaveData, ISaveable
    {
        public int currentIndex;
        public float timer;
    }
}