using System;
using System.Collections.Generic;

namespace Endciv
{
	[Serializable]
	public class AnimalSpawnSystemSaveData : ISaveable
	{
		public List<string> currentDogsUIDs;
		public float newDogCounter;

		public ISaveable CollectData()
		{
			return this;
		}
	}
}
