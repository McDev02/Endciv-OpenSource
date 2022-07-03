namespace Endciv
{
	public class ConsumableFeature : Feature<ConsumableFeatureSaveData>
	{		
		public ConsumableFeatureStaticData StaticData { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ConsumableFeatureStaticData>();
		}

		public override ISaveable CollectData()
		{
			var data = new ConsumableFeatureSaveData();
			return data;
		}

		public override void ApplyData(ConsumableFeatureSaveData data)
		{
			
		}
	}
}