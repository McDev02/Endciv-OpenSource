using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class LocalizedButton : StylizedButton
	{
		public override void UpdateStyle()
		{
			base.UpdateStyle();

			for (int i = 0; i < styles.Count; i++)
			{
				var style = styles[i];
				if (style == null) continue;

				if (style is GUILocalizedButtonStyle)
				{
					var textstyle = style as GUILocalizedButtonStyle;
					var cols = colors;
					if (textstyle.hasColorNormal) cols.normalColor = textstyle.colorNormal;
					if (textstyle.hasColorHighlighted) cols.highlightedColor = textstyle.colorHighlighted;
					if (textstyle.hasColorPressed) cols.pressedColor = textstyle.colorPressed;
					if (textstyle.hasColorDisabled) cols.disabledColor = textstyle.colorDisabled;
					colors = cols;
				}
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
		}
	}
}