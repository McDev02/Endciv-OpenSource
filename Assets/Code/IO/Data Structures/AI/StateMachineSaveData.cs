using System;

namespace Endciv
{
    [Serializable]
    public class StateMachineSaveData : ISaveable
    {
        public string lastState;
        public string currentState;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}