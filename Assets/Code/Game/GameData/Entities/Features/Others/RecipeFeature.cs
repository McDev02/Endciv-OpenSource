using UnityEngine;

namespace Endciv
{
	public class RecipeFeature : Feature<RecipeFeatureSaveData>
	{
		public RecipeFeatureStaticData StaticData { get; private set; }

		//Amount of resources ordered for production
		//Edit: In total
		public int targetAmount;
		//Amount of resources that should be produced minimum
		public int minAmount;
		//Amount of resources currently being produced by production facilities
		public int amountInProgress;
		public int amountCompleted;

		//Priority set by production system based on the current state of production lines
		public float currentPriority;

		//Resources remaining to be produced
		public int BatchesLeft
		{
			get
			{
				//Maybe we update this in a field only once in a loop.
				return Mathf.Max(0, targetAmount - (amountCompleted + amountInProgress));
			}
		}

		public float CurrentProgress = 0f;

		//Whether an AI entity is currently producing this order
		public bool InProduction = false;

		public float TotalProgress
		{
			get
			{
				return amountCompleted / (float)targetAmount;
			}
		}

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = entity.StaticData.GetFeature<RecipeFeatureStaticData>();
		}

		public override void ApplyData(RecipeFeatureSaveData data)
		{
			targetAmount = data.targetAmount;
			minAmount = data.minAmount;
			amountInProgress = data.amountInProgress;
			amountCompleted = data.amountCompleted;
			currentPriority = data.currentPriority;
			CurrentProgress = data.currentProgress;
			InProduction = data.inProduction;
		}

		public override ISaveable CollectData()
		{
			var data = new RecipeFeatureSaveData();
			data.targetAmount = targetAmount;
			data.minAmount = minAmount;
			data.amountInProgress = amountInProgress;
			data.amountCompleted = amountCompleted;
			data.currentPriority = currentPriority;
			data.currentProgress = CurrentProgress;
			data.inProduction = InProduction;
			return data;
		}
	}
}

