using System;

namespace Endciv
{
    [Serializable]
    public class TemperatureSaveData : ISaveable
    {
		public float temperature;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}