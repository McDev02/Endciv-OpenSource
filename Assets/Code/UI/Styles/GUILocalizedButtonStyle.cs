using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	[CreateAssetMenu(fileName = "New LocalizedButtonStyle", menuName = "GUI Styles/Localized Button Style", order = 1)]
	public class GUILocalizedButtonStyle : GUIButtonStyle
	{
		public bool hasTextColorNormal = true;
		public Color textColorNormal = Color.white;


		public override void EnableAll()
		{
			base.EnableAll();
			hasColorNormal = true;
			hasColorHighlighted = true;
			hasColorPressed = true;
			hasColorDisabled = true;
		}
		public override void DisableAll()
		{
			base.DisableAll();
			hasColorNormal = false;
			hasColorHighlighted = false;
			hasColorPressed = false;
			hasColorDisabled = false;
		}
	}
}