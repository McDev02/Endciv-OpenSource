using System;

namespace Endciv
{
	[Serializable]
	public class ItemFeatureSaveData : ISaveable
	{
		public int quantity;

		public ISaveable CollectData()
		{
			return this;
		}
	}
}
