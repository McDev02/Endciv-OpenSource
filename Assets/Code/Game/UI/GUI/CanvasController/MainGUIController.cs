using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Endciv
{
	public class MainGUIController : MonoBehaviour
	{
		public class MessageWindowOptions
		{
			public string message;
			public bool buttonConfirm;
			public bool buttonDeny;
			public bool showLoading;
			public ListenerCallInteger callback;

			public MessageWindowOptions(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
			{
				this.message = message;
				this.buttonConfirm = buttonConfirm;
				this.buttonDeny = buttonDeny;
				this.showLoading = showLoading;
				this.callback = callback;
			}
		}

		Main main;
		[SerializeField] public GUIMessageWindow messageWindow;
		[SerializeField] public GUILoadSaveGamePanel loadSaveGameWindow;
		[SerializeField] public GUIOptionsWindow optionsWindow;
		[SerializeField] GUILoadingScreen loadingScreen;
		[SerializeField] GameObject FpsPanel;

		[SerializeField] public GUIFeedbackPanel feedbackWindow;
		public GUIDevConsole devConsole;

		public Action OnOptionsMenuClose;
		public Action OnLoadMenuClose;
		public Action OnSaveMenuClose;
		public Action OnQuitGamePanelClose;

		bool IsMessageWindowActive { get { return messageWindow.IsActive; } }

		public List<MessageWindowOptions> queuedMessages = new List<MessageWindowOptions>();

		public void ToggleFPS()
		{
			if (FpsPanel != null) FpsPanel.SetActive(!FpsPanel.activeSelf);
		}

		internal void OpenFeedbackPanel()
		{
			feedbackWindow.OnShow();
		}

		public void Setup(Main main)
		{
			this.main = main;
			optionsWindow.Setup(main);
			feedbackWindow.Setup(this);
		}

		protected void Start()
		{
			DontDestroyOnLoad(this);
			CloseAll();
			messageWindow.OnWindowClosed = PopNextQueue;
			messageWindow.OnClose();
		}

		private void Update()
		{
			if (main != null && main.gameInputManager.GetActionDown("OpenConsole"))
				devConsole.OnToggleActive();
		}

		public void ShowMessageWindow(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
		{
			if (IsMessageWindowActive)
				queuedMessages.Add(new MessageWindowOptions(message, buttonConfirm, buttonDeny, showLoading, callback));
			else
				messageWindow.OnOpen(message, buttonConfirm, buttonDeny, showLoading, callback);
		}

		public void UpdateMessageWindow(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
		{
			if (!IsMessageWindowActive)
			{
				ShowMessageWindow(message, buttonConfirm, buttonDeny, showLoading, callback);
			}
			else
			{
				messageWindow.Setup(message, buttonConfirm, buttonDeny, showLoading, callback);
			}
		}

		private void PopNextQueue()
		{
			StartCoroutine("DelayedPop");
		}

		private IEnumerator DelayedPop()
		{
			yield return new WaitForEndOfFrame();
			if (queuedMessages.Count > 0)
			{
				var msg = queuedMessages[0];
				messageWindow.OnOpen(msg.message, msg.buttonConfirm, msg.buttonDeny, msg.showLoading, msg.callback);
				queuedMessages.Remove(msg);
			}
		}

		public void OnLoadGameWindow(bool open)
		{
			if (open)
				loadSaveGameWindow.OnOpen(GUILoadSaveGamePanel.EMode.Load);
			else
			{
				loadSaveGameWindow.OnClose();
				if (OnLoadMenuClose != null) OnLoadMenuClose.Invoke();
			}
		}

		public void OnSaveGameWindow(bool open)
		{
			if (open)
				loadSaveGameWindow.OnOpen(GUILoadSaveGamePanel.EMode.Save);
			else
			{
				loadSaveGameWindow.OnClose();
				if (OnLoadMenuClose != null) OnLoadMenuClose.Invoke();
			}
		}

		public void OnFeedbackWindow(bool open)
		{
			if (open)
				feedbackWindow.OnOpen();
			else
				feedbackWindow.OnClose();
		}

		internal void CloseAll()
		{
			OnLoadGameWindow(false);
			OnOptionsWindow(false);
			devConsole.OnClose();
			if (FpsPanel != null) FpsPanel.SetActive(false);
		}

		public void OnOptionsWindow(bool open)
		{
			if (open)
				optionsWindow.OnOpen();
			else
			{
				optionsWindow.OnClose();
				if (OnOptionsMenuClose != null) OnOptionsMenuClose.Invoke();
			}
		}
		public void OnShowExitGameWindow(bool exitToWindows)
		{
			string message = exitToWindows ? "Quit game and go back to Desktop?" : "Quit game and go back to the Main Menu?";
			ShowMessageWindow(message, true, true, false, OnExitWindowClosed);
		}

		void OnExitWindowClosed(int id)
		{
			if (OnQuitGamePanelClose != null) OnQuitGamePanelClose.Invoke();
		}

		public void QuitGame()
		{
			Main.Instance.QuitGame();
		}

		public void ShowLoadingScreen(LoadingState state)
		{
			loadingScreen.gameObject.SetActive(true);
			loadingScreen.Setup(state);
		}

		public void HideLoadingScreen()
		{
			loadingScreen.Exit();
		}
	}
}