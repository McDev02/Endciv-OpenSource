using System;

namespace Endciv
{
	[Serializable]
	public class ConsumableFeatureSaveData : ISaveable
	{
		public ISaveable CollectData()
		{
			return this;
		}
	}
}