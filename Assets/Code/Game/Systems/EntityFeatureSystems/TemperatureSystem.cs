using System;


namespace Endciv
{
	/// <summary>
	/// A system containing all Structure Entitites
	/// </summary>
	public class TemperatureSystem : EntityFeatureSystem<TemperatureFeature>
	{
		WeatherSystem weatherSystem;
		public TemperatureSystem(WeatherSystem weatherSystem, int factions) : base(factions)
		{
			this.weatherSystem = weatherSystem;
		}

		public Action OnStructureAdded;
		public Action OnStructureRemoved;

		internal override void RegisterFeature(TemperatureFeature feature)
		{
			base.RegisterFeature(feature);
			OnStructureAdded?.Invoke();
		}
		internal override void DeregisterFeature(TemperatureFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			OnStructureRemoved?.Invoke();
		}

		public override void UpdateGameLoop()
		{
			var data = GameConfig.Instance.GeneralSystemsData;
			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				var feature = FeaturesCombined[i];
				feature.temperature = Mathf.Lerp(feature.temperature, weatherSystem.Temperature, data.temperatureAdaption);
			}
		}

		public override void UpdateStatistics()
		{
		}
	}
}