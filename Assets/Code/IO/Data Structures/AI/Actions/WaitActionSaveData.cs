using System;

namespace Endciv
{
    [Serializable]
    public class WaitActionSaveData : ActionSaveData, ISaveable
    {
        public float duration;
        public float timer;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}