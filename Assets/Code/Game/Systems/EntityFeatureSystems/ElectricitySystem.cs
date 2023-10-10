using System.Collections.Generic;
using System;

namespace Endciv
{
	/// <summary>
	/// Manages electricity distribution
	/// </summary>
	public class ElectricitySystem : BaseGameSystem
	{
		private List<List<ElectricityProducerFeature>> Producers;
		private List<List<PowerSourceElectricityFeature>> Consumers;
		private List<List<ElectricityStorageFeature>> Storages;

		public List<float> TotalProduction { get; private set; }
		public List<float> TotalConsumption { get; private set; }
		public List<float> TotalBalance { get; private set; }

		public List<float> TotalCapacity { get; private set; }
		public List<float> TotalStorage { get; private set; }

		public ElectricitySystem(int factions) : base()
		{
			Producers = new List<List<ElectricityProducerFeature>>(factions);
			Consumers = new List<List<PowerSourceElectricityFeature>>(factions);
			Storages = new List<List<ElectricityStorageFeature>>(factions);

			TotalProduction = new List<float>(factions);
			TotalConsumption = new List<float>(factions);
			TotalBalance = new List<float>(factions);
			TotalCapacity = new List<float>(factions);
			TotalStorage = new List<float>(factions);

			for (int i = 0; i < factions; i++)
			{
				Producers.Add(new List<ElectricityProducerFeature>());
				Consumers.Add(new List<PowerSourceElectricityFeature>());
				Storages.Add(new List<ElectricityStorageFeature>());

				TotalProduction.Add(0);
				TotalConsumption.Add(0);
				TotalBalance.Add(0);
				TotalCapacity.Add(0);
				TotalStorage.Add(0);
			}
			UpdateStatistics();
		}

		// ----------------------------------------
		// Note: Factions are not properly implemented maybe on all de/registration methods!
		// ----------------------------------------

		public void RegisterProducer(ElectricityProducerFeature feature)
		{
			if (!Producers[feature.FactionID].Contains(feature))
				Producers[feature.FactionID].Add(feature);
		}

		public void DeregisterProducer(ElectricityProducerFeature feature, int faction = -1)
		{
			if (faction < 0) faction = feature.FactionID;

			if (Producers[faction].Contains(feature))
				Producers[faction].Remove(feature);
		}

		public void RegisterConsumer(PowerSourceElectricityFeature feature)
		{

			if (!Consumers[feature.FactionID].Contains(feature))
				Consumers[feature.FactionID].Add(feature);
		}

		public void DeregisterConsumer(PowerSourceElectricityFeature feature, int faction = -1)
		{
			if (faction < 0) faction = feature.FactionID;

			if (Consumers[faction].Contains(feature))
				Consumers[faction].Remove(feature);
		}

		public void RegisterStorage(ElectricityStorageFeature feature)
		{
			if (!Storages[feature.FactionID].Contains(feature))
				Storages[feature.FactionID].Add(feature);
		}

		public void DeregisterStorage(ElectricityStorageFeature feature, int faction = -1)
		{
			if (faction < 0) faction = feature.FactionID;

			if (Storages[faction].Contains(feature))
				Storages[faction].Remove(feature);
		}

		public override void UpdateGameLoop()
		{
			UpdateElectricityGrid();
		}

		void UpdatePowerSources()
		{

		}
		void UpdateElectricityGrid()
		{
			//Energy production
			for (int f = 0; f < Producers.Count; f++)
			{
				TotalProduction[f] = 0;
				for (int i = 0; i < Producers[f].Count; i++)
				{
					var facility = Producers[f][i];
					var gain = facility.StaticData.Production * facility.effectivity;

					TotalProduction[f] += gain;
					TotalStorage[f] += gain;
				}
			}

			//Energy consumption
			for (int f = 0; f < Consumers.Count; f++)
			{
				TotalConsumption[f] = 0;
				//Sort consumers by priority
				var consumers = Consumers[f];
				consumers.Sort((x, y) => x.Priority.CompareTo(y.Priority));

				float lastPriority = float.MaxValue;
				for (int i = 0; i < consumers.Count; i++)
				{
					var facility = consumers[i];

					if (lastPriority < facility.Priority) { Debug.LogError("Sorting is wrong."); }
					lastPriority = facility.Priority;

					var consumption = facility.StaticData.Consumption * facility.consumptionFactor;
					if (TotalStorage[f] < consumption || consumption <= 0) continue;

					//Consumption Successful
                    foreach(var facilityConsumer in facility.consumers)
                    {
                        facilityConsumer.effectivity = 1f;
                    }					
					TotalConsumption[f] += consumption;
					TotalStorage[f] -= consumption;
				}
				TotalBalance[f] = TotalProduction[f] - TotalConsumption[f];
			}


			//Energy storage
			for (int f = 0; f < Storages.Count; f++)
			{
				TotalCapacity[f] = 0;
				for (int i = 0; i < Storages[f].Count; i++)
				{
					var facility = Storages[f][i];
					//Simulate Capacity decay
					facility.storageCapacityFactor -= facility.StaticData.Decay;
					if (facility.storageCapacityFactor <= 0)
						facility.storageCapacityFactor = 0;
					else
						TotalCapacity[f] += facility.StaticData.Capacity * facility.storageCapacityFactor;
				}

				TotalStorage[f] = CivMath.Clamp0X(TotalStorage[f], TotalCapacity[f]);
			}
			UpdateStatistics();
		}

		public override void UpdateStatistics()
		{
			GameStatistics.MainTownStatistics.TotalElectricityProduction = TotalProduction[SystemsManager.MainPlayerFaction];
			GameStatistics.MainTownStatistics.TotalElectricityConsumption = TotalConsumption[SystemsManager.MainPlayerFaction];
			GameStatistics.MainTownStatistics.TotalElectricityBalance = TotalBalance[SystemsManager.MainPlayerFaction];
			GameStatistics.MainTownStatistics.TotalElectricityCapacity = TotalCapacity[SystemsManager.MainPlayerFaction];
			GameStatistics.MainTownStatistics.TotalElectricityStored = TotalStorage[SystemsManager.MainPlayerFaction];
		}
	}
}