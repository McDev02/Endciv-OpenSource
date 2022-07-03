using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TabControllerColor : TabController
	{
		[SerializeField] Color ActiveNormal;
		[SerializeField] Color ActiveHover;
		[SerializeField] Color InactiveNormal;
		[SerializeField] Color InactiveHover;

		public override void SelectTab(int id)
		{
			CurrentTab = id;
			for (int i = 0; i < ToggleButtons.Count; i++)
			{
				var btn = ToggleButtons[i];
				var cols = btn.colors;

				var content = i < ToggleContent.Count ? ToggleContent[i] : null;

				if (i == CurrentTab)
				{
					cols.normalColor = ActiveNormal;
					cols.highlightedColor = ActiveHover;
					cols.pressedColor = ActiveNormal;
					if (content != null) content.SetActive(true);
				}
				else
				{
					cols.normalColor = InactiveNormal;
					cols.highlightedColor = InactiveHover;
					cols.pressedColor = InactiveNormal;
					if (content != null) content.SetActive(false);
				}

				ToggleButtons[i].colors = cols;
			}

			if (OnToggleChanged != null) OnToggleChanged.Invoke(CurrentTab);
		}
	}
}