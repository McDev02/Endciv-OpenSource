using System;

namespace Endciv
{
	[Serializable]
	public class PowerSourceCombustionSaveData : ISaveable
	{


		public ISaveable CollectData()
		{
			return this;
		}
	}
	[Serializable]
	public class PowerSourceRenewableSaveData : ISaveable
	{


		public ISaveable CollectData()
		{
			return this;
		}
	}
}