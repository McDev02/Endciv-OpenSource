using System;

namespace Endciv
{
	[Serializable]
	public class DurabilityFeatureSaveData : ISaveable
	{
		public float durability;

		public ISaveable CollectData()
		{			
			return this;
		}
	}
}