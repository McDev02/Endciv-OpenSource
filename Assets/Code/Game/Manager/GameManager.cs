using System;
using System.Collections;
using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// Master Class for a Game Session. Is created when a game is started and destroyed when the game is quit.
	/// There can only be one game at once.
	/// </summary>
	public class GameManager : MonoBehaviour, IRunLogic
	{
		//public static GameManager Instance { get; private set; } Lets ty without

		LoadingState loadingState;

		public GameConfig gameConfig;
		public GameHelper gameHelper;
		public TimeManager timeManager;
		public GameMechanicSettings gameMechanicSettings;
		[SerializeField] CameraController m_CameraController;
		public CameraController CameraController { get { return m_CameraController; } }

		public UserToolSystem UserToolSystem { get; private set; }
		[SerializeField] UserToolsView userToolsView;

		public GameInputManager gameInputManager { get { return Main.Instance.gameInputManager; } }
		[NonSerialized] public GameGUIController GameGUIController;

		public GridRectController gridRectController;

		[SerializeField] PathfindingManager PathfindingManager;
		public SystemsManager SystemsManager { get; private set; }
		public FactoryManager Factories;

		[SerializeField] TerrainManager terrainManager;
		public TerrainManager TerrainManager { get { return terrainManager; } }

		[SerializeField] WorldView worldView;

		[SerializeField] GridMap gridMap;
		public GridMap GridMap { get { return gridMap; } }

		[SerializeField] LevelGenerator levelGenerator;

		[SerializeField] GameMapSettingsReference gameMapSettingsReference;

		/// <summary>
		/// Do not save game if there are no changes. This is only false while the game is paused and was saved, when it continues it becomes true.
		/// </summary>
		public bool UnsavedGameChanges = true;

		public Action OnGameRun;

		public bool IsRunning { get; private set; }

		public void PauseGame(bool forceUntilRelief = false)
		{
			timeManager.PauseGame(forceUntilRelief);
		}
		public void UnpauseGame(bool forceUntilRelief = false)
		{
			timeManager.UnpauseGame(forceUntilRelief);
		}
		public GameMapSettings gameMapSettings { get; set; }

		//Load Saved game
		public IEnumerator StartGame(SavegameDataBase save, LoadingState loadingState)
		{
			Main.Instance.saveManager.LoadGameMapSettings(save, gameMapSettingsReference);
			GridMap.SetSize(gameMapSettings.terrainSettings.FullMapSize, gameMapSettings.terrainSettings.FullMapSize);
			Initialize(gameMapSettings);
			yield return null;
			//Load terrain
			GridMap.CreateEmpty(this, gameMapSettings.terrainSettings.FullMapSize, gameMapSettings.terrainSettings.FullMapSize);
			GridMap.Data.ApplySaveData(save.gridMapSaveData.mapData);

			loadingState.SetMessage("Convert Terrain Data");
			terrainManager.factory.ConvertData(save.gridMapSaveData.terrainExchangeData, false);
			loadingState.SetMessage("Create terrain");
			yield return terrainManager.factory.CreateTerrain(gameMapSettings.terrainSettings, loadingState);

			TerrainManager.GenerateView();

			loadingState.SetMessage("Initialize after map creation");
			yield return null;
			InitializeAfterMapCreation();
			ObjectReferenceManager.Initialize();
			//Load units
			loadingState.SetMessage("Load Units");
			yield return Main.Instance.saveManager.LoadUnits(save, loadingState);
			//Load resource piles
			loadingState.SetMessage("Load ResourcePiles");
			yield return null;
			yield return Main.Instance.saveManager.LoadResourcePiles(save, loadingState);
			//Load structures
			loadingState.SetMessage("Load Structures");
			yield return Main.Instance.saveManager.LoadStructures(save, loadingState);
			//Load Systems
			loadingState.SetMessage("Load Systems");
			yield return null;
			Main.Instance.saveManager.LoadSystems(save, loadingState);
			//Load AI
			loadingState.SetMessage("Load AI");
			yield return Main.Instance.saveManager.LoadAI(save, loadingState);
			OnFinalizeGameLoad();
			//Two frames required before inventory statistics are populated            
			yield return null;
			//Apply loaded settings
			Main.Instance.graphicsManager.ApplyTemporaryValues(false, false);
			Main.Instance.audioManager.ApplyTemporaryValues(false, false);
			Main.Instance.generalSettingsManager.ApplyTemporaryValues(false, false);
		}

		//Generate new map
		public IEnumerator StartGame(GameMapSettings gameMapSettings, LoadingState loadingState)
		{
			this.gameMapSettings = gameMapSettings;
			GridMap.SetSize(gameMapSettings.terrainSettings.FullMapSize, gameMapSettings.terrainSettings.FullMapSize);
			Initialize(gameMapSettings);
			yield return null;
			//Create terrain
			GridMap.CreateEmpty(this, gameMapSettings.terrainSettings.FullMapSize, gameMapSettings.terrainSettings.FullMapSize);
			//Generate map
			var generator = terrainManager.terrainGenerator;
			yield return generator.Generate(gameMapSettings.terrainSettings, loadingState);

			loadingState.SetMessage("Convert Terrain Data");
			terrainManager.factory.ConvertData(generator.GetExchangeData(), false);
			loadingState.SetMessage("CreateTerrain");
			yield return terrainManager.factory.CreateTerrain(gameMapSettings.terrainSettings, loadingState);

			TerrainManager.GenerateView();

			loadingState.SetMessage("Initialize after map creation");
			yield return null;
			InitializeAfterMapCreation();

			//Distribute resources
			yield return levelGenerator.DistributeResources(gameMapSettings, loadingState);
			if (gameMapSettings.GeneratePlayerCity)
				yield return levelGenerator.GeneratePlayerCity();
			//Populate with units  
			yield return levelGenerator.SpawnStartingCitizen(gameMapSettings, loadingState);
			//Create Starting resources
			yield return levelGenerator.GenerateStartingResources(gameMapSettings, loadingState);
			OnFinalizeGameLoad();
			yield return null;
			Main.Instance.graphicsManager.ApplyTemporaryValues(false, false);
			Main.Instance.audioManager.ApplyTemporaryValues(false, false);
			Main.Instance.generalSettingsManager.ApplyTemporaryValues(false, false);
		}

		//Not good, makeshift for initialization timing
		private void InitializeAfterMapCreation()
		{
			SystemsManager.InitializeAfterMapCreation();
		}

		private void Initialize(GameMapSettings gameMapSettings)
		{
			StaticDataIO.Instance.Run();

			//Functionality
			GameGUIController = FindObjectOfType<GameGUIController>();
			var worldData = gameMapSettings.worldData;

			timeManager = new TimeManager(this, worldData);
			gameHelper = new GameHelper(this);
			Main.Instance.generalSettingsManager.RegisterAutoSaveCallback();
			SystemsManager = new SystemsManager(this, timeManager);
			Factories.Setup(this);
			SystemsManager.Setup(gameConfig, worldData);
			UserToolSystem = new UserToolSystem(this, userToolsView, Factories, gameInputManager);
			GameGUIController.Setup(this);
			levelGenerator.Setup(this, gridMap, Factories, UserToolSystem);
			TerrainManager.Setup(this, GridMap);
		}

		public void OnFinalizeGameLoad()
		{
			PathfindingManager.Setup(this);
			CameraController.SetBounds(new Rect(0, 0, gameMapSettings.terrainSettings.FullMapSize * GridMapView.TileSize, gameMapSettings.terrainSettings.FullMapSize * GridMapView.TileSize));
			CameraController.Setup(gameInputManager);

			//Run Game Logic
			gameInputManager.SetupGameMode(this, TerrainManager, CameraController);
			gridMap.Run();
			PathfindingManager.Run();
			SystemsManager.Run();
			CameraController.Run();

			OnGameRun?.Invoke();
			gridRectController.Run(SystemsManager.WeatherSystem);
			worldView.Run(timeManager, SystemsManager.WeatherSystem);
			UserToolSystem.Run();

			//Run UI last
			GameGUIController.Run();
			//Notification System requires UI running before initializing
			SystemsManager.NotificationSystem.Run();
			timeManager.SetTimeSpeed(TimeManager.EGameSpeed._1x);

			IsRunning = true;
		}

		bool hasSeenGameOver = false;
		//Main Game Update Loop
		private void Update()
		{
			if (!IsRunning) return;
			UserToolSystem.Process();

			//Temp input
			if (gameInputManager.IsGameInputAllowed)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._1x);
				else if (Input.GetKeyDown(KeyCode.Alpha2))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._3x);
				else if (Input.GetKeyDown(KeyCode.Alpha3))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._5x);
				else if (Input.GetKeyDown(KeyCode.Alpha4))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._10x);
#if UNITY_EDITOR || DEV_MODE
				else if (Input.GetKeyDown(KeyCode.Alpha5))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._20x);
				else if (Input.GetKeyDown(KeyCode.Alpha6))
					timeManager.SetTimeSpeed(TimeManager.EGameSpeed._50x);
#endif
				else if (gameInputManager.GetActionDown("PauseGame"))
					timeManager.TogglePauseGame();
			}

			//Move this somewhere else?
			if (!hasSeenGameOver && SystemsManager.UnitSystem.Citizens[SystemsManager.MainPlayerFaction].Count <= 0)
			{
				hasSeenGameOver = true;
				Debug.Log("Game over!");
				GameGUIController.OnShowGameOverWindow();
			}
			else if (SystemsManager.UnitSystem.Citizens[SystemsManager.MainPlayerFaction].Count > 0)
			{
				hasSeenGameOver = false;
			}
		}

		/// <summary>
		/// Properly dispose of static singletons
		/// </summary>
		private void OnDestroy()
		{
			gameInputManager.Stop();
		}
	}
}