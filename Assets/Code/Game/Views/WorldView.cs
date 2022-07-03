using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	public class WorldView : MonoBehaviour
	{
		WeatherSystem weatherSystem;
		TimeManager timeManager;
		[SerializeField] CameraController cameraController;
		bool isReady;

		[SerializeField] float AngleOffset = 0;
		[SerializeField] Transform SunGear;
		[SerializeField] Light SunLight;

		[SerializeField] int heavyRainEmission = 400;
		[SerializeField] int heavySnowEmission = 200;
		[SerializeField] float heavyRainTurbulance = 0.005f;
		[SerializeField] float heavySnowTurbulance = 0.005f;

		[SerializeField] float sunDayOffsetMin;
		[SerializeField] float sunDayOffsetMax;

		[SerializeField] Material skyboxWorld;

		[SerializeField] WindZone windZone;
		[SerializeField] ParticleSystem rainParticlePrefab;
		[SerializeField] ParticleSystem snowParticlePrefab;
		private List<ParticleSystem> rainParticles;
		private List<ParticleSystem> snowParticles;
		const float particleTileSize = 24;
		const float particleYOffset = 30;

		public void Run(TimeManager timeManager, WeatherSystem weatherSystem)
		{
			this.timeManager = timeManager;
			this.weatherSystem = weatherSystem;
			isReady = true;

			rainParticles = new List<ParticleSystem>();
			snowParticles = new List<ParticleSystem>();
			for (int i = 0; i < 9; i++)
			{
				rainParticles.Add(Instantiate(rainParticlePrefab));
			}
			for (int i = 0; i < 9; i++)
			{
				var p = Instantiate(snowParticlePrefab);
				snowParticles.Add(p);
			}
			UpdateParticles();

			sunDayOffsetMin = timeManager.DaytimeTimeline.GetStartFadeTime(0);
			sunDayOffsetMax = timeManager.DaytimeTimeline.GetEndFadeTime(0);

			weatherSystem.OnGameLoopUpdate -= UpdateWeather;
			weatherSystem.OnGameLoopUpdate += UpdateWeather;
		}

		// Update is called once per frame
		void Update()
		{
			if (!isReady) return;

			var s = weatherSystem.SeasonTimeline.BlendedNode;

			//float t = Mathf.Clamp01(timeManager.CurrentDaytimeProgress - sunDayOffsetMin) / ((1 - sunDayOffsetMax) - sunDayOffsetMin);
			float t = Mathf.Clamp01((timeManager.CurrentDayTickFloat - sunDayOffsetMin) / (sunDayOffsetMax - sunDayOffsetMin));
			float angle = 35 + s.Altitude + 180 * t;
			SunGear.rotation = Quaternion.AngleAxis(angle, Vector3.up);
			SunGear.Rotate(Vector3.right, CivMath.fQuadratic(Mathf.PingPong(t * 2f, 1f)) * s.Azimuth, Space.Self);
			SunLight.enabled = t > 0 && t < 1;

			windZone.windMain = weatherSystem.WindPower;
			windZone.windTurbulence = weatherSystem.WindTurbulance;
			windZone.transform.LookAt(windZone.transform.position + weatherSystem.WindDirection, Vector3.up);
			UpdateParticles();

			// Shader Globals
			//Shader.SetGlobalVector("_SunDir", (Sun.transform.position - transform.position).normalized);
			//Shader.SetGlobalFloat ( "_Daytime", daytime );
			Color suncol = SunLight.color * SunLight.intensity;
			suncol.a = SunLight.intensity;
			Shader.SetGlobalVector("_GlobalSunColor", suncol);
			Shader.SetGlobalFloat("_GlobalRainfall", weatherSystem.Rainfall);
			Shader.SetGlobalFloat("_GlobalWetness", Mathf.Pow(weatherSystem.Wetness, 0.5f) * 0.55f);
			Shader.SetGlobalFloat("_GlobalSnow", weatherSystem.Snowlevel * 0.85f);

			Shader.SetGlobalColor("_GlobalGrassColor", s.GrassColor);
			Shader.SetGlobalFloat("_GlobalGrassGrowth", s.GrassGrowth);
		}

		void UpdateWeather()
		{
			var w = weatherSystem.DaytimeTimeline.BlendedNode;
			var s = weatherSystem.SeasonTimeline.BlendedNode;

			SunLight.color = w.sunData.Color.ToColor();
			SunLight.intensity = w.sunData.Brightness;

			RenderSettings.ambientSkyColor = w.ambientData.HSBSkyColor.ToColor();
			RenderSettings.ambientEquatorColor = w.ambientData.HSBEquatorColor.ToColor();
			RenderSettings.ambientGroundColor = w.ambientData.HSBGroundColor.ToColor();
			RenderSettings.reflectionIntensity = w.ambientData.ReflectionIntensity;
			cameraController.postProcessingSettings.SetPostExposure(w.cameraFX.LuminanceTonemapper);

			//skyboxWorld.SetColor("_SkyColor", w.ambientData.SkyColor);
			//skyboxWorld.SetColor("_EquatorColor", w.ambientData.EquatorColor);
			//skyboxWorld.SetColor("_GroundColor", w.ambientData.GroundColor);

			//DynamicGI.UpdateEnvironment();
		}

		void UpdateParticles()
		{
			int i = 0;
			ParticleSystem pr;
			ParticleSystem ps;
			Vector3 center = cameraController.transform.position;
			center.x = Mathf.Round(center.x / particleTileSize) * particleTileSize;
			center.z = Mathf.Round(center.z / particleTileSize) * particleTileSize;
			center.y = particleYOffset;
			float rf = Mathf.Pow(weatherSystem.Rainfall, 1.5f);
			float sf = Mathf.Pow(weatherSystem.Snowfall, 1.5f);
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					pr = rainParticles[i];
					ps = snowParticles[i];
					var pos = center + new Vector3(x * particleTileSize, 0, y * particleTileSize);
					pr.transform.position = ps.transform.position = pos;
					var e = pr.emission;
					var s = pr.shape;
					e.rateOverTime = heavyRainEmission * rf;
					s.randomDirectionAmount = heavyRainTurbulance * rf;
					e = ps.emission;
					s = ps.shape;
					e.rateOverTime = heavySnowEmission * sf;
					s.randomDirectionAmount = heavySnowTurbulance * sf;
					i++;
				}
			}
		}
	}
}