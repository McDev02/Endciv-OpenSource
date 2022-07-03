namespace Endciv
{
	public class UnitFeatureParams : FeatureParams<UnitFeature>
	{
		public ELivingBeingAge Age { get; set; }
		public ELivingBeingGender Gender { get; set; }
		public EUnitType UnitType { get; set; }
	}
}
