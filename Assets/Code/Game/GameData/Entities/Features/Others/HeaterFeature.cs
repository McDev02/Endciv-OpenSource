namespace Endciv
{
	public class HeaterFeature : Feature<HeaterSaveData>
	{
		//Static Data
		public HeaterStaticData StaticData { get; private set; }
		SystemsManager manager;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<HeaterStaticData>();
		}

		//Properties

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			//Register to Thermal system?
		}

		public override void Stop()
		{
			base.Stop();
			//Deregister from Thermal system?
		}

		public override ISaveable CollectData()
		{
			var data = new HeaterSaveData();

			return data;
		}

		public override void ApplyData(HeaterSaveData data)
		{
		}
	}
}