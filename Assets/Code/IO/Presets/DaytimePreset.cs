using UnityEngine;

namespace Endciv
{
	[CreateAssetMenu(fileName = "DaytimePreset", menuName = "Settings/DaytimePreset", order = 1)]
	public class DaytimePreset : ScriptableObject, IBlendedNode<DaytimePreset>
	{
		//Weather Effects
		public WeatherSunData sunData;
		public WeatherAmbientData ambientData;
		public WeatherData weatherData;
		public WeatherFogData fogData;
		public WeatherCameraFX cameraFX;

		public WeatherAISettings aiData;

		[LocaId]
		public string locaID;
		public string Name { get { return LocalizationManager.GetText(locaID); } }
		public float Length { get; set; }

		public float StartLength { get; set; }
		public float StartFade { get; set; }
		public float EndFade { get; set; }

		public void Blend(DaytimePreset BlendData, float value)
		{
			sunData.Blend(BlendData.sunData, value);
			ambientData.Blend(BlendData.ambientData, value);
			weatherData.Blend(BlendData.weatherData, value);
			fogData.Blend(BlendData.fogData, value);
			cameraFX.Blend(BlendData.cameraFX, value);
			aiData.Blend(BlendData.aiData, value);
		}


		public void Blend(DaytimePreset blendPreset, float value, ref DaytimePreset result)
		{
			result.sunData = sunData;
			result.ambientData = ambientData;
			result.weatherData = weatherData;
			result.fogData = fogData;
			result.cameraFX = cameraFX;
			result.aiData = aiData;

			result.Blend(blendPreset, value);
		}
	}
}