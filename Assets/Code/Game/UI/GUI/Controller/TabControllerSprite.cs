using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TabControllerSprite : TabController
	{
		[SerializeField] Sprite ActiveNormal;
		[SerializeField] Sprite ActiveHover;
		[SerializeField] Sprite InactiveNormal;
		[SerializeField] Sprite InactiveHover;
		
		public override void SelectTab(int id)
		{
			CurrentTab = id;
			for (int i = 0; i < ToggleButtons.Count; i++)
			{
				var btn = ToggleButtons[i];
				var sprites = btn.spriteState;
				var mainSprite = btn.targetGraphic as Image;

				var content = i < ToggleContent.Count ? ToggleContent[i] : null;

				if (i == CurrentTab)
				{
					mainSprite.sprite = ActiveNormal;
					sprites.highlightedSprite = ActiveHover;
					sprites.pressedSprite = ActiveNormal;
					if (content != null) content.SetActive(true);
				}
				else
				{
					mainSprite.sprite = InactiveNormal;
					sprites.highlightedSprite = InactiveHover;
					sprites.pressedSprite = InactiveNormal;
					if (content != null) content.SetActive(false);
				}

				ToggleButtons[i].spriteState = sprites;
			}
			if (OnToggleChanged != null) OnToggleChanged.Invoke(CurrentTab);
		}
	}
}