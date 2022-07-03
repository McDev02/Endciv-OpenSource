using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Endciv
{
	public abstract class TabController : MonoBehaviour
	{
		public List<Button> ToggleButtons = new List<Button>();
		public List<GameObject> ToggleContent = new List<GameObject>();

		public ListenerCallInteger OnToggleChanged;
		public bool AllowNoSelection = false;

		[SerializeField] protected int currentTab;
		public int CurrentTab { get { return currentTab; } set { currentTab = value; } }
		protected bool isReady;

		public abstract void SelectTab(int id);

		void Awake()
		{
			isReady = true;
			if (!AllowNoSelection) currentTab = Mathf.Clamp(currentTab, 0, ToggleButtons.Count);
			SelectTab(currentTab);
		}
	}
}