using System;
using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// Graphics related settings data structure.
	/// </summary>
	[Serializable]
	public class GraphicsSettingsData : ISaveable
	{
		public string Setting;
		//Screen
		public int ScreenWidth;
		public int ScreenHeight;
		public bool Fullscreen;
		public bool VSynch;
				
		//General
		public bool Sharpen;

		//Rendering
		public EAASettings AAQuality;
		public EShadowQuality ShadowQuality;
		public ETextureQuality TextureQuality;

		//Special FX
		public bool MotionBlur;
		public bool SSAO;
		public bool Bloom;

		public ISaveable CollectData()
		{
			if (Main.Instance.graphicsManager.TmpSettings == null)
				GetDataFrom(Main.Instance.graphicsManager.GetTemplateData());
			else
				GetDataFrom(Main.Instance.graphicsManager.TmpSettings);
			return this;
		}

		public GraphicsSettingsData() { }
		public GraphicsSettingsData(GraphicsSettingsData other)
		{
			GetDataFrom(other);
		}
		public void GetDataFrom(GraphicsSettingsData other)
		{
			Debug.Log("GetDataFrom()");
			Setting = other.Setting;
			ScreenWidth = other.ScreenWidth;
			ScreenHeight = other.ScreenHeight;
			Fullscreen = other.Fullscreen;
			VSynch = other.VSynch;
			AAQuality = other.AAQuality;
			Sharpen = other.Sharpen;
			ShadowQuality = other.ShadowQuality;
			TextureQuality = other.TextureQuality;
			MotionBlur = other.MotionBlur;
			SSAO = other.SSAO;
			Bloom = other.Bloom;
		}

		public GraphicsSettingsData GetCopy()
		{
			return new GraphicsSettingsData(this);
		}

		[Serializable]
		public enum EQuality
		{
			Off,
			Very_Low,
			Low,
			Medium,
			High,
			Very_High
		}
		[Serializable]
		public enum ETextureQuality
		{
			Low,
			Medium,
			High,
			Very_High
		}
		[Serializable]
		public enum EShadowQuality
		{
			Off,
			//Low,
			Medium,
			High
		}
		[Serializable]
		public enum EAASettings
		{
			Off,
			On
		}
	}
}

