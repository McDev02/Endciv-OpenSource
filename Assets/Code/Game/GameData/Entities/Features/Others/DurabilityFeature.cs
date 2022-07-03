namespace Endciv
{
	public class DurabilityFeature : Feature<DurabilityFeatureSaveData>
	{
		public DurabilityFeatureStaticData StaticData { get; private set; }
		public float Durability { get; set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<DurabilityFeatureStaticData>();
			Durability = StaticData.maxDurability;
		}

		public void SetDurability(float value)
		{
			Durability = value;
		}

		public void AddDurability(float value)
		{
			Durability = Durability + value;
		}

		public override void ApplyData(DurabilityFeatureSaveData data)
		{
			Durability = data.durability;
		}

		public override ISaveable CollectData()
		{
			var data = new DurabilityFeatureSaveData();
			data.durability = Durability;
			return data;
		}
	}
}