using System;

namespace Endciv
{
    [Serializable]
    public class CattleFeatureSaveData : ISaveable
    {
		public float producedGoods;

		public ISaveable CollectData()
        {
            return this;
        }
    }
}