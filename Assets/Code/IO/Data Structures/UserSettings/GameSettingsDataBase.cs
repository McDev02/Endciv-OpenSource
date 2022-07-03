using System;
namespace Endciv
{
	/// <summary>
	/// The data structure for non user related game settings. Used to generate a chain reaction
	/// that eventually sets up all data for saving.
	/// </summary>
	[Serializable]
	public class GameSettingsDataBase : ISaveable
	{
		public ISaveable CollectData()
		{
			return null;
		}
	}
}