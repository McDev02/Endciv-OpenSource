using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class UIDropdown : MonoBehaviour
	{
		[SerializeField] DropdownListElement DropdownElementPrefab;
		[SerializeField] GUICanvasGroup DropdownPanel;
		[SerializeField] RectTransform ElementContainer;
		public bool showActiveElement;

		[SerializeField] Text Label;
		List<bool> selectables = new List<bool>();
		List<DropdownListElement> elements = new List<DropdownListElement>();
		List<DropdownListElement> innactiveElements = new List<DropdownListElement>();
		List<DropdownListElement> elementPool = new List<DropdownListElement>();

		public int Count { get { return elements == null ? 0 : elements.Count; } }

		public int SelectedElement { get; private set; }
		public string SelectedOption
		{
			get
			{
				if (SelectedElement < 0 | elements == null || elements.Count <= SelectedElement)
					return string.Empty;
				if (elements[SelectedElement] == null)
					return string.Empty;
				var text = elements[SelectedElement].GetComponentInChildren<Text>();
				if (text == null)
					return string.Empty;
				return text.text;
			}
		}

		[SerializeField] Color BaseColorNormal;
		[SerializeField] Color BaseColorHighlighted;
		[SerializeField] Color ActiveColorNormal;
		[SerializeField] Color ActiveColorHighlighted;

		private void OnEnable()
		{
			DropdownPanel.OnHide();
		}

		public void Clear()
		{
			foreach (var element in elements)
			{
				element.gameObject.SetActive(false);
				if (!innactiveElements.Contains(element))
					innactiveElements.Add(element);
			}
			elements.Clear();
			selectables.Clear();
		}

		public void AddOption(string text, bool selectable = true)
		{
			DropdownListElement entry = null;
			if (innactiveElements.Count > 0)
			{
				entry = innactiveElements[0];
				innactiveElements.Remove(entry);
				entry.gameObject.SetActive(true);
			}
			else
			{
				entry = Instantiate(DropdownElementPrefab, ElementContainer, false);
			}
			entry.value = text;
			entry.Label.text = text;
			int id = elements.Count;
			entry.Button.onClick.RemoveAllListeners();
			entry.Button.onClick.AddListener(() => SelectValue(id));
			elements.Add(entry);
			selectables.Add(selectable);
		}

		public void SelectValue(int value)
		{
			SelectedElement = Mathf.Clamp(value, 0, elements.Count - 1);
			if (elements.Count > SelectedElement)
				Label.text = elements[SelectedElement].value;
			CloseDropdown();
		}

		void UpdateListEntries()
		{
			for (int i = 0; i < elements.Count; i++)
			{
				bool isactive = i == SelectedElement;
				if (!selectables[i])
					elements[i].gameObject.SetActive(false);
				else
				{
					elements[i].gameObject.SetActive(showActiveElement || !isactive);

					var cols = elements[i].Button.colors;
					if (isactive)
					{
						cols.highlightedColor = ActiveColorHighlighted;
						cols.normalColor = ActiveColorNormal;
						cols.pressedColor = ActiveColorNormal;
					}
					else
					{
						cols.highlightedColor = BaseColorHighlighted;
						cols.normalColor = BaseColorNormal;
						cols.pressedColor = BaseColorNormal;
					}
					elements[i].Button.colors = cols;
				}
			}
		}

		public void OpenDropdown()
		{
			UpdateListEntries();
			DropdownPanel.OnOpen();
		}

		public void CloseDropdown()
		{
			DropdownPanel.OnClose();
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var mousePos = Input.mousePosition;
				foreach (var btn in elements)
				{
					var rect = btn.GetComponent<RectTransform>();
					if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
					{
						return;
					}

				}
				if (DropdownPanel != null)
				{
					var scrollRect = DropdownPanel.GetComponent<ScrollRect>();
					if (scrollRect != null && scrollRect.verticalScrollbar != null)
					{
						var rect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
						if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
						{
							return;
						}
					}
				}

				CloseDropdown();
			}

		}
	}
}