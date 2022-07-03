using System;

namespace Endciv
{
	[Serializable]
	public class UnitSystemConfig
	{
		public float hungerConsumptionCenter = 2f;
		public float hungerConsumptionBalance = 0.5f;
		public float thirstConsumptionCenter = 2f;
		public float thirstConsumptionBalance = 0.5f;
		
		public float conditionBias = 0.01f;

		public float stressUnderWork;
		public float stressUnderIdle;
		public float stressUnderRelaxing;
	}


	[Serializable]
	public class AgricultureSystemConfig
	{
		public float cropsWateringThreshold = 0.3f;
		public float cropsWateringBuffer = 0.5f;
		public float cropsGrowthRate = 1f;
	}

	[Serializable]
	public class DebugModifersConfig
	{
		public float UnitHungerConsumption = 1f;
		public float UnitThirstConsumption = 1f;
	}
	[Serializable]
	public class GlobalAIConfig
	{
		public float ImmigrationBaseValue = 100f;
		public float ImmigrationFactor = 0.1f;

		public float NewTraderTime = 835;
		public float TraderWaitTicks = 250;

		public float NewDogTime = 100f;
		public float DogSpawnFactor = 0.1f;
		//Bandidts, raiding dogs etc
	}

	[Serializable]
	public class GeneralEconomyValuesConfig
	{
		public int ConstructionSpeed = 1;
		public int DemolitionSpeed = 3;
		public float ConstructionDecay = 0.2f;

		public float GatheringSpeed = 0.2f;
		public float StorageGatheringSpeed = 0.5f;
		public float ProductionSpeed = 0.1f;
	}


	[Serializable]
	public class GeneralSystemsConfig
	{
		public float rainfallPerTile = 15;
		public float rainwaterCollectorFactor = 1f;
		public float groundwaterCollectorFactor = 1f;

		public float energyByWind = 1f;
		public float energyBySolar = 1f;
		public float fuelCombustionPerTick = 0.1f;

		public float temperatureAdaption = 0.05f;
	}

}