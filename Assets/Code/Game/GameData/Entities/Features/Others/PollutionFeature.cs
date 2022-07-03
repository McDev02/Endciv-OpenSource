namespace Endciv
{
	public class PollutionFeature : Feature<PollutionSaveData>
	{
		//Static Data
		public PollutionStaticData StaticData { get; private set; }
		SystemsManager manager;


		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<PollutionStaticData>();
		}

		//Properties
		public float pollution;

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
			var data = new PollutionSaveData();
			data.pollution = pollution;
			return data;
		}

		public override void ApplyData(PollutionSaveData data)
		{
			pollution = data.pollution;
		}
	}
}