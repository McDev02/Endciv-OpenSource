using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	[CreateAssetMenu(fileName = "New TextStyle", menuName = "GUI Styles/Text Style", order = 1)]
	public class GUITextStyle : GUIStyle
	{
		public bool hasFont;
		public Font font;
		public bool hasFontStyle;
		public FontStyle fontStyle;
		public bool hasFontSize;
		public int fontSize;
		public bool hasColor;
		public Color color = Color.white;

		public bool hasRichText;
		public bool richText;
		public bool hasRaycastTarget;
		public bool raycastTarget;



		public override void EnableAll()
		{
			hasFont = true;
			hasFontStyle = true;
			hasFontSize = true;
			hasColor = true;

			hasRichText = true;
			hasRaycastTarget = true;
		}
		public override void DisableAll()
		{
			hasFont = false;
			hasFontStyle = false;
			hasFontSize = false;
			hasColor = false;

			hasRichText = false;
			hasRaycastTarget = false;
		}
	}
}