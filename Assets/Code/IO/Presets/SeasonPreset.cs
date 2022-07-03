using UnityEngine;

namespace Endciv
{
	[CreateAssetMenu(fileName = "SeasonPreset", menuName = "Settings/SeasonPreset", order = 1)]
	public class SeasonPreset : ScriptableObject, IBlendedNode<SeasonPreset>
	{
		/// <summary>
		/// Altitude of Sun at Noon time. 0°< x <90°
		/// </summary>
		public float Altitude;
		/// <summary>
		/// Azimuth of Sun at Sunrise.
		/// </summary>
		[Range(0, 90)]
		public float Azimuth = 50;

		public Color GrassColor;
		public float GrassGrowth;

		public float BaseTemperature;
		public float SunPower;
		public float cloudProbability;
		public float stormProbability;
		public float rainProbability;
		public float SnowProbability;
		public float VegetationGrowth;
		//public float CropGrowth;

		[LocaId]
		public string locaID;
		public string Name { get { return LocalizationManager.GetText(locaID); } }
		public float Length { get; set; }

		public float StartLength { get; set; }
		public float StartFade { get; set; }
		public float EndFade { get; set; }

		public HSBColor HSBGrassColor
		{
			get
			{
				HSBColor color = HSBColor.FromColor(GrassColor);
				return color;
			}
		}
		public void Blend(SeasonPreset BlendData, float value)
		{
			Azimuth = Mathf.Lerp(Azimuth, BlendData.Azimuth, value);
			Altitude = Mathf.Lerp(Altitude, BlendData.Altitude, value);

			GrassColor = HSBColor.ToColor(HSBColor.Lerp(HSBGrassColor, BlendData.HSBGrassColor, value));
			GrassGrowth = Mathf.Lerp(GrassGrowth, BlendData.GrassGrowth, value);
			VegetationGrowth = Mathf.Lerp(VegetationGrowth, BlendData.VegetationGrowth, value);
			SnowProbability = Mathf.Lerp(SnowProbability, BlendData.SnowProbability, value);
			BaseTemperature = Mathf.Lerp(BaseTemperature, BlendData.BaseTemperature, value);
			SunPower = Mathf.Lerp(SunPower, BlendData.SunPower, value);
			cloudProbability = Mathf.Lerp(cloudProbability, BlendData.cloudProbability, value);
			stormProbability = Mathf.Lerp(stormProbability, BlendData.stormProbability, value);
		}

		public void Blend(SeasonPreset blendPreset, float value, ref SeasonPreset result)
		{
			result.Altitude = Altitude;
			result.Azimuth = Azimuth;

			result.GrassColor = GrassColor;
			result.GrassGrowth = GrassGrowth;
			result.VegetationGrowth = VegetationGrowth;
			result.SnowProbability = SnowProbability;
			result.BaseTemperature = BaseTemperature;
			result.cloudProbability = cloudProbability;
			result.stormProbability = stormProbability;

			result.Blend(blendPreset, value);
		}
	}
}