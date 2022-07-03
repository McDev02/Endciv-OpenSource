using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TabControllerSpriteText : TabController
	{
		[SerializeField] Sprite ActiveNormal;
		[SerializeField] Sprite ActiveHover;
		[SerializeField] Sprite InactiveNormal;
		[SerializeField] Sprite InactiveHover;

		[SerializeField] Color textColorActive;
		[SerializeField] Color textColorInactive;
		public override void SelectTab(int id)
		{
			CurrentTab = id;
			for (int i = 0; i < ToggleButtons.Count; i++)
			{
				var btn = ToggleButtons[i];
				var sprites = btn.spriteState;
				var mainSprite = btn.targetGraphic as Image;

				var content = i < ToggleContent.Count ? ToggleContent[i] : null;

				Graphic icon = null;
				var child = btn.transform.GetChild(0);
				if (child != null)
					icon = child.GetComponent<Graphic>();

				if (i == CurrentTab)
				{
					mainSprite.sprite = ActiveNormal;
					sprites.highlightedSprite = ActiveHover;
					sprites.pressedSprite = ActiveNormal;
					if (icon != null) icon.color = textColorActive;
					if (content != null) content.SetActive(true);
				}
				else
				{
					mainSprite.sprite = InactiveNormal;
					sprites.highlightedSprite = InactiveHover;
					sprites.pressedSprite = InactiveNormal;
					if (icon != null) icon.color = textColorInactive;
					if (content != null) content.SetActive(false);
				}

				ToggleButtons[i].spriteState = sprites;
			}
			if (OnToggleChanged != null) OnToggleChanged.Invoke(CurrentTab);
		}
	}
}