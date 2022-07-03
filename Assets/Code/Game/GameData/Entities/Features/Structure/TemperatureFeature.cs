namespace Endciv
{
	public class TemperatureFeature : Feature<TemperatureSaveData>
	{
		//Static Data
		public TemperatureStaticData StaticData { get; private set; }
		private SystemsManager manager;

		public float temperature;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<TemperatureStaticData>();
		}

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			manager.TemperatureSystem.RegisterFeature(this);
		}

		public override void Stop()
		{
			base.Stop();
			manager.TemperatureSystem.DeregisterFeature(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.TemperatureSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.TemperatureSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new TemperatureSaveData();

			return data;
		}

		public override void ApplyData(TemperatureSaveData data)
		{
		}
	}
}