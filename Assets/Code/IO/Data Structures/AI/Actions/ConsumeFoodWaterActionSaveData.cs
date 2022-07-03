using System;

namespace Endciv
{
    [Serializable]
    public class ConsumeFoodWaterActionSaveData : ActionSaveData, ISaveable
    {
        public float timer;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}