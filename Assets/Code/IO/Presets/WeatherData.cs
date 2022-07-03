using UnityEngine;

namespace Endciv
{
	//Weather Effects
	[System.Serializable]
	public struct WeatherSunData
	{
		public float Hue;
		public float HueFade;
		public float Saturation;
		public float SaturationFade;

		public float Brightness;
		public bool BrightnessMultiply;

		public float ShadowSoftness;
		public bool ShadowAffection;

		public HSBColor Color
		{
			get { return new HSBColor(Hue, Saturation, 1, 0); }
		}

		public void Blend(WeatherSunData BlendData, float value)
		{
			Hue = Mathf.Lerp(Hue, BlendData.Hue, value * BlendData.HueFade);
			Saturation = Mathf.Lerp(Saturation, BlendData.Saturation, value * BlendData.SaturationFade);
			if (BlendData.BrightnessMultiply)
				Brightness = Mathf.Lerp(Brightness, Brightness * BlendData.Brightness, value);
			else
				Brightness = Mathf.Lerp(Brightness, BlendData.Brightness, value);
			if (BlendData.ShadowAffection)
			{
				ShadowSoftness = Mathf.Lerp(ShadowSoftness, BlendData.ShadowSoftness, value);
			}
		}
	}

	[System.Serializable]
	public struct WeatherAmbientData
	{
		public Color SkyColor;
		public Color EquatorColor;
		public Color GroundColor;
		public bool ColorMultiply;

		public float Saturation;
		public float Brightness;
		public float ReflectionIntensity;

		public HSBColor HSBSkyColor
		{
			get
			{
				HSBColor color = HSBColor.FromColor(SkyColor);
				color.S *= Saturation;
				color.B *= Brightness;
				return color;
			}
		}
		public HSBColor HSBEquatorColor
		{
			get
			{
				HSBColor color = HSBColor.FromColor(EquatorColor);
				color.S *= Saturation;
				color.B *= Brightness;
				return color;
			}
		}
		public HSBColor HSBGroundColor
		{
			get
			{
				HSBColor color = HSBColor.FromColor(GroundColor);
				color.S *= Saturation;
				color.B *= Brightness;
				return color;
			}
		}

		public void Blend(WeatherAmbientData BlendData, float value)
		{

			if (BlendData.ColorMultiply)
			{
				Brightness *= BlendData.Brightness;
				Saturation *= BlendData.Saturation;

				ReflectionIntensity = Mathf.Lerp(ReflectionIntensity, ReflectionIntensity * BlendData.ReflectionIntensity, value);
			}
			else
			{
				Brightness =Mathf.Lerp(Brightness, BlendData.Brightness,value);
				Saturation = Mathf.Lerp(Saturation, BlendData.Saturation, value);

				SkyColor = HSBColor.ToColor(HSBColor.Lerp(SkyColor, BlendData.SkyColor, value));
				EquatorColor = HSBColor.ToColor(HSBColor.Lerp(EquatorColor, BlendData.EquatorColor, value));
				GroundColor = HSBColor.ToColor(HSBColor.Lerp(GroundColor, BlendData.GroundColor, value));

				ReflectionIntensity = Mathf.Lerp(ReflectionIntensity, BlendData.ReflectionIntensity, value);
			}
		}
	}

	[System.Serializable]
	public struct WeatherFogData
	{
		public float Hue;
		public float HueFade;
		public float Saturation;
		public float SaturationFade;
		public float Brightness;
		public float Distance;
		public float Density;
		public float DensityBlendExponent;

		public bool BrightnessMultiply;

		public HSBColor Color
		{
			get { return new HSBColor(Hue, Saturation, Brightness, 0); }
		}

		public void Blend(WeatherFogData BlendData, float value)
		{
			Hue = Mathf.Lerp(Hue, BlendData.Hue, value * BlendData.HueFade);
			Saturation = Mathf.Lerp(Saturation, BlendData.Saturation, value * BlendData.SaturationFade);
			if (BlendData.BrightnessMultiply)
				Brightness = Mathf.Lerp(Brightness, Brightness * BlendData.Brightness, value);
			else
				Brightness = Mathf.Lerp(Brightness, BlendData.Brightness, value);

			Density = Mathf.Lerp(Density, BlendData.Density, Mathf.Pow(value, BlendData.DensityBlendExponent));
			Distance = Mathf.Lerp(Distance, BlendData.Distance, Mathf.Pow(value, BlendData.DensityBlendExponent));
		}
	}

	[System.Serializable]
	public struct WeatherData
	{
		public float SunPower;
		public float HumidDryout;
		public float TemperatureOffset;
		public bool TemperatureAdditive;

		public void Blend(WeatherData BlendData, float value)
		{
			if (BlendData.TemperatureAdditive)
				TemperatureOffset += Mathf.Lerp(0, BlendData.TemperatureOffset, value);
			else
				TemperatureOffset = Mathf.Lerp(TemperatureOffset, BlendData.TemperatureOffset, value);

			HumidDryout = Mathf.Lerp(HumidDryout, BlendData.HumidDryout, value);
			SunPower = Mathf.Lerp(SunPower, BlendData.SunPower, value);
		}
	}

	//Camera Effects

	[System.Serializable]
	public struct WeatherCameraFX
	{
		public float LuminanceTonemapper;

		public bool LuminanceMultiply;

		public void Blend(WeatherCameraFX BlendData, float value)
		{
			if (BlendData.LuminanceMultiply)
				LuminanceTonemapper = Mathf.Lerp(LuminanceTonemapper, LuminanceTonemapper * BlendData.LuminanceTonemapper, value);
			else
				LuminanceTonemapper = Mathf.Lerp(LuminanceTonemapper, BlendData.LuminanceTonemapper, value);
		}
	}

	//AI Settings

	[System.Serializable]
	public struct WeatherAISettings
	{
		public Vector2 BirdsFlying;
		public Vector2 BirdsLanding;
		public Vector2 CrowsFlying;
		public bool Multiply;

		public void Blend(WeatherAISettings BlendData, float value)
		{
			if (Multiply)
			{
				BirdsFlying = Vector2.Lerp(BirdsFlying, CivMath.MulVector(BirdsFlying, BlendData.BirdsFlying), value);
				BirdsLanding = Vector2.Lerp(BirdsLanding, CivMath.MulVector(BirdsFlying, BlendData.BirdsLanding), value);
				CrowsFlying = Vector2.Lerp(CrowsFlying, CivMath.MulVector(BirdsFlying, BlendData.CrowsFlying), value);
			}
			else
			{
				BirdsFlying = Vector2.Lerp(BirdsFlying, BlendData.BirdsFlying, value);
				BirdsLanding = Vector2.Lerp(BirdsLanding, BlendData.BirdsLanding, value);
				CrowsFlying = Vector2.Lerp(CrowsFlying, BlendData.CrowsFlying, value);
			}
		}
	}
}