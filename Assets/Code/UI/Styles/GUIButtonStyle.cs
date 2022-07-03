using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Endciv
{
	[CreateAssetMenu(fileName = "New ButtonStyle", menuName = "GUI Styles/Button Style", order = 1)]
	public class GUIButtonStyle : GUIStyle
	{
		public bool hasColorNormal = true;
		public Color colorNormal = Color.white;
		public bool hasColorHighlighted = true;
		public Color colorHighlighted = Color.white;
		public bool hasColorPressed = true;
		public Color colorPressed = Color.white;
		public bool hasColorDisabled = true;
		public Color colorDisabled = Color.white;


		public override void EnableAll()
		{
			hasColorNormal = true;
			hasColorHighlighted = true;
			hasColorPressed = true;
			hasColorDisabled = true;
		}
		public override void DisableAll()
		{
			hasColorNormal = false;
			hasColorHighlighted = false;
			hasColorPressed = false;
			hasColorDisabled = false;
		}
	}
}