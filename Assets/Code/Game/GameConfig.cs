namespace Endciv
{
	public class GameConfig : ResourceSingleton<GameConfig>
	{
		public const float WaterPortion = 0.25f;

		public float FoodDecay = 0.1f;

		#region Special settings
		public float StorageCapacityMultipler = 1;
		#endregion

		public int DayTickLength = 10 * 60;
		public int YearDayLength = 40;

		public GeneralEconomyValuesConfig GeneralEconomyValues;
		public UnitSystemConfig UnitSystemData;
		public GeneralSystemsConfig GeneralSystemsData;
		public AgricultureSystemConfig AgricultureSystemData;
		public DebugModifersConfig DebugModifers;
		public GlobalAIConfig GlobalAIData;
			   
		#region Temporary Values
		public float CityDensityViewThreshold = 1;
		public float PollutionViewThreshold = 1;
		public float GroundWaterViewThreshold = 1;
	
		public int CityDensityIterations = 4;
		public float CityDensityMaxValue = 4;
		public float PollutionMaxValue = 4;

		public float CityDensityMaxDistance = 16;
		public float CityDensityDistancePower = 2;
		public float PollutionDistancePower = 2;

		public float OpenAreaDistance = 3;

		public float pathfindingOpenAreaCost = 8;
		public int pathfindingTargetReachedBias = 8;


		#endregion
	}
}