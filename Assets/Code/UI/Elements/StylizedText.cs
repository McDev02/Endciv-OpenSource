using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class StylizedText : Text, IGUIStyle
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
				raycastTarget = false;
			}

			if(!(this is LocalizedText))
			{
				Debug.LogError("I am still a StylizedText :(");
			}
		}
		public void UpdateStyle()
		{
			for (int i = 0; i < styles.Count; i++)
			{
				var style = styles[i];
				if (style == null) continue;

				if (style is GUITextStyle)
				{
					var textstyle = style as GUITextStyle;
					if (textstyle.hasFont) font = textstyle.font;
					if (textstyle.hasFontStyle) fontStyle = textstyle.fontStyle;
					if (textstyle.hasFontSize) fontSize = textstyle.fontSize;
					if (textstyle.hasColor) color = textstyle.color;
					if (textstyle.hasRichText) supportRichText = textstyle.richText;
					if (textstyle.raycastTarget) supportRichText = textstyle.raycastTarget;
				}
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
		}
	}
}