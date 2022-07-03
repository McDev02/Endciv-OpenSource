using System;

namespace Endciv
{
	/// <summary>
	/// The data structure for user related game settings. Used to generate a chain reaction
	/// that eventually sets up all data for saving.
	/// </summary>
	[Serializable]
	public class UserSettingsDataBase : ISaveable
	{
		public GraphicsSettingsData graphicSettings;
        public AudioSettingsData audioSettings;
        public GeneralSettingsDataBase generalSettings;
        public GameInputManagerSaveData inputSettings;
        
		public ISaveable CollectData()
		{
			inputSettings = new GameInputManagerSaveData();
			inputSettings.CollectData();

			graphicSettings = new GraphicsSettingsData();
			graphicSettings.CollectData();

            audioSettings = new AudioSettingsData();
            audioSettings.CollectData();

            generalSettings = new GeneralSettingsDataBase();
            generalSettings.CollectData();
			return null;
		}
	}
}