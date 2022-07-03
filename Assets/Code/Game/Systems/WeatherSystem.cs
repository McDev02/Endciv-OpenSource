using System;
using UnityEngine;

namespace Endciv
{
	public class WeatherSystem : BaseGameSystem
	{
		//Settings
		/// <summary>
		/// How much rainfall affects wetness
		/// </summary>
		public const float RainfallWetnessFactor = 0.015f;
		/// <summary>
		/// How much water evaporates in open containers
		/// </summary>
		public const float WaterEvaporateRate = 0.1f;
		/// <summary>
		/// How much rainfall affects wetness
		/// </summary>
		public const float SnowfallWetnessFactor = 0.018f;
		/// <summary>
		/// How much rainfall affects wetness
		/// </summary>
		public const float SnowfallWetnessMeltingFactor = 2f;

		//Values
		/// <summary>
		/// Amount of clouds
		/// </summary>
		public float Cloudiness;
		/// <summary>
		/// Amount of clouds
		/// </summary>
		public float SunPower;
		/// <summary>
		/// How much rain is on the ground
		/// </summary>
		public float Wetness;
		/// <summary>
		/// How much snow is on the ground
		/// </summary>
		public float Snowlevel;
		/// <summary>
		/// Indicates rain power 0-1
		/// </summary>
		public float Rainfall;
		/// <summary>
		/// Indicates snow power 0-1
		/// </summary>
		public float Snowfall;
		/// <summary>
		/// Indicates storm power 0-1
		/// </summary>
		public float Storm;
		/// <summary>
		/// Fillrate by rain per grid tile per tick
		/// </summary>
		public float RainfillPerTile;
		/// <summary>
		/// Current temperature in Celius - (char)176 + "C";
		/// </summary>
		public float Temperature;
		/// <summary>
		/// Current temperature in Fahrenheit - (char)176 + "F"
		/// </summary>
		public float TemperatureF { get { return Temperature * 1.8f + 32; } }

		/// <summary>
		/// Power of the wind
		/// </summary>
		public float WindPower { get; set; }
		public Vector3 WindDirection;
		//Visual values
		public float WindTurbulance { get; set; }
		public Vector3 SolarDirection { get; private set; }

		public WorldData worldData { get; private set; }
		public BlendedTimeline<DaytimePreset> DaytimeTimeline { get { return timeManager.DaytimeTimeline; } }  //Rename to Daytime or else
		public BlendedTimeline<SeasonPreset> SeasonTimeline { get { return timeManager.SeasonTimeline; } }
		//public BlendedTimeline<WeatherPreset> WeatherTimeline { get; }

		public DaytimePreset blendedWeather;
		public SeasonPreset blendedSeason;

		public Action OnGameLoopUpdate;
#if UNITY_EDITOR
		public bool updateSymulation = true;
#endif

		Perlin perlin;
		TimeManager timeManager;

		public WeatherSystem(TimeManager timeManager, WorldData worldData) : base()
		{
			this.timeManager = timeManager;
			this.worldData = worldData;

			perlin = new Perlin(4155);
			//Todo fix solar direction to sundir calculation
			SolarDirection = new Vector3(0.5f, 0, 1).normalized;
		}

		public override void UpdateGameLoop()
		{
			blendedWeather = DaytimeTimeline.GetBlendedNode(timeManager.CurrentDayTickFloat);
			blendedSeason = SeasonTimeline.GetBlendedNode(timeManager.totalDaysFloat);
			blendedWeather.Blend(worldData.rainyWeather, Cloudiness);

#if UNITY_EDITOR
			if (updateSymulation)
				UpdateWeather(timeManager.CurrentDayTickFloat, 1f);
#else
			UpdateWeather(timeManager.CurrentDayTickFloat);
#endif

			OnGameLoopUpdate?.Invoke();
		}


		public override void UpdateStatistics()
		{
		}

		public void SetCloudiness(float v)
		{
			Cloudiness = v;
		}

#if UNITY_EDITOR
		public void UpdateWeather(float t, float days)
#else
		public void UpdateWeather(float t)
#endif
		{
			var w = blendedWeather.weatherData;

			Storm = Mathf.Pow(Mathf.Clamp01((perlin.Noise(t * 0.00475f) + blendedSeason.stormProbability) * 2.5f), 2);
			Storm = Mathf.Clamp01(Storm * 2);

			Cloudiness = 1.4f * (perlin.Noise(t * 0.004f)) + blendedSeason.cloudProbability + Storm;
			WindPower = Mathf.Clamp01(perlin.Noise(t * 0.0095f) + 0.1f + Cloudiness * 0.5f) * 2;

			WindDirection.x = 0.25f + perlin.Noise(t * 0.0137f);
			WindDirection.z = 0.25f + perlin.Noise(t * 0.0097f, 5);
			WindDirection.Normalize();

			Cloudiness = Mathf.Clamp01(Cloudiness);
			Rainfall = Mathf.Clamp01((Cloudiness - 0.1f) * (1f / 0.9f));
			float snowRatio = Mathf.Clamp01((2 - Temperature) * (1f / 4f));
			Snowfall = Rainfall * snowRatio;
			Rainfall *= 1 - snowRatio;
			Temperature = blendedSeason.BaseTemperature + w.TemperatureOffset + (perlin.Noise(t * 0.0075f) - Cloudiness) * 12;

			SunPower = w.SunPower * blendedSeason.SunPower;
			RainfillPerTile = Mathf.Pow(Rainfall, 2f) * GameConfig.Instance.GeneralSystemsData.rainfallPerTile * 24 * timeManager.dayTickFactor;

			var snowDelta = Snowfall * SnowfallWetnessFactor - w.HumidDryout * Mathf.Max(0, (Temperature - 1) * (1f / 15));
			var humidityDelta = Rainfall * RainfallWetnessFactor - w.HumidDryout * Mathf.Max(0, (Temperature + 10) * (1f / 25));
			//Add humidity when snow melts
			if (Snowlevel > 0.01f)
				humidityDelta += Mathf.Abs(-snowDelta) * SnowfallWetnessMeltingFactor;

#if UNITY_EDITOR
			Snowlevel = Mathf.Clamp01(Snowlevel + days * snowDelta);
			Wetness = Mathf.Clamp01(Wetness + days * humidityDelta);
#else
			Snowlevel = Mathf.Clamp01(Snowlevel + snowDelta);
			Wetness = Mathf.Clamp01(Wetness + humidityDelta);
#endif
		}
	}
}