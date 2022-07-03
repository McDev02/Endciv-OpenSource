using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class StylizedButton : Button, IGUIStyle
	{
		[SerializeField] protected List<GUIStyle> styles = new List<GUIStyle>(1);
		public List<GUIStyle> Styles { get { return styles; } set { styles = value; } }

		[SerializeField] private bool initialized;
		protected override void Awake()
		{
			//Default values
			if (!initialized)
			{
				initialized = true;
				var nav = navigation;
				nav.mode = Navigation.Mode.None;
				navigation = nav;
			}
		}
		public virtual void UpdateStyle()
		{
			for (int i = 0; i < styles.Count; i++)
			{
				var style = styles[i];
				if (style == null) continue;

				if (style is GUIButtonStyle)
				{
					var textstyle = style as GUIButtonStyle;
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