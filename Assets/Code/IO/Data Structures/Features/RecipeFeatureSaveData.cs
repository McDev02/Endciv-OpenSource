using System;

namespace Endciv
{
	[Serializable]
	public class RecipeFeatureSaveData : ISaveable
	{
		public int targetAmount;
		public int minAmount;
		public int amountInProgress;
		public int amountCompleted;
		public float currentPriority;
		public float currentProgress;
		public bool inProduction;

		public ISaveable CollectData()
		{
			return this;
		}
	}
}