using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Endciv
{
	public class ScenarioController : GUIAnimatedPanel
	{
		[SerializeField] MainMenuController mainMenuController;
		[SerializeField] Button buttonReference;
		[SerializeField] TabController tabController;
		[SerializeField] GameMapSettings[] mapSettings;

		[SerializeField] Button startGameButton;

		[SerializeField] Text description;
		[SerializeField] GameObject settingsObject;

		[SerializeField] GameObject mapSizeObject;
		[SerializeField] UIDropdown mapSizeDropdown;

		[SerializeField] GameObject resourcesObject;
		[SerializeField] UIDropdown resourcesDropdown;

		[SerializeField] GameObject citizensObject;
		[SerializeField] Slider citizensSlider;
		[SerializeField] Text citizensValue;

		[SerializeField] GameObject startingResourcesObject;
		[SerializeField] UIDropdown startingResourcesDropdown;
		int currentScenario;

		private string[] mapSizeNames;
		private int minMapIndex = -1;
		private int maxMapIndex = -1;

		private string[] resourceNames;
		private int minResourceIndex = -1;
		private int maxResourceIndex = -1;

		private string[] startResourceNames;
		private int minStartResourceIndex = -1;
		private int maxStartResourceIndex = -1;

		private void Start()
		{
			Button btn;
			tabController.ToggleButtons.Clear();
			tabController.ToggleContent.Clear();
			for (int i = 0; i < mapSettings.Length; i++)
			{
				if (i == 0)
					btn = buttonReference;
				else
					btn = Instantiate(buttonReference, buttonReference.transform.parent, false);
				int t = i;

				btn.onClick.AddListener(() => { tabController.SelectTab(t); });
				var text = btn.GetComponentInChildren<Text>();
				tabController.ToggleButtons.Add(btn);

				text.text = LocalizationManager.GetText(mapSettings[i].title);
			}
			tabController.OnToggleChanged -= TabSelected;
			tabController.OnToggleChanged += TabSelected;
			tabController.SelectTab(-1);
		}

		public void UpdateCitizenValue()
		{
			citizensValue.text = citizensSlider.value.ToString();
		}
		public void OnStartGame()
		{
			//Start game with:
			var setting = mapSettings[tabController.CurrentTab].Clone();

			var scenario = mapSettings[currentScenario];
			//Apply scecnario data from UI

			//Map Size
			if (scenario.userSettings.enableTerrainSizeSettings)
			{
				TerrainSettings.EMapSize mapSize;
				var option = mapSizeNames[mapSizeDropdown.SelectedElement + minMapIndex];

				if (Enum.TryParse(option, out mapSize))
				{
					setting.mapSize = mapSize;
				}
			}
			setting.terrainSettings.ApplyMapSize(setting.mapSize);

			//Map Resources
			if (scenario.userSettings.enableResourcesSettings)
			{
				GameMapSettings.EQuantitySetting resourceSetting;
				var option = resourceNames[resourcesDropdown.SelectedElement + minResourceIndex];
				if (Enum.TryParse(option, out resourceSetting))
				{
					var id = (int)resourceSetting - 1;
					id = Mathf.Clamp(id, 0, scenario.generationData.mapResourcesFactor.Length - 1);
					setting.resourceDensity = scenario.generationData.mapResourcesFactor[id];
				}
			}
			//Citizens
			setting.startingCitizens = Mathf.Clamp((int)citizensSlider.value, scenario.userSettings.minCitizens, scenario.userSettings.maxCitizens);


			//MStarting Resources
			if (scenario.userSettings.enableStartingResourcesSettings)
			{
				GameMapSettings.EQuantitySetting resourceSetting;
				GameGenerationData.ResourceGenerationEntry s;
				var option = startResourceNames[startingResourcesDropdown.SelectedElement + minStartResourceIndex];
				if (Enum.TryParse(option, out resourceSetting))
				{
					//var id = (int)resourceSetting - 1;
					//id = Mathf.Clamp(id, 0, scenario.generationData.startingResources.Length - 1);
					s = scenario.generationData.startingResources[0];

					//Add the next tier elements on top and use max values:
					for (int i = 1; i < scenario.generationData.startingResources.Length; i++)
					{
						var o = scenario.generationData.startingResources[i];

						s.waterFactor = Mathf.Max(s.waterFactor, o.waterFactor);
						s.waterMin = Mathf.Max(s.waterMin, o.waterMin);

						s.foodFactor = Mathf.Max(s.foodFactor, o.foodFactor);
						s.foodMin = Mathf.Max(s.foodMin, o.foodMin);
						foreach (var res in o.foodPool)
						{
							if (!s.foodPool.Contains(res))
								s.foodPool.Add(res);
						}

						s.materialFactor = Mathf.Max(s.materialFactor, o.materialFactor);
						s.materialMin = Mathf.Max(s.materialMin, o.materialMin);
						foreach (var res in o.materialPool)
						{
							if (!s.materialPool.Contains(res))
								s.materialPool.Add(res);
						}

						s.weaponsFactor = Mathf.Max(s.weaponsFactor, o.weaponsFactor);
						s.weaponsMin = Mathf.Max(s.weaponsMin, o.weaponsMin);
						foreach (var res in o.weaponsPool)
						{
							if (!s.weaponsPool.Contains(res))
								s.weaponsPool.Add(res);
						}
					}

					setting.startingResources = s;
				}
			}

			//Start Game
			mainMenuController.OnStartGame(setting);
		}

		void TabSelected(int id)
		{
			currentScenario = id;
			if (id < 0 || id >= mapSettings.Length)
			{
				mapSizeObject.SetActive(false);
				resourcesObject.SetActive(false);
				citizensObject.SetActive(false);
				description.text = "";
				settingsObject.SetActive(false);
				startGameButton.interactable = false;

				return;
			}
			else
			{
				startGameButton.interactable = true;
				var scenario = mapSettings[currentScenario];
				description.text = LocalizationManager.GetText(scenario.description);

				bool enableSettings = scenario.userSettings.enableTerrainSizeSettings || scenario.userSettings.enableResourcesSettings || scenario.userSettings.enableCitizenSettings;
				settingsObject.SetActive(enableSettings);

				mapSizeObject.SetActive(scenario.userSettings.enableTerrainSizeSettings);
				resourcesObject.SetActive(scenario.userSettings.enableResourcesSettings);
				citizensObject.SetActive(scenario.userSettings.enableCitizenSettings);

				if (scenario.userSettings.enableTerrainSizeSettings)
				{
					mapSizeNames = Enum.GetNames(typeof(TerrainSettings.EMapSize));
					minMapIndex = mapSizeNames.ToList().IndexOf(scenario.userSettings.minMapSize.ToString());
					maxMapIndex = mapSizeNames.ToList().IndexOf(scenario.userSettings.maxMapSize.ToString());
#if UNITY_EDITOR   //Smallest map by default for faster loading time
					minMapIndex = (int)TerrainSettings.EMapSize.Mini;
					int startIndex = 1;
#else                //In the build we use the predefined setting
					int startIndex = mapSizeNames.ToList().IndexOf(scenario.userSettings.defaultMapSize.ToString());
#endif
					mapSizeDropdown.Clear();
					for (int i = 0; i < mapSizeNames.Length; i++)
					{
						if (i < minMapIndex)
							continue;
						if (i > maxMapIndex)
							break;
						var option = LocalizationManager.GetText("#UI/MainMenu/Values/MapSize_" + i);
						if (i == startIndex)
							startIndex = mapSizeDropdown.Count;
						mapSizeDropdown.AddOption(option, true);
					}
					mapSizeDropdown.SelectValue(startIndex);
				}

				if (scenario.userSettings.enableResourcesSettings)
				{
					resourceNames = Enum.GetNames(typeof(GameMapSettings.EQuantitySetting));
#if UNITY_EDITOR    //The editor allows no resources  as well
					minResourceIndex = 0;
#else
					minResourceIndex = resourceNames.ToList().IndexOf(scenario.userSettings.minResourceSize.ToString());
#endif
					maxResourceIndex = resourceNames.ToList().IndexOf(scenario.userSettings.maxResourceSize.ToString());

					int startIndex = resourceNames.ToList().IndexOf(scenario.userSettings.defaultResourceSize.ToString());
					resourcesDropdown.Clear();
					for (int i = 0; i < resourceNames.Length; i++)
					{
						if (i < minResourceIndex)
							continue;
						if (i > maxResourceIndex)
							break;
						var option = LocalizationManager.GetText("#UI/MainMenu/Values/resources_" + i);
						if (i == startIndex)
							startIndex = resourcesDropdown.Count;
						resourcesDropdown.AddOption(option, true);
					}
					resourcesDropdown.SelectValue(startIndex);
				}
				if (scenario.userSettings.enableStartingResourcesSettings)
				{
					startResourceNames = Enum.GetNames(typeof(GameMapSettings.EQuantitySetting));
#if UNITY_EDITOR    //The editor allows no resources  as well
					minStartResourceIndex = 0;
#else
					minStartResourceIndex = startResourceNames.ToList().IndexOf(scenario.userSettings.minStartingResourceSize.ToString());
#endif
					maxStartResourceIndex = startResourceNames.ToList().IndexOf(scenario.userSettings.maxStartingResourceSize.ToString());

					int startIndex = startResourceNames.ToList().IndexOf(scenario.userSettings.defaultStartingResourceSize.ToString());

					startingResourcesDropdown.Clear();
					for (int i = 0; i < startResourceNames.Length; i++)
					{
						if (i < minStartResourceIndex)
							continue;
						if (i > maxStartResourceIndex)
							break;
						var option = LocalizationManager.GetText("#UI/MainMenu/Values/resources_" + i);
						if (i == startIndex)
							startIndex = startingResourcesDropdown.Count;
						startingResourcesDropdown.AddOption(option, true);
					}
					startingResourcesDropdown.SelectValue(startIndex);
				}
				if (scenario.userSettings.enableCitizenSettings)
				{
					citizensSlider.wholeNumbers = true;
					citizensSlider.maxValue = 9999;
					citizensSlider.minValue = scenario.userSettings.minCitizens;
					citizensSlider.maxValue = scenario.userSettings.maxCitizens;
					citizensSlider.value = scenario.userSettings.defaultCitizens;
					UpdateCitizenValue();
				}
			}
		}
	}
}