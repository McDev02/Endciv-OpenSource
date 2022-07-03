using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class WeatherSimulator : MonoBehaviour
	{
		public Perlin perlin;
		List<float> cloudiness = new List<float>();
		List<float> rainvalues = new List<float>();
		List<float> rainfall = new List<float>();
		List<float> snowfall = new List<float>();
		List<float> windvalues = new List<float>();
		List<float> temperatures = new List<float>();
		List<float> stormValues = new List<float>();
		List<float> humidityValues = new List<float>();

		public GraphEditor cloudsGraph;
		public GraphEditor rainGraph;
		public GraphEditor rainfallGraph;
		public GraphEditor snowfallGraph;
		public GraphEditor temperatureGraph;
		public GraphEditor windGraph;
		public GraphEditor stormGraph;
		public GraphEditor humidityGraph;

		WeatherSystem weatherSystem;
		TimeManager timeManager;
		[SerializeField] WorldData worldData;

		[SerializeField] float seasonTime = 0.25f;

		public int Days = 1;
		public int Seed = 0;
		int oldSeed;

		[SerializeField]
		WeatherData summerData;

		[Serializable]
		public class WeatherData
		{
		}

		private void Start()
		{
			timeManager = new TimeManager(null, worldData);
			weatherSystem = new WeatherSystem(timeManager, worldData);

			Seed = (int)(CivRandom.Value * int.MaxValue);
			perlin = new Perlin(Seed);
		}

		void Update()
		{
			if (Seed != oldSeed)
				perlin = new Perlin(Seed);
			oldSeed = Seed;

			cloudiness.Clear();
			rainvalues.Clear();
			rainfall.Clear();
			snowfall.Clear();
			windvalues.Clear();
			temperatures.Clear();
			stormValues.Clear();
			humidityValues.Clear();

			weatherSystem.Wetness = 0;

			for (int i = 0; i < GameConfig.Instance.DayTickLength; i++)
			{
				var dayT = i * Days;

				weatherSystem.blendedWeather = weatherSystem.DaytimeTimeline.GetBlendedNode(dayT);
				weatherSystem.blendedSeason = weatherSystem.SeasonTimeline.GetBlendedNode(seasonTime * worldData.yearDayLength);
				weatherSystem.blendedWeather.Blend(worldData.rainyWeather, weatherSystem.Cloudiness);

#if UNITY_EDITOR
				weatherSystem.UpdateWeather(dayT, Days);
#else
				weatherSystem.UpdateWeather(dayT);
#endif

				cloudiness.Add(weatherSystem.Cloudiness);
				rainvalues.Add(weatherSystem.Rainfall);
				rainfall.Add(weatherSystem.RainfillPerTile);
				snowfall.Add(weatherSystem.Snowfall);
				windvalues.Add(weatherSystem.WindPower);
				temperatures.Add(weatherSystem.Temperature);
				stormValues.Add(weatherSystem.Storm);
				humidityValues.Add(weatherSystem.Wetness);
			}

			cloudsGraph.SetValues(cloudiness.ToArray());
			rainGraph.SetValues(rainvalues.ToArray());
			rainfallGraph.SetValues(rainfall.ToArray());
			snowfallGraph.SetValues(snowfall.ToArray());
			temperatureGraph.SetValues(temperatures.ToArray());
			windGraph.SetValues(windvalues.ToArray());
			stormGraph.SetValues(stormValues.ToArray());
			humidityGraph.SetValues(humidityValues.ToArray());
		}
	}
}