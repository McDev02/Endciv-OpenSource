using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

namespace Endciv
{
	public class Main : MonoBehaviour
	{
		public static Main Instance { get; private set; }

		public int CoreVersion;
		public int SubVersion;
		public string BuildVersion;
		public string BuildString { get { return $"{CoreVersion}.{SubVersion}{BuildVersion}"; } }

		public enum EScene { Main, MainGUI, Intro, MainMenu, Game }
		public EScene CurrentScene { get; private set; }
		private MainDebug MyDebug;

		public MainGUIController MainGUIController { get; private set; }
		public GameManager GameManager { get; private set; }
		public GameInputManager gameInputManager;

		public static float deltaTime;
		/// <summary>
		/// A delta time clamped to the value of 25FPS. In other words this value can never be higher than 0.04 to prevent big jumps in movement when the game lags. IN fact returns unscaledDeltaTimeSafe * Time.timeScale
		/// </summary>
		public static float deltaTimeSafe;
		public static float unscaledDeltaTime;
		/// <summary>
		/// An unscaled delta time clamped to the value of 25FPS. In other words this value can never be higher than 0.04 to prevent big jumps in movement when the game lags.
		/// </summary>
		public static float unscaledDeltaTimeSafe;
		const float FIXED_FRAME_DELTA_TIME = 1f / 25f;

		public SaveManager saveManager;
		public GraphicsManager graphicsManager;
		public AudioManager audioManager;
		public GeneralSettingsManager generalSettingsManager;
		public ResourceManager resourceManager;

#if UNITY_EDITOR
		public bool NoGameUpdate;
		public bool workOnly;
		public bool noSpareTime;
		public bool UnlockAllTech;
#else
		public bool NoGameUpdate = false;
		public bool workOnly = false;
		public bool noSpareTime = false;
		public bool UnlockAllTech = false;
#endif

		public StringBuilder unityLog;

		private LoadingState loadingState;

		private void Awake()
		{
			Instance = this;
			unityLog = new StringBuilder();
			Application.logMessageReceivedThreaded += UnityLogMessage;

			UpdateDeltaTime();

			ReadCommandLineArguments();

			DontDestroyOnLoad(this);
			MyDebug = GetComponent<MainDebug>();
			saveManager = new SaveManager();
			graphicsManager.Setup(this);
			audioManager.Setup(this);
			generalSettingsManager.Setup(this);
			resourceManager.Initialize();
			//if (saveManager.UserSettings.inputSettings != null)
			//{
			//	gameInputManager.ApplySaveData(saveManager.UserSettings.inputSettings);
			//}
			//No GPU over-usage, especially in main menu
			Application.targetFrameRate = 200;

			Debug.Log($"ECOS: Game Version: {BuildVersion} Savegame Version: {SaveFileConverter.currentSaveVersion}");
		}

		void UpdateDeltaTime()
		{
			deltaTime = Time.deltaTime;
			unscaledDeltaTime = Time.unscaledDeltaTime;
			unscaledDeltaTimeSafe = Mathf.Min(unscaledDeltaTime, FIXED_FRAME_DELTA_TIME);
			deltaTimeSafe = unscaledDeltaTimeSafe * Time.timeScale;
		}

		void UnityLogMessage(string logString, string stackTrace, LogType type)
		{
			if (type != LogType.Warning)
				unityLog.AppendLine(logString);
		}

		void ReadCommandLineArguments()
		{
			string[] args = System.Environment.GetCommandLineArgs();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"Parameters:");
			for (int i = 0; i < args.Length; i++)
			{
				builder.AppendLine(args[i]);
				if (args[i].Replace("-", "") == "nogameloop")
				{
					NoGameUpdate = true;
				}
			}

			builder.Append($"{(char)101}{(char)99}{(char)111}{(char)115}");			
			Debug.Log(builder.ToString());
		}

		private void OnDestroy()
		{
			//Reset time scale for the editor
			Time.timeScale = 1;
		}

		private IEnumerator Start()
		{
			UpdateDeltaTime();

			//Preload Shaders
			yield return SceneManager.LoadSceneAsync("WarmupShaders", LoadSceneMode.Additive);
			yield return null;

			//Load Main GUI
			yield return SceneManager.LoadSceneAsync("MainGUI", LoadSceneMode.Additive);
			yield return null;
			MainGUIController = FindObjectOfType<MainGUIController>();
			MainGUIController.Setup(this);

			//Next Scene
			if (MyDebug != null && MyDebug.SkipIntro)
				SwitchScene(EScene.MainMenu);
			else
				SwitchScene(EScene.Intro);
		}

		internal void QuitGame()
		{
			Debug.Log("Game esoc about to quit successfully");
			Application.logMessageReceivedThreaded -= UnityLogMessage;
			Application.Quit();
		}

		private void Update()
		{
			UpdateDeltaTime();

			//Temp
			if (gameInputManager.GetActionDown("QuickSave"))
			{
				saveManager.QuickSave();
				saveManager.LoadSaveGames();
			}
			if (gameInputManager.GetActionDown("ToggleFPSPanel"))
			{
				MainGUIController.ToggleFPS();
			}
		}

		public void SwitchScene(EScene scene, GameMapSettings gameMapSettings = null, bool loadSaveData = false)
		{
			if (CurrentScene == scene) return;
			StartCoroutine(LoadScene(scene, gameMapSettings, loadSaveData));
		}

		private IEnumerator LoadScene(EScene scene, GameMapSettings gameMapSettings = null, bool loadSaveData = false)
		{
			loadingState = new LoadingState();
			MainGUIController.ShowLoadingScreen(loadingState);
			yield return null;
			CurrentScene = scene;

			audioManager.SetState(AudioManager.EAmbientMode.Processing);
			if (scene == EScene.Game) yield return LoadGameScene(gameMapSettings, loadSaveData);
			else if (scene == EScene.MainMenu) yield return LoadMainMenuScene();
			else yield return SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
		}

		private IEnumerator LoadMainMenuScene()
		{
			loadingState.SetState(LoadingState.EState.LoadScene);

			yield return null;
			yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
			loadingState.SetState(LoadingState.EState.Done);

			audioManager.SetState(AudioManager.EAmbientMode.MainMenu);
			yield return null;
			MainGUIController.HideLoadingScreen();
		}

		private IEnumerator LoadGameScene(GameMapSettings gameMapSettings, bool loadSaveData = false)
		{
			loadingState.SetState(LoadingState.EState.LoadScene);

			yield return null;
			yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
			GameManager = FindObjectOfType<GameManager>();
			if (GameManager == null) Debug.LogError("GameManager could not be found!");
			yield return SceneManager.LoadSceneAsync("GameGUI", LoadSceneMode.Additive);
			SavegameDataBase save = null;
			loadingState.SetState(LoadingState.EState.MapGenerator);

			yield return null;
			if (loadSaveData)
			{
				save = saveManager.GetCurrentSaveGame();
				yield return GameManager.StartGame(save, loadingState);
			}
			else
			{
				yield return GameManager.StartGame(gameMapSettings, loadingState);
			}

			audioManager.SetState(AudioManager.EAmbientMode.Game);

			if (loadSaveData)
			{
				//Manual update of Statistics Loops so we don't have to wait for 1 game loop to get data
				GameManager.SystemsManager.StorageSystem.UpdateGameLoop();
				GameManager.SystemsManager.HousingSystem.UpdateGameLoop();
				//Diagnostics
				if (!GameStatistics.MainTownStatistics.Compare(save.townStatisticsData))
				{
					Debug.LogError("Town Statistics values mismatch.");
				}
				if (!GameStatistics.InventoryStatistics.Compare(save.inventoryStatisticsData))
				{
					Debug.LogError("Inventory Statistics values mismatch.");
				}
			}
			loadingState.SetState(LoadingState.EState.Done);

			yield return null;
			MainGUIController.HideLoadingScreen();
		}
	}
}