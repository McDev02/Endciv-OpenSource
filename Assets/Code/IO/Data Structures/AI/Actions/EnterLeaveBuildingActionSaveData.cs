using System;

namespace Endciv
{
    [Serializable]
    public class EnterLeaveBuildingActionSaveData : ActionSaveData, ISaveable
    {
        public LocationSaveData destination;
        public bool goToEntrancePoint;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}