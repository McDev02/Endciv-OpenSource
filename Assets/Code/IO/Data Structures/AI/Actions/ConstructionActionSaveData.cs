using System;

namespace Endciv
{
    [Serializable]
    public class ConstructionActionSaveData : ActionSaveData, ISaveable
    {
        public bool inConstruction;
        public bool willTakeBreak;
        public int breakTimer;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}
