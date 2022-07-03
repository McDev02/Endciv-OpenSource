using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace Endciv
{
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] GUICanvasGroup MainMenuPanel;
		[SerializeField] GUICanvasGroup ScenariosPanel;
		MainGUIController MainGUIController;

		[SerializeField] LanguageSelectionWindow languageSelectionWindow;

		[SerializeField] Text gameVersion;

		protected void Start()
		{
			MainGUIController = Main.Instance.MainGUIController;

			gameVersion.text = $"{LocalizationManager.GetText("#UI/MainMenu/GameVersion")} {Main.Instance.BuildString}";

			CloseAll();
			MainGUIController.CloseAll();
			OnShowMainMenuPanel();

			MainGUIController.OnLoadMenuClose += OnShowMainMenuPanel;
			MainGUIController.OnOptionsMenuClose += OnShowMainMenuPanel;
			MainGUIController.OnQuitGamePanelClose += OnShowMainMenuPanel;

			if (!Main.Instance.generalSettingsManager.userHasSelectedLanguage)
				languageSelectionWindow.OnOpen();
			else
				languageSelectionWindow.OnClose();
		}

		protected void OnDestroy()
		{
			MainGUIController.OnLoadMenuClose -= OnShowMainMenuPanel;
			MainGUIController.OnOptionsMenuClose -= OnShowMainMenuPanel;
			MainGUIController.OnQuitGamePanelClose -= OnShowMainMenuPanel;
		}

		public void OpenDiscord()
		{
			Application.OpenURL("https://discord.gg/zCb3XmS");
		}
		public void OpenFacebook()
		{
			Application.OpenURL("https://www.facebook.com/endcivgame/");
		}

		public void OnStartGame(GameMapSettings setting)
		{
			MainMenuPanel.OnClose();
			Main.Instance.SwitchScene(Main.EScene.Game, setting);
		}

		public void OnShowMainMenuPanel()
		{
			CloseAll();
			MainMenuPanel.OnShow();
		}

		public void OnShowScenariosPanel()
		{
			CloseAll();
			ScenariosPanel.OnOpen();
		}

		public void OpenFeedbackPanel()
		{
			MainGUIController.OpenFeedbackPanel();
		}

		public void OnShowLoadGameWindow()
		{
			MainMenuPanel.OnClose();
			MainGUIController.OnLoadGameWindow(true);
		}
		public void OnShowOptionsWindow()
		{
			MainMenuPanel.OnClose();
			MainGUIController.OnOptionsWindow(true);

		}
		public void OnShowExitGameWindow()
		{
			Main.Instance.QuitGame();
			//MainMenuPanel.OnClose();
			//MainGUIController.OnExitGameWindow(true,true);
		}

		internal void CloseAll()
		{
			MainMenuPanel.OnClose();
			ScenariosPanel.OnClose();
		}
	}
}