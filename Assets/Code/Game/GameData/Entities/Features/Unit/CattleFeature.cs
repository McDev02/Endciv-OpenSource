namespace Endciv
{
	/// <summary>
	/// Unit which must eat and drink
	/// </summary>
	public class CattleFeature : Feature<CattleFeatureSaveData>
	{
		public CattleStaticData staticData;

		public PastureFeature Pasture { get; set; }
		
		public float ProducedGoods { get; set; }

		public bool HasHome
		{
			get
			{
				return Pasture != null;
			}
		}
		
		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			staticData = Entity.StaticData.GetFeature<CattleStaticData>();
		}

		public override void ApplyData(CattleFeatureSaveData data)
		{
			ProducedGoods = data.producedGoods;		
		}

		public override ISaveable CollectData()
		{
			var data = new CattleFeatureSaveData();
			data.producedGoods = ProducedGoods;			
			return data;
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			manager.PastureSystem.RegisterCattle(this);
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.PastureSystem.DeregisterCattle(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.PastureSystem.DeregisterCattle(this, oldFaction);
			SystemsManager.PastureSystem.RegisterCattle(this);
		}
	}
}