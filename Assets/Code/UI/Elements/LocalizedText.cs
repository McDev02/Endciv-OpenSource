using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class LocalizedText : StylizedText, IGUIStyle
	{
		[LocaId]
		public string locaID;
		public LocalizationManager.ETextStyle textStyle;

		protected override void Awake()
		{
			base.Awake();
			UpdateText();

			LocalizationManager.OnLanguageChanges -= UpdateText;
			LocalizationManager.OnLanguageChanges += UpdateText;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			LocalizationManager.OnLanguageChanges -= UpdateText;
		}

		public void SetLocaKey(string key)
		{
			locaID = key;
			UpdateText();
		}

		public void UpdateText()
		{
			string output;
			if (LocalizationManager.GetTextSafely(locaID, out output, textStyle))
			{
				text = output;
			}
		}
	}
}