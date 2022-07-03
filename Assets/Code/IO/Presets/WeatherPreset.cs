using UnityEngine;

namespace Endciv
{
	[CreateAssetMenu(fileName = "WeatherPreset", menuName = "Settings/WeatherPreset", order = 1)]
	public class WeatherPreset : ScriptableObject, IBlendedNode<WeatherPreset>
	{
		public float clouds;
		public float rainfall;
		public float snowfall;
		public float temperatureDelta;

		[LocaId]
		public string locaID;
		public string Name { get { return LocalizationManager.GetText( locaID); } }
		public float Length { get; set; }

		public float StartLength { get; set; }
		public float StartFade { get; set; }
		public float EndFade { get; set; }

		public void Blend(WeatherPreset BlendData, float value)
		{
			rainfall = Mathf.Lerp(rainfall, BlendData.rainfall, value);
		}

		public void Blend(WeatherPreset blendPreset, float value, ref WeatherPreset result)
		{
			result.rainfall = rainfall;
			result.Blend(blendPreset, value);
		}
	}
}