using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TutorialController : MonoBehaviour
	{
		[SerializeField] Button showTutorialButton;
		List<GUICanvasGroup> Windows;
		int currentWindowID;

		private void Awake()
		{
			var children = transform.childCount;
			Windows = new List<GUICanvasGroup>(children);
			for (int i = 0; i < children; i++)
			{
				var window = transform.GetChild(i).GetComponent<GUICanvasGroup>();
				if (window == null) continue;
				window.OnClose();
				Windows.Add(window);
			}
			StartTutorial();
		}

		private void ShowPage(int id)
		{
			currentWindowID = id;
			Windows[currentWindowID].OnClose();
			currentWindowID = Mathf.Clamp(id, 0, Windows.Count - 1);
			Windows[currentWindowID].OnShow();
		}

		public void Next()
		{
			if (currentWindowID >= Windows.Count - 1)
			{
				EndTutorial();
				return;
			}
			Windows[currentWindowID].OnClose();
			currentWindowID = Mathf.Clamp(currentWindowID + 1, 0, Windows.Count - 1);
			Windows[currentWindowID].OnShow();
		}
		public void Previous()
		{
			Windows[currentWindowID].OnClose();
			currentWindowID = Mathf.Clamp(currentWindowID - 1, 0, Windows.Count - 1);
			Windows[currentWindowID].OnShow();
		}
		public void StartTutorial()
		{
			ShowPage(0);
			showTutorialButton.interactable = false;
		}
		public void EndTutorial()
		{
			Windows[currentWindowID].OnClose();
			showTutorialButton.interactable = true;
		}
	}
}