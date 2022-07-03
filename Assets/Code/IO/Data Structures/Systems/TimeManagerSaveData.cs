using System;

namespace Endciv
{
    [Serializable]
    public class TimeManagerSaveData : ISaveable
    {
        public float currentTickProgress;
        public float currentDaytimeProgress;
        public int currentTotalTick;
        public int currentDayTick;
        public int dayTickLength;
        public float dayTickFactor;
        public int currentDay;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}