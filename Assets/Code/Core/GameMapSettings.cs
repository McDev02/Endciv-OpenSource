using UnityEngine;
using System;

namespace Endciv
{
	/// <summary>
	/// When you add members, implement them in Clone()
	/// </summary>
	public class GameMapSettings : 
        ScriptableObject, ISaveable, ILoadable<GameMapSettings.GameMapSettingsSaveData>
	{
		public string ID;
		public bool GeneratePlayerCity;
		public float resourceDensity = 0.1f;
		public TerrainSettings.EMapSize mapSize = TerrainSettings.EMapSize.Medium;
		public int startingCitizens = 3;
		public GameGenerationData.ResourceGenerationEntry startingResources;

		public TerrainSettings terrainSettings;
		public WorldData worldData;
		public ScenarioBase[] Scenarios;
		public ScenarioMapSettings userSettings;
		public GameGenerationData generationData;

		[LocaId]
		public string title;
		[LocaId]
		public string description;

		[Serializable]
		public struct ScenarioMapSettings
		{
			public bool enableTerrainSizeSettings;
			public TerrainSettings.EMapSize minMapSize;
			public TerrainSettings.EMapSize maxMapSize;
			public TerrainSettings.EMapSize defaultMapSize;

			public bool enableResourcesSettings;
			public EQuantitySetting minResourceSize;
			public EQuantitySetting maxResourceSize;
			public EQuantitySetting defaultResourceSize;

			public bool enableCitizenSettings;
			public int minCitizens;
			public int maxCitizens;
			public int defaultCitizens;

			public bool enableStartingResourcesSettings;
			public EQuantitySetting minStartingResourceSize;
			public EQuantitySetting maxStartingResourceSize;
			public EQuantitySetting defaultStartingResourceSize;
		}

		public enum EQuantitySetting { None, Low, Medium, High }

		[Serializable]
		public class GameMapSettingsSaveData : ISaveable
		{
			public string id;
			public bool generatePlayerCity;
			public float resourceDensity;
			public int startingCitizens;
			public TerrainSettingsSaveData terrainSettings;

			public ISaveable CollectData()
			{
				return this;
			}
		}

		public GameMapSettings Clone()
		{
			var settings = ScriptableObject.CreateInstance<GameMapSettings>();
			settings.ID = ID;
			settings.GeneratePlayerCity = GeneratePlayerCity;
			settings.resourceDensity = resourceDensity;
			settings.startingCitizens = startingCitizens;
			settings.terrainSettings = terrainSettings.Clone();
			settings.worldData = worldData.Clone();
			//Scenarios can not be edited so we do shallow copy of them
			settings.Scenarios = Scenarios;

			settings.title = title;
			settings.description = description;
			return settings;
		}

		public ISaveable CollectData()
		{
			var data = new GameMapSettingsSaveData();
			data.id = ID;
			data.generatePlayerCity = GeneratePlayerCity;
			data.resourceDensity = resourceDensity;
			data.startingCitizens = startingCitizens;
			data.terrainSettings = (TerrainSettingsSaveData)terrainSettings.CollectData();
			return data;
		}

		public void ApplySaveData(GameMapSettingsSaveData data)
		{
			if (data == null)
				return;			
			ID = data.id;
			GeneratePlayerCity = data.generatePlayerCity;
			resourceDensity = data.resourceDensity;
			startingCitizens = data.startingCitizens;
			terrainSettings.ApplySaveData(data.terrainSettings);
		}
	}
}