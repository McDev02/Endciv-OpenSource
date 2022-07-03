using System;
namespace Endciv
{
	/// <summary>
	/// Audio related settings data structure.
	/// </summary>
	[Serializable]
	public class AudioSettingsData : ISaveable
	{
        public string Setting;
        public float totalVolume;
        public float musicVolume;
        public float soundVolume;
        public float uiVolume;

		public ISaveable CollectData()
		{
            if(Main.Instance.audioManager.TmpSettings == null)
                GetDataFrom(Main.Instance.audioManager.GetTemplateData());
            else
                GetDataFrom(Main.Instance.audioManager.TmpSettings);
            return this;
		}

        public AudioSettingsData() { }
        public AudioSettingsData(AudioSettingsData other)
        {
            GetDataFrom(other);
        }
        public void GetDataFrom(AudioSettingsData other)
        {
            if (other == null)
                return;
            Setting = other.Setting;
            totalVolume = other.totalVolume;
            musicVolume = other.musicVolume;
            soundVolume = other.soundVolume;
            uiVolume = other.uiVolume;            
        }

        public AudioSettingsData GetCopy()
        {
            return new AudioSettingsData(this);
        }
    }
}