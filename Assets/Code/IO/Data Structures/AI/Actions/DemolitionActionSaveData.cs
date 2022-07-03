using System;

namespace Endciv
{
    [Serializable]
    public class DemolitionActionSaveData : ActionSaveData, ISaveable
    {
        public bool inDemolition;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}