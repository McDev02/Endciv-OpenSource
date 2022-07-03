namespace Endciv
{
	public class ElectricityStorageFeature : Feature<ElectricityStorageSaveData>
	{
		//Static Data
		public ElectricityStorageStaticData StaticData { get; private set; }

		SystemsManager manager;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ElectricityStorageStaticData>();
			storageCapacityFactor = 1;
		}

		//Properties
		public float storageCapacityFactor;

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			manager.ElectricitySystem.RegisterStorage(this);
		}

		public override void Stop()
		{
			base.Stop();
			manager.ElectricitySystem.DeregisterStorage(this);
		}

		public override ISaveable CollectData()
		{
			var data = new ElectricityStorageSaveData();
		
			return data;
		}

		public override void ApplyData(ElectricityStorageSaveData data)
		{
		}
	}
}