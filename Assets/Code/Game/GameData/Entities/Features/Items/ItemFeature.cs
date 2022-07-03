namespace Endciv
{
	public class ItemFeature : Feature<ItemFeatureSaveData>
	{
		public ItemFeatureStaticData StaticData { get; private set; }
		public int Quantity { get; set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ItemFeatureStaticData>();
		}

		public override ISaveable CollectData()
		{
			var data = new ItemFeatureSaveData();
			data.quantity = Quantity;
			return data;
		}

		public override void ApplyData(ItemFeatureSaveData data)
		{
			Quantity = data.quantity;
		}		
	}
}