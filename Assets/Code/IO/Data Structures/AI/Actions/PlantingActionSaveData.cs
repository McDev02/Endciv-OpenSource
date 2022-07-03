using System;

namespace Endciv
{
    [Serializable]
    public class PlantingActionSaveData : ActionSaveData, ISaveable
    {
        public float progress;
    }
}