namespace Endciv
{
	public class ElectricityProducerFeature : Feature<ElectricityProducerSaveData>
	{
		//Static Data
		public ElectricityProducerStaticData StaticData { get; private set; }
		private SystemsManager manager;
		
		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ElectricityProducerStaticData>();
		}

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			manager.ElectricitySystem.RegisterProducer(this);
		}

		public override void Stop()
		{
			base.Stop();
			manager.ElectricitySystem.DeregisterProducer(this);
		}

		public override ISaveable CollectData()
		{
			var data = new ElectricityProducerSaveData();

			return data;
		}

		public override void ApplyData(ElectricityProducerSaveData data)
		{
		}
	}
}