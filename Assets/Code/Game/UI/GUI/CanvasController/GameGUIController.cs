using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Endciv
{
	public class GameGUIController : MonoBehaviour, IRunLogic
	{
		Stopwatch watch;

		[SerializeField] GUICanvasGroup inGameMenuPanel;
		MainGUIController mainGUIController;
		public GameManager gameManager;
		AudioManager audioManager;
		GameInputManager inputManager;
		[SerializeField] GUICanvasGroup gameGUIPanel;

		//Controllers
		[SerializeField] public UI3DController ui3DController;
		[SerializeField] UiUserInput uiUserInput;
		//[SerializeField] TutorialController tutorialController;
		public Transform objectivesContainer;
		[SerializeField] ObjectiveWindow objectiveWindow;
		public ToolbarPanel toolbarPanel;

        public PlacementInfoUIController placementInfoUIController;
		public UserInfo userInfo;

		//On screen Panels
		[SerializeField] InteractionMenu interactionMenu;
		[SerializeField] CursorInfoPanel cursorInfoPanel;
		[SerializeField] TimeMenu timeMenu;
		public MilestonePanel milestonePanel;
		[SerializeField] public NotificationPanel notificationPanel;
		[SerializeField] TradingWindow tradingWindow;
		[SerializeField] MapLayerPanel mapLayerPanel;

		//Others
		[SerializeField] ContentPanel[] VariousPanels;
		List<GUICanvasGroup> inputHideWindows;
		[SerializeField] Transform featureInfoPanelRoot;
		private Dictionary<Type, BaseFeatureInfoPanel> featureInfoPanelLookup;
		BaseFeatureInfoPanel currentInfoPanel;
		[SerializeField] MouseCursorManager mouseCursorManager;
		[SerializeField] Image loadingIcon;
		public ImmigrantsOverviewPanel immigrantsOverviewPanel;
		[SerializeField] GUICanvasGroup gameOverWindow;

		public bool IsRunning { get; private set; }

		private bool showLoadingIcon;

		public SelectionRectController selectionRect;

        private InputField[] inputFields;

		protected void Start()
		{
			watch = new Stopwatch("GameGUIController");

			mainGUIController = Main.Instance.MainGUIController;
			audioManager = Main.Instance.audioManager;

			InitializeAllWindows();

			inGameMenuPanel.OnClose();
			mainGUIController.CloseAll();
            inputFields = Resources.FindObjectsOfTypeAll<InputField>();
			//tutorialController.gameObject.SetActive(true);
		}        

		internal void Setup(GameManager gameManager)
		{
			this.gameManager = gameManager;
			inputManager = gameManager.gameInputManager;

			selectionRect.gameInputManager = inputManager;

			SetupListeners();
			InputStopWindowChanged();
		}

        public bool IsTyping
        {
            get
            {
                if (inputFields == null && mainGUIController.devConsole == null)
                    return false;
                if(inputFields != null)
                {
                    for (int i = 0; i < inputFields.Length; i++)
                    {
                        if (inputFields[i].isFocused)
                            return true;
                    }
                }
                if(mainGUIController.devConsole != null)
                {
                    return mainGUIController.devConsole.IsFocused;
                }
                return false;
            }
        }

        public void OpenDiscord()
		{
			Application.OpenURL("https://discord.gg/zCb3XmS");
		}

		public void ShowUI()
		{
			gameGUIPanel.OnShow();
			mouseCursorManager.ShowCursor();
		}
		public void HideUI()
		{
			gameGUIPanel.OnHide();
			mouseCursorManager.HideCursor();
		}
		public void ToggleUI()
		{
			if (gameGUIPanel.IsVisible) HideUI();
			else ShowUI();
		}

		internal void Run()
		{
			IsRunning = true;
			for (int i = 0; i < VariousPanels.Length; i++)
			{
				if (VariousPanels[i] == null)
					Debug.LogError("Unassigned panel. Clear up");
				else
					VariousPanels[i].Run();
			}

            //Setup - we do this here to prevent systems from bein not ready
            placementInfoUIController.Setup();
			cursorInfoPanel.Setup(gameManager.gameInputManager, gameManager.GridMap);
			milestonePanel.Setup(gameManager.SystemsManager.NotificationSystem, audioManager);
			notificationPanel.Setup(gameManager.SystemsManager.NotificationSystem, audioManager);
			uiUserInput.Setup(this, gameManager);
			interactionMenu.Setup(gameManager);
			timeMenu.Setup( gameManager.timeManager, gameManager.SystemsManager.WeatherSystem, gameManager.SystemsManager.AIAgentSystem.CitizenAISystem);
			toolbarPanel.Setup(mainGUIController, this, gameManager.SystemsManager.ConstructionSystem);
			mapLayerPanel.Setup(this, gameManager);
			userInfo.Run(gameManager.CameraController, inputManager);

			//Run
			cursorInfoPanel.Run();
			ui3DController.Run(gameManager.gameInputManager, gameManager.CameraController.Camera);
			timeMenu.Run();
			toolbarPanel.Run();

			interactionMenu.Run();

#if UNITY_EDITOR || DEV_MODE
			cursorInfoPanel.OnShow();
#else
			cursorInfoPanel.OnClose();
#endif
			StartCoroutine(UpdatePanels());
		}

		void SetupListeners()
		{
			inputHideWindows = new List<GUICanvasGroup>();
			inputHideWindows.Add(inGameMenuPanel);
			inputHideWindows.Add(mainGUIController.feedbackWindow);
			inputHideWindows.Add(mainGUIController.loadSaveGameWindow);
			inputHideWindows.Add(mainGUIController.messageWindow);
			inputHideWindows.Add(mainGUIController.devConsole);

			//Panels
			inputHideWindows.Add(toolbarPanel.productionWindow);
			inputHideWindows.Add(tradingWindow);
			inputHideWindows.Add(gameOverWindow);

			tradingWindow.OnWindowClosed -= OnHideTradingWindow;
			tradingWindow.OnWindowClosed += OnHideTradingWindow;

			for (int i = 0; i < inputHideWindows.Count; i++)
			{
				inputHideWindows[i].OnWindowOpened += InputStopWindowChanged;
				inputHideWindows[i].OnWindowClosed += InputStopWindowChanged;
			}
		}

		void InputStopWindowChanged()
		{
			bool isInputActive = true;

			for (int i = 0; i < inputHideWindows.Count; i++)
			{
				if (inputHideWindows[i].IsVisible)
				{
					isInputActive = false; continue;
				}
			}
			//Enable or disable user tools, Selection tool by default. We Reset other tools used previously
			if (inputManager.gameInputShouldBeAllowed != isInputActive)
				gameManager.UserToolSystem.SwitchState(isInputActive ? UserToolSystem.EToolState.Selection : UserToolSystem.EToolState.None);

			inputManager.gameInputShouldBeAllowed = isInputActive;
		}

		void InitializeAllWindows()
		{
			var childWindows = FindAllPanels(false);
			for (int i = 0; i < childWindows.Count; i++)
			{
				if (!childWindows[i].gameObject.activeSelf)
					childWindows[i].gameObject.SetActive(true);
			}
			featureInfoPanelLookup = new Dictionary<Type, BaseFeatureInfoPanel>();
			var panels = featureInfoPanelRoot.GetComponentsInChildren<BaseFeatureInfoPanel>(true);
			foreach (var panel in panels)
			{
				var t = panel.GetType();
				if (!featureInfoPanelLookup.ContainsKey(t))
				{
					featureInfoPanelLookup.Add(t, panel);
				}

			}
		}

		public void OpenFeedbackPanel()
		{
			mainGUIController.OpenFeedbackPanel();
		}

		List<GUICanvasGroup> FindAllPanels(bool includeSubPanels)
		{
			List<GUICanvasGroup> panels = new List<GUICanvasGroup>();
			Stack<Transform> searchers = new Stack<Transform>();
			for (int i = 0; i < transform.childCount; i++)
			{
				searchers.Push(transform.GetChild(i));
			}
			int endlessquit = 1000;
			while (endlessquit > 0 && searchers.Count > 0)
			{
				endlessquit--;
				var sercher = searchers.Pop();
				var panel = sercher.GetComponent<GUICanvasGroup>();
				if (panel != null)
					panels.Add(panel);
				if (includeSubPanels || panel == null)
				{
					for (int i = 0; i < sercher.childCount; i++)
					{
						searchers.Push(sercher.GetChild(i));
					}
				}
			}
			if (endlessquit <= 0) Debug.LogError("Loop quit before finished, remaining: " + searchers.Count);
			Debug.Log("Panels found: " + panels.Count);
			return panels;
		}

		protected void OnDestroy()
		{
			inputManager.gameInputShouldBeAllowed = true;

			for (int i = 0; i < inputHideWindows.Count; i++)
			{
				if (inputHideWindows[i] == null) continue;
				inputHideWindows[i].OnWindowOpened -= InputStopWindowChanged;
				inputHideWindows[i].OnWindowClosed -= InputStopWindowChanged;
			}
			inputHideWindows.Clear();
		}

		public void SetLoadingIcon(bool val)
		{
			showLoadingIcon = val;
		}

		private void Update()
		{
			if (!IsRunning) return;
			var color = loadingIcon.color;
			if (showLoadingIcon)
			{
				if (color.a < 1f)
				{
					color.a += Time.unscaledDeltaTime;
				}
			}
			else
			{
				if (color.a > 0f)
				{
					color.a -= Time.unscaledDeltaTime;
				}
			}
			loadingIcon.color = color;
			if (loadingIcon.color.a > 0f)
			{
				loadingIcon.transform.Rotate(new Vector3(0f, 0f, -1f * Time.deltaTime * 150f));
			}
			if (inputManager.GetActionDown("ToggleGUI") && !IsTyping)
				ToggleUI();

			if (Input.GetKeyDown(KeyCode.F10))
			{
				cursorInfoPanel.OnToggleActive();
			}
			if (inputManager.GetActionDown("ToggleIngameMenu"))       //ActionKey.ToggleIngameMenu
			{
				// if (gameManager.UserToolSystem.CurrentState > UserToolSystem.EToolState.Selection)
				//     gameManager.UserToolSystem.SwitchState(UserToolSystem.EToolState.Selection);
				// else
				OnShowInGameMenu(true);
			}

			if (cursorInfoPanel.IsRunning) cursorInfoPanel.UpdateData();

			ui3DController.UpdateElements();

		}

		IEnumerator UpdatePanels()
		{
			while (IsRunning)
			{
#if _LogTime
				watch.Reset();
				watch.Start();
				Debug.Log("UpdatePanels() -----------------------------------------");
#endif
				for (int i = 0; i < VariousPanels.Length; i++)
				{
					var panel = VariousPanels[i];
					if (panel != null && panel.IsActive && panel.IsRunning)
					{
#if _LogTime
						watch.Reset();
						watch.Start();
#endif
						panel.UpdateData();
#if _LogTime
						watch.LogRound($"Panel ({i})");
#endif
						yield return null;
					}
				}

				if (toolbarPanel.productionWindow.IsRunning)
				{
#if _LogTime
					watch.Reset();
					watch.Start();
#endif
					toolbarPanel.productionWindow.UpdateData();
#if _LogTime
					watch.LogRound("ResourcesManagementPanel");
#endif
					yield return null;
				}
				yield return null;
				if (currentInfoPanel != null)
				{
#if _LogTime
					watch.Reset();
					watch.Start();
#endif
					currentInfoPanel.UpdateData();
#if _LogTime
					watch.LogRound("CurrentInfoPanel");
#endif
					yield return null;
				}
				if (toolbarPanel.citizenOverviewPanel != null)
				{
#if _LogTime
					watch.Reset();
					watch.Start();
#endif
					toolbarPanel.citizenOverviewPanel.UpdateData();
#if _LogTime
					watch.LogRound("CitizenOverviewPanel");
#endif
					yield return null;
				}
				if (immigrantsOverviewPanel != null)
				{
#if _LogTime
					watch.Reset();
					watch.Start();
#endif
					immigrantsOverviewPanel.UpdateData();
#if _LogTime
					watch.LogRound("ImmigrantsOverviewPanel");
#endif
					yield return null;
				}
			}
		}

		public void OnShowInGameMenu(bool open)
		{
			if (open == inGameMenuPanel.IsActive) return;
			if (open)
			{
				gameManager.PauseGame();
				inGameMenuPanel.OnOpen();
			}
			else
			{
				gameManager.UnpauseGame();
				inGameMenuPanel.OnClose();
			}

			gameManager.UserToolSystem.SwitchState(open ? UserToolSystem.EToolState.None : UserToolSystem.EToolState.Selection);
		}

		private T GetInfoPanel<T>() where T : BaseFeatureInfoPanel
		{
			if (featureInfoPanelLookup.ContainsKey(typeof(T)))
			{
				return (T)featureInfoPanelLookup[typeof(T)];
			}
			else
				return null;
		}

		public void OnShowSelectedEntityInfo(BaseEntity entity)
		{
			currentInfoPanel = null;
			//Main Features

			if (entity.HasFeature<ConstructionFeature>() &&
				entity.GetFeature<ConstructionFeature>().ConstructionState != ConstructionSystem.EConstructionState.Ready)
			{
				currentInfoPanel = GetInfoPanel<ConstructionSiteInfoPanel>();
			}
			else if (entity.HasFeature<ImmigrantAIAgentFeature>())
			{
				currentInfoPanel = GetInfoPanel<ImmigrantGroupInfoPanel>();
			}
			else if (entity.HasFeature<CitizenAIAgentFeature>())
			{
				currentInfoPanel = GetInfoPanel<CitizenFeatureInfoPanel>();
			}
			else if (entity.HasFeature<TraderAIAgentFeature>())
			{
				currentInfoPanel = GetInfoPanel<TraderAIAgentFeatureInfoPanel>();
			}
			else if (entity.HasFeature<ResourcePileFeature>())
			{
				if (entity.GetFeature<ResourcePileFeature>().ResourcePileType == ResourcePileSystem.EResourcePileType.ResourcePile)
					currentInfoPanel = GetInfoPanel<ResourcePileInfoPanel>();
				else
					currentInfoPanel = GetInfoPanel<InventoryFeatureInfoPanel>();
			}
			else if (entity.HasFeature<ProductionFeature>())
			{
				currentInfoPanel = GetInfoPanel<ProductionFeatureInfoPanel>();
			}
			else if (entity.HasFeature<FarmlandFeature>())
			{
				currentInfoPanel = GetInfoPanel<FarmlandFeatureInfoPanel>();
			}
			else if (entity.HasFeature<PastureFeature>())
			{
				currentInfoPanel = GetInfoPanel<PastureFeatureInfoPanel>();
			}
			else if (entity.HasFeature<HousingFeature>())
			{
				currentInfoPanel = GetInfoPanel<HousingFeatureInfoPanel>();
			}

			//Fallback features
			else if (entity.HasFeature<InventoryFeature>())
			{
				currentInfoPanel = GetInfoPanel<InventoryFeatureInfoPanel>();
			}
			if (currentInfoPanel != null)
			{
				currentInfoPanel.OnShow();
				currentInfoPanel.Setup(this, entity);
			}

		}

		public void ShowObjectiveWindow(string[] pages, string title)
		{
			objectiveWindow.Setup(pages, title);
			objectiveWindow.OnOpen();
		}

		public void OnShowGameOverWindow()
		{
			gameManager.PauseGame(true);
			gameOverWindow.OnOpen();
		}
		public void OnCloseGameOverWindow()
		{
			gameManager.UnpauseGame(true);
			gameOverWindow.OnClose();
		}

		void OnHideTradingWindow()
		{
			gameManager.UnpauseGame(true);
			tradingWindow.OnClose();
		}

		public void ShowTradingWindow(TraderAIAgentFeature trader)
		{
			gameManager.PauseGame(true);
			tradingWindow.OnOpen();
			tradingWindow.Setup(gameManager.GridMap,
				gameManager.SystemsManager.StorageSystem,
				trader,
				gameManager.SystemsManager.AgricultureSystem,
				gameManager.SystemsManager.PastureSystem);
		}
		public void ShowProductionWindow()
		{
		}

		public void ShowImmigrationPanel()
		{
			immigrantsOverviewPanel.Setup();
			immigrantsOverviewPanel.OnOpen();
		}

		public void LocateEntity(BaseEntity entity)
		{
			gameManager.CameraController.FollowEntity(entity);
		}

		public void OnDeselectEntity()
		{
			gameManager.UserToolSystem.SelectionTool.ResetSelection();
		}

		public void OnCloseEntityInfo()
		{
			if (currentInfoPanel != null)
				currentInfoPanel.OnClose();
			currentInfoPanel = null;
		}

		public void OnShowOptionsMenu()
		{
			mainGUIController.OnOptionsWindow(true);
			mouseCursorManager.ShowCursor();
		}

		public void OnShowLoadGame()
		{
			if (gameManager.UnsavedGameChanges) { Debug.LogWarning("Implement unsaved game check"); }
			mainGUIController.OnLoadGameWindow(true);
			mouseCursorManager.ShowCursor();
		}
		public void OnShowSaveGame()
		{
			mainGUIController.OnSaveGameWindow(true);
			mouseCursorManager.ShowCursor();
		}

		public void OnBackToMainMenu()
		{
			if (gameManager.UnsavedGameChanges) { Debug.LogWarning("Implement unsaved game check"); }
			Main.Instance.SwitchScene(Main.EScene.MainMenu);
			mouseCursorManager.ShowCursor();
		}
		public void OnQuitGame()
		{
			if (gameManager.UnsavedGameChanges) { Debug.LogWarning("Implement unsaved game check"); }
			mainGUIController.OnShowExitGameWindow(true);
			Main.Instance.QuitGame();
			mouseCursorManager.ShowCursor();
		}

		internal void CloseAll()
		{
			for (int i = 0; i < VariousPanels.Length; i++)
			{
				VariousPanels[i].OnClose();
			}
		}
	}
}