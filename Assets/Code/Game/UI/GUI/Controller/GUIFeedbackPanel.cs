using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIFeedbackPanel : GUIAnimatedPanel
	{
		const string addess = "https://endciv.de/feedback/";
		const string sendFile = "core/recievefb_j884n2urn8ur4vr93nut4d.php";
		const string sendAddress = addess + sendFile;

		bool isbound;
		bool isInputValid;

		[SerializeField] TabController typeSelector;
		[SerializeField] TabController moodSelector;

		[SerializeField] InputField mainInput;
		[SerializeField] InputField emailInput;

		[SerializeField] Toggle includeLogfile;
		[SerializeField] Toggle includeSavegames;
		[SerializeField] Toggle includeScreenshot;

		[SerializeField] Text systemInfoText;
		[SerializeField] Button sendButton;

		[SerializeField] GUICanvasGroup errorInfoPanel;
		[SerializeField] Text errorMessageLabel;
		[SerializeField] Text charactersLeft;
		string errorMessage;

		StringBuilder stringBuilder = new StringBuilder();
		MainGUIController mainGUIController;

		string hash = "te4SWMeocB7c4RJ3";
		string typ;
		string mood;
		string mail;
		string content;

		bool isSending;

		public void Mailto()
		{
			ValidateFeedback();

			string email = "support@endciv.de";
			string subject = MyEscapeURL("Endciv " + ((typeSelector.CurrentTab == 0) ? "Bug report" : "Feedback"));
			string body = "";
			if (string.IsNullOrEmpty(content))
			{
				if (typeSelector.CurrentTab == 0)
					body = MyEscapeURL("Hey folks\r\nI have an issue with my game:");
				else
					body = MyEscapeURL("Hey folks\r\nI just want to tell you something:");
			}
			else body = MyEscapeURL($"Hey folks\r\n{content}");
			body += MyEscapeURL("\r\n\r\nMy Specs:\r\n");
			body += MyEscapeURL(GetSystemInfo());
			Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
		}

		internal void Setup(MainGUIController mainGUIController)
		{
			this.mainGUIController = mainGUIController;
		}

		string MyEscapeURL(string url)
		{
			return WWW.EscapeURL(url).Replace("+", "%20");
		}
		private void OnTypeChanged(int id)
		{
			var placeholder = mainInput.placeholder as Text;
			if (placeholder != null)
			{
				if (id == 0)
					placeholder.text = LocalizationManager.GetText("#UI/General/Feedback/feedbackBlankText");
				else
					placeholder.text = LocalizationManager.GetText("#UI/General/Feedback/bugBlankText");
			}
			ValidateFeedback();
		}

		protected override void Awake()
		{
			base.Awake();
			ValidateFeedback();
			if (!isbound) typeSelector.OnToggleChanged += OnTypeChanged;
			isbound = true;
			Reset();
		}

		private void Reset()
		{
			emailInput.text = "";
			mainInput.text = "";
			moodSelector.SelectTab(1);
			typeSelector.SelectTab(-1);
			ValidateFeedback();
		}

		string ReadLogfile()
		{
			return Main.Instance.unityLog.ToString();
			/*
#if UNITY_STANDALONE_LINUX
			string pathfile="";
			return "Sent from Linux";
#else
			string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).ToString();
			 path = Path.Combine(path, "LocalLow", Application.companyName, Application.productName);
			string pathfile = Path.Combine(path, "output_log.txt");
#endif
			
			if (!Directory.Exists(path))
			{
				Debug.LogError("Logfile path found.");
				return null;
			}
			if (!File.Exists(pathfile))
			{
				Debug.LogError("Logfile not found.");
				return null;
			}
			try
			{
				StreamReader reader = new StreamReader(pathfile);
				string log = reader.ReadToEnd();
				reader.Close();
				return log;
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return null;
			}
			*/
		}

		public void ValidateFeedback()
		{
			errorMessage = "";
			isInputValid = true;

			typ = "bug";
			if (typeSelector.CurrentTab == 1) typ = "fed";
			mood = "bad";
			if (moodSelector.CurrentTab == 1) mood = "mid";
			if (moodSelector.CurrentTab == 2) mood = "god";
			mail = emailInput.text;
			content = mainInput.text;

			var charsLeft = mainInput.characterLimit - mainInput.text.Length;
			charactersLeft.text = $"{charsLeft} {LocalizationManager.GetText("#UI/General/Feedback/infoCharactersLeft")}";

			if (charsLeft <= 0)
			{
				errorMessage = LocalizationManager.GetText("#UI/General/Feedback/error_CharacterLimit");
				isInputValid = false;
			}
			if (mail.Length <= 0)//	if (!ValididateEmail(mail))
			{
				errorMessage = LocalizationManager.GetText("#UI/General/Feedback/error_ContactInfo");
				isInputValid = true;    //Optional
			}
			if (content.Length < 10)
			{
				errorMessage = LocalizationManager.GetText("#UI/General/Feedback/error_CharacterMin");
				isInputValid = false;
			}
			if (typeSelector.CurrentTab < 0)
			{
				errorMessage = LocalizationManager.GetText("#UI/General/Feedback/error_SelectType");
				isInputValid = false;
			}
			systemInfoText.text = GetSystemInfo();
			sendButton.interactable = isInputValid;

			if (isInputValid)
				errorInfoPanel.OnClose();
			else ShowErrorMessage(errorMessage);
		}

		bool ValididateEmail(string mail)
		{
			string[] split;
			if (!mail.Contains("@")) return false;
			split = mail.Split('@');
			if (split.Length > 2) return false;
			string name = split[0];
			string address = split[1];
			if (!address.Contains(".")) return false;
			split = mail.Split('.');
			if (split.Length > 2) return false;
			string domain = split[1];
			address = split[0];

			if (name.Length <= 0) return false;
			if (address.Length <= 0) return false;
			if (domain.Length <= 0) return false;

			return true;
		}

		public void OnSendFeedback()
		{
			if (!isSending)
				StartCoroutine(SendData());
		}

		string GetSystemInfo()
		{
			stringBuilder.Clear();
			//stringBuilder.AppendLine(SystemInfo.deviceUniqueIdentifier);
			stringBuilder.AppendLine($"OS: {SystemInfo.operatingSystem}");
			stringBuilder.AppendLine(SystemInfo.processorType);
			stringBuilder.AppendLine($"Threads: {SystemInfo.processorCount}");
			stringBuilder.AppendLine(SystemInfo.graphicsDeviceName);
			stringBuilder.AppendLine(SystemInfo.graphicsDeviceVersion);
			stringBuilder.AppendLine($"{SystemInfo.graphicsMemorySize.ToString()}MB");
			stringBuilder.AppendLine($"Max Texture Size: {SystemInfo.maxTextureSize.ToString()}");
			stringBuilder.AppendLine($"Memory: {SystemInfo.systemMemorySize}");
			if (Screen.resolutions.Length > 0)
			{
				var res = Screen.resolutions[Screen.resolutions.Length - 1];
				stringBuilder.AppendLine($"Screen: {res.width}x{res.height}@{res.refreshRate} {Screen.dpi}dpi");
			}
			return stringBuilder.ToString();
		}

		void ShowErrorMessage(string text)
		{
			errorMessageLabel.text = text;
			errorInfoPanel.OnShow();
		}

		IEnumerator SendData()
		{
			Debug.Log("Send feedback data");
			isSending = true;
			ValidateFeedback();

			WWWForm form = new WWWForm();
			form.AddField("typ", typ);
			form.AddField("mood", mood);
			form.AddField("mail", mail);
			form.AddField("content", content);
			string logfile = includeLogfile.isOn ? ReadLogfile() : null;
			form.AddField("logfile", logfile == null ? "" : logfile);
			Debug.Log("Logfile: " + logfile.Length);
			form.AddField("hash", hash);
			form.AddField("gamev", Main.Instance.BuildString);
			var systemInfo = GetSystemInfo();
			Debug.Log(systemInfo);
			form.AddField("system", systemInfo);
			form.AddField("usrkey", SystemInfo.deviceUniqueIdentifier);
			//formData.Add(new MultipartFormFileSection("my file data", "myfile.sav"));

			UnityWebRequest www = UnityWebRequest.Post(sendAddress, form);
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				mainGUIController.ShowMessageWindow($"ERROR: Something went wrong :(", true, false, false, null);
			}
			else
			{
				if (www.downloadHandler.text == "success")
					mainGUIController.ShowMessageWindow(LocalizationManager.GetText("#UI/General/Feedback/infoFeedbackSend"), true, false, false, OnMessageSuccess);
				else if (www.downloadHandler.text.Contains("ERROR:"))
					mainGUIController.ShowMessageWindow(www.downloadHandler.text, true, false, false, null);
				else
					mainGUIController.ShowMessageWindow(LocalizationManager.GetText("#UI/General/Feedback/infoFeedbackFailed"), true, false, false, null);
			}
			isSending = false;
			Debug.Log("Send feedback successful");
		}

		void OnMessageSuccess(int id)
		{
			OnClose();
		}
	}
}