using System;

namespace Endciv
{
	[Serializable]
	public class WeaponFeatureSaveData : ISaveable
	{
		public ISaveable CollectData()
		{
			return this;
		}
	}
}