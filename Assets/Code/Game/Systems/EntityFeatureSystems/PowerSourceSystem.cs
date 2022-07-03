using System.Collections.Generic;
using System;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Manages electricity distribution
	/// </summary>
	public class PowerSourceSystem : BaseGameSystem
	{
		public enum EPowerType { Solar, Wind, Combustion }
		private List<PowerSourceCombustionFeature> CombustionSources;
		private List<PowerSourceRenewableFeature> RenewableSources;

		WeatherSystem weatherSystem;

		public PowerSourceSystem(WeatherSystem weatherSystem) : base()
		{
			this.weatherSystem = weatherSystem;

			CombustionSources = new List<PowerSourceCombustionFeature>();
			RenewableSources = new List<PowerSourceRenewableFeature>();

			UpdateStatistics();
		}

		// ----------------------------------------
		// Note: Factions are not properly implemented maybe on all de/registration methods!
		// ----------------------------------------

		public void RegisterPowerSource(PowerSourceCombustionFeature feature)
		{
			if (!CombustionSources.Contains(feature))
				CombustionSources.Add(feature);
		}

		public void DeregisterPowerSource(PowerSourceCombustionFeature feature)
		{
			CombustionSources.Remove(feature);
		}

		public void RegisterPowerSource(PowerSourceRenewableFeature feature)
		{
			if (!RenewableSources.Contains(feature))
				RenewableSources.Add(feature);
		}

		public void DeregisterPowerSource(PowerSourceRenewableFeature feature)
		{
			RenewableSources.Remove(feature);
		}

		public override void UpdateGameLoop()
		{
			var config = GameConfig.Instance.GeneralSystemsData;

			//shall not be necessary but we could iterate over all consumers and set thair gain to 0 here.

			for (int i = 0; i < CombustionSources.Count; i++)
			{
				var source = CombustionSources[i];
				if (source.consumers == null || source.consumers.Count <= 0)
					continue;

				//Calculate Power
				float output = 0;
				if (source.currentFuel >= 0.01f)
				{
					var consumption = config.fuelCombustionPerTick * source.StaticData.fuelCombustionRate;
					source.currentFuel -= consumption;
					output = source.currentFuel >= 0 ? 1 : 0;
					if (source.currentFuel < 0)
						source.currentFuel = 0;
				}
				//Adapt output power
				source.powerOutput = CivMath.LerpStep(source.powerOutput, output, 0.34f);

				foreach (var sourceConsumer in source.consumers)
				{
					//Apply power to consumer
					sourceConsumer.effectivity = source.powerOutput;
				}

			}
			for (int i = 0; i < RenewableSources.Count; i++)
			{
				var source = RenewableSources[i];
				if (source.consumers == null || source.consumers.Count <= 0)
					continue;

				//Calculate Power
				float powerOutput = 0;
				switch (source.StaticData.powerType)
				{
					case EPowerType.Solar:
						powerOutput = weatherSystem.SunPower * GameConfig.Instance.GeneralSystemsData.energyBySolar;
						break;
					case EPowerType.Wind:
						powerOutput = weatherSystem.WindPower * GameConfig.Instance.GeneralSystemsData.energyByWind;
						break;
				}

				//Apply power to consumer
				foreach (var sourceConsumer in source.consumers)
				{
					//Apply power to consumer
					sourceConsumer.effectivity = powerOutput;
				}
			}

			UpdateStatistics();
		}

		public override void UpdateStatistics()
		{
		}
	}
}