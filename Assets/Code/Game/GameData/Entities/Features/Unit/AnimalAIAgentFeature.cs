namespace Endciv
{
    public class AnimalAIAgentFeature : AIAgentFeature<AnimalAIAgentFeatureSaveData>
	{
		public override ISaveable CollectData()
		{
			var data = new AnimalAIAgentFeatureSaveData();
			if (CurrentTask != null)
			{
				data.taskData = CurrentTask.CollectData() as TaskSaveData;
			}

			return data;
		}

		public override void ApplyData(AnimalAIAgentFeatureSaveData data)
		{

		}
	}
}