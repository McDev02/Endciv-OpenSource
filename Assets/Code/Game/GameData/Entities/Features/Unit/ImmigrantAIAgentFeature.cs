namespace Endciv
{
	public enum EImmigrantState
	{
		Arriving,
		Waiting,
		Leaving
	}

	public class ImmigrantAIAgentFeature : AIAgentFeature<ImmigrantAIAgentFeatureSaveData>
	{
		public EImmigrantState State { get; set; }

        public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
        {
            base.Setup(entity, args);
            SetAIAgentSettings(Main.Instance.GameManager.gameMechanicSettings.aiSettings.citizenClasses[0]);
        }

        public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.ImmigrantUnits, Entity, true);
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ImmigrantUnits, Entity, true);
		}

		public override void Destroy()
		{
			base.Destroy();
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ImmigrantUnits, Entity, true);
		}

		public override void ApplyData(ImmigrantAIAgentFeatureSaveData data)
		{
			State = (EImmigrantState)data.state;
		}

		public override ISaveable CollectData()
		{
			var data = new ImmigrantAIAgentFeatureSaveData();
			data.state = (int)State;
			return data;
		}
	}
}