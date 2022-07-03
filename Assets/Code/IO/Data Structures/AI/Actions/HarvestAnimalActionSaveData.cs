using System;

namespace Endciv
{
	[Serializable]
	public class HarvestAnimalActionSaveData : ActionSaveData, ISaveable
	{
		public float progress;
	}
}