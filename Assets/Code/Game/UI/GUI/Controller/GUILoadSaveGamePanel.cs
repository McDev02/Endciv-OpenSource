using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Endciv
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUILoadSaveGamePanel : GUIAnimatedPanel
	{
		public enum EMode { Load, Save }
		EMode currentMode;

		[SerializeField] MainGUIController MainController;

		[SerializeField] RectTransform SavegameCollection;
		[SerializeField] GUISavegameListEntry SavegameListEntryPrefab;

		[SerializeField] RectTransform LoadGameContent;
		[SerializeField] RectTransform SaveGameContent;

		[SerializeField] Text windowTitle;
		[SerializeField] Text invalidSavegameNotice;
		[SerializeField] GameObject NoSavegamesInfo;

		[SerializeField] Text CurrentSavegameName;
		[SerializeField] Text CurrentSavegameDate;
		[SerializeField] Button LoadGameButton;
		[SerializeField] Button SaveGameButton;
		[SerializeField] InputField SavegameNameField;
		[SerializeField] Text SavegameNameErrorMessage;
		string savegameName;

		private List<GUISavegameListEntry> files = new List<GUISavegameListEntry>();
		private Stack<GUISavegameListEntry> filePool = new Stack<GUISavegameListEntry>();
		private int currentSelection = 0;

		private List<char> invalidChars = new List<char>();

		private void Start()
		{
			invalidChars.Clear();
			invalidChars = System.IO.Path.GetInvalidFileNameChars().ToList();
			OnSavegameFieldChanged();
			Main.Instance.saveManager.OnSaveGameComplete -= OnSaveComplete;
			Main.Instance.saveManager.OnSaveGameComplete += OnSaveComplete;
			files.Clear();
			files.AddRange(SavegameCollection.GetComponentsInChildren<GUISavegameListEntry>());
			RefreshList();
		}

		private void OnSaveComplete()
		{
			RefreshList(true);
		}

		public void OnOpen(EMode mode)
		{
			OnOpen();
			currentMode = mode;

			windowTitle.text = LocalizationManager.GetText($"#UI/Game/Windows/InGameMenu/{mode.ToString()}_Title");

			LoadGameContent.gameObject.SetActive(mode == EMode.Load);
			SaveGameContent.gameObject.SetActive(mode == EMode.Save);
		}
		public void OnShow(EMode mode)
		{
			OnOpen(mode);
		}

		public void RefreshList(bool reloadFiles = false)
		{
			currentSelection = 0;
			if (reloadFiles)
				Main.Instance.saveManager.LoadSaveGames();
			//Todo: maybe pool that?
			var count = SavegameCollection.childCount;
			for (int i = files.Count - 1; i >= 0; i--)
			{
				RemoveFilePrefab(files[i]);
			}

			files.Clear();
			//Find savegames and populate list
			var games = Main.Instance.saveManager.SaveGames.Values.ToList();
			//Load them in reverse order, from newest to oldest
			for (int i = games.Count - 1; i >= 0; i--)
			{
				var game = games[i];
				var file = GetFilePrefab();

				file.Title.text = game.name;
				file.Date.text = game.date;
				file.deprecated.gameObject.SetActive(game.IsDeprecated);
				file.ID = i;
				//Required to avoid boxing - leave "j" as is!!!                
				int j = i;
				file.GetComponent<Button>().onClick.RemoveAllListeners();
				file.GetComponent<Button>().onClick.AddListener(() => { OnSavegameButtonPressed(j); });
				files.Add(file);
			}

			NoSavegamesInfo.SetActive(games.Count == 0);

			UpdateSelectedFile();

			UpdateInvalidFiles(Main.Instance.saveManager.InvalidSavegames);
		}

		public GUISavegameListEntry GetFilePrefab()
		{
			GUISavegameListEntry filePrefab = null;
			while (filePrefab == null && filePool.Count > 0)
			{
				filePrefab = filePool.Pop();
			}
			if (filePrefab == null)
			{
				filePrefab = Instantiate(SavegameListEntryPrefab, SavegameCollection);
			}
			filePrefab.gameObject.SetActive(true);
			return filePrefab;
		}

		public void RemoveFilePrefab(GUISavegameListEntry filePrefab)
		{
			if (!files.Contains(filePrefab))
				return;
			files.Remove(filePrefab);
			filePrefab.GetComponent<Button>().onClick.RemoveAllListeners();
			filePrefab.gameObject.SetActive(false);
			if (!filePool.Contains(filePrefab))
				filePool.Push(filePrefab);
		}

		public void OnLoadGame()
		{
			Main.Instance.SwitchScene(Main.EScene.Game, null, true);
		}

		public void OnSaveGame()
		{
			//Show override warning
			bool overrideFile = SavegameFileExists(savegameName);
			if (overrideFile)
			{
				string message = "Savegame with same name exists already. Override?";
				MainController.ShowMessageWindow(message, true, true, false, OnOverrideConfirmationPressed);
			}
			else
			{
				StartCoroutine(SaveCoroutine(savegameName));
			}
		}

		//Save name sanitation and save button toggle
		public void OnSavegameFieldChanged()
		{
			savegameName = SavegameNameField.text;
			string message = string.Empty;

			//Check for save name length (less than 3 characters long)
			if (string.IsNullOrEmpty(savegameName) || savegameName.Length < 3)
			{
				message = "Save name too short";
			}
			else
			{
				//Check for special characters
				foreach (char c in savegameName)
				{
					if (invalidChars.Contains(c))
					{
						message = "Do not include special characters";
						break;
					}
				}
			}

			//Display proper message
			SavegameNameErrorMessage.text = message;

			//Toggle save button based on save name sanitation            
			SaveGameButton.interactable = message == string.Empty;

		}

		private void OnOverrideConfirmationPressed(int id)
		{
			if (id == 0)
			{
				StartCoroutine(SaveCoroutine(savegameName));
			}
			else
			{
				//Do nothing
			}
		}

		private IEnumerator SaveCoroutine(string savegameName)
		{
			MainController.messageWindow.OnCloseNow();
			yield return null;
			MainController.ShowMessageWindow("Save in progress...", false, false, true, null);
			yield return new WaitForSecondsRealtime(3f);
			Main.Instance.saveManager.SaveGame(savegameName);
			yield return new WaitForSecondsRealtime(2f);
			MainController.UpdateMessageWindow("Game saved successfully.", true, false, false, OnSuccessConfirmation);
		}

		private void OnSuccessConfirmation(int id)
		{
			RefreshList(true);            
		}

		private void OnSavegameButtonPressed(int id)
		{
			//Inverted to reference the proper save file in the actual order
			currentSelection = files.Count - 1 - id;

			//Select current
			for (int i = 0; i < files.Count; i++)
			{
				var btn = files[i].GetComponent<Button>();
				btn.interactable = i != currentSelection;
			}

			UpdateSelectedFile();
			if (currentMode == EMode.Save)
			{
				var game = Main.Instance.saveManager.GetSaveByID(id);
				SavegameNameField.text = game.name;
			}
		}

		public void OnDeleteGame()
		{
			int id = files.Count - 1 - currentSelection;
			bool success = Main.Instance.saveManager.DeleteSaveByID(id);
			if (success)
			{
				MainController.ShowMessageWindow("Save deleted successfully.", true, false, false, OnSuccessConfirmation);
			}
			else
			{
				MainController.ShowMessageWindow("Save could not be deleted.", true, false, false, null);
			}
		}

		bool SavegameFileExists(string name)
		{
			return Main.Instance.saveManager.SaveGames.Values.FirstOrDefault(x => x.name == name) != null;
		}

		private void Update()
		{
			if (currentMode == EMode.Load)
			{
				if (currentSelection < 0 || currentSelection >= files.Count)
				{
					LoadGameButton.interactable = false;
				}
				else
				{
					int id = files.Count - 1 - currentSelection;
					var game = Main.Instance.saveManager.GetSaveByID(id);
					LoadGameButton.interactable = !game.IsDeprecated;
				}
			}
		}

		void UpdateInvalidFiles(int invalidFiles)
		{
			if (invalidFiles > 0)
			{
				invalidSavegameNotice.gameObject.SetActive(true);
				invalidSavegameNotice.text = $"{LocalizationManager.GetText("#UI/Game/Windows/Savegames/InvalidFiles")} {invalidFiles}";
			}
			else
				invalidSavegameNotice.gameObject.SetActive(false);
		}

		public void CleanupInvalidFiles()
		{
			Main.Instance.saveManager.CleanupInvalidFiles();
			RefreshList(true);
		}

		void UpdateSelectedFile()
		{
			if (currentSelection < 0)
				return;
			if (currentSelection >= files.Count)
				return;
			Main.Instance.saveManager.SetSaveGame(currentSelection);
			CurrentSavegameDate.text = files[currentSelection].Date.text;
			CurrentSavegameName.text = files[currentSelection].Title.text;
		}

		private void OnDestroy()
		{
			Main.Instance.saveManager.OnSaveGameComplete -= OnSaveComplete;
		}
	}
}