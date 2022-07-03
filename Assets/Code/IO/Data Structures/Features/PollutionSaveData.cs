using System;

namespace Endciv
{
    [Serializable]
    public class PollutionSaveData : ISaveable
	{
		public float pollution;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}