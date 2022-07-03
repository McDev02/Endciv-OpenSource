namespace Endciv
{
	public class MiningFeature : Feature<MiningSaveData>
	{
		//Static Data
		public MiningStaticData StaticData { get; private set; }
		private SystemsManager manager;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<MiningStaticData>();
		}

		//Properties
		/// <summary>
		/// Progress indication of resource
		/// </summary>
		public float collectedResource;
		/// <summary>
		/// indicates collection of resource per tick.
		/// </summary>
		public float resourceCollectionRate;

		//Well
		public MiningSystem.WellData wellData;

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			manager.MiningSystem.RegisterFeature(this);
		}

		public override void Stop()
		{
			base.Stop();
			manager.MiningSystem.DeregisterFeature(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.MiningSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.MiningSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new MiningSaveData();
            data.collectedResource = collectedResource;
            data.resourceCollectionRate = resourceCollectionRate;
			return data;
		}

		public override void ApplyData(MiningSaveData data)
		{
            resourceCollectionRate = data.resourceCollectionRate;
            collectedResource = data.collectedResource;
		}
	}
}