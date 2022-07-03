using UnityEngine;
using System.Collections;

namespace Endciv
{
	public class LanguageSelectionWindow : GUIAnimatedPanel
	{
		public void SelectLanguage(string lang)
		{
			var tmp = Main.Instance.generalSettingsManager.TmpSettings;
			tmp.language = lang;
			Main.Instance.generalSettingsManager.ApplyTemporaryValues();

			OnClose();
		}
	}
}