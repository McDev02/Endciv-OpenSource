using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Endciv
{
	public class PastureCattleListEntry : MonoBehaviour
	{
		public Text label;
		public Text amount;
		public Button button;
		public Image checkmark;

		public Sprite spriteOn;
		public Sprite spriteOff;

		public string cattleID;

		private bool allowCattle;
		public bool AllowCattle
		{
			get { return allowCattle; }
			set
			{
				allowCattle = value;
				checkmark.sprite = enabled ? spriteOn : spriteOff;
			}
		}

		public void TogglePolicy()
		{
			AllowCattle = !AllowCattle;
		}
	}
}