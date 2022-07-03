namespace Endciv
{
    public class TraderAIAgentFeature : AIAgentFeature<TraderAIAgentFeatureSaveData>
	{
		public TraderStaticData traderData;
		public NpcSpawnSystem.ETraderState state;
		public float waitCounter;

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.TraderUnits, Entity, true);
		}

		public override void Stop()
		{
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TraderUnits, Entity, true);
			base.Stop();
		}

		public override void Destroy()
		{
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TraderUnits, Entity);
			base.Destroy();
		}

		public override ISaveable CollectData()
		{
			var data = new TraderAIAgentFeatureSaveData();
			if (CurrentTask != null)
			{
				data.taskData = CurrentTask.CollectData() as TaskSaveData;
			}
            data.state = (int)state;
            data.waitCounter = waitCounter;
			return data;
		}

		public override void ApplyData(TraderAIAgentFeatureSaveData data)
		{
			if (data == null)
				return;
            state = (NpcSpawnSystem.ETraderState)data.state;
            waitCounter = data.waitCounter;
		}
	}
}