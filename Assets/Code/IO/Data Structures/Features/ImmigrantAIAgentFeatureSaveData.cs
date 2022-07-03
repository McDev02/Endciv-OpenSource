using System;

namespace Endciv
{
	[Serializable]
	public class ImmigrantAIAgentFeatureSaveData : AIAgentFeatureSaveData
	{
		public int state;

		public override ISaveable CollectData()
		{
			return this;
		}
	}
}