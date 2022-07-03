namespace Endciv
{
	public enum EConsumptionType { Food, Drink, Raw }

	public class ConsumableFeatureStaticData : FeatureStaticData<ConsumableFeature>
	{
		public float Nutrition;
		public float Water;

		public EConsumptionType ConsumptionType;
	}
}