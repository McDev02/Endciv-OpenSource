using UnityEngine;
namespace Endciv
{
	public class GraphicSettingsPreset : ScriptableObject
	{
		//General
		public bool Sharpen;

		//Rendering
		public GraphicsSettingsData.EAASettings AAQuality;
		public GraphicsSettingsData.EShadowQuality ShadowQuality;
		public GraphicsSettingsData.ETextureQuality TextureQuality;

		//Special FX
		public bool SSAO;
		public bool Bloom;

		//Preset to Settings Converter
		public GraphicsSettingsData ToGraphicSettingsData()
		{
			var data = new GraphicsSettingsData();
			data.Setting = name;
			data.AAQuality = AAQuality;
			data.Sharpen = Sharpen;
			data.ShadowQuality = ShadowQuality;
			data.TextureQuality = TextureQuality;
			data.SSAO = SSAO;
			data.Bloom = Bloom;
			return data;
		}
	}
}