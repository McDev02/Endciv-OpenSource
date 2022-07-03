using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIConsumableRationListEntry : MonoBehaviour
	{
		public Text title;
		public Image toggleCheckmark;
		public Sprite toggleOn;
		public Sprite toggleOff;
		public bool isAllowed;

		public string ItemID { get; private set; }

		public void UpdateValues(string itemID, bool isAllowed)
		{
			ItemID = itemID;
			title.text = LocalizationManager.GetResourceName(itemID);
			this.isAllowed = isAllowed;
		}

		public void OnToggleAllowance()
		{
			isAllowed = !isAllowed;
			UpdateUI();
		}

		public void UpdateUI()
		{
			toggleCheckmark.sprite = isAllowed ? toggleOn : toggleOff;
		}
	}
}