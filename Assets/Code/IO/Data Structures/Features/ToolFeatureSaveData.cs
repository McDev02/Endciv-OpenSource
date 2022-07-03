using System;

namespace Endciv
{
	[Serializable]
	public class ToolFeatureSaveData : ISaveable
	{
		public ISaveable CollectData()
		{
			return this;
		}
	}
}