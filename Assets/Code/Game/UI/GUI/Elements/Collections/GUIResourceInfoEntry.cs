using System;
using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class GUIResourceInfoEntry : MonoBehaviour
	{
		public Image icon;
		public Text amount;
		public EIconSize iconSize;
		[SerializeField] UITooltip tooltip;

		internal void Setup(Sprite icon, int amount, string resID, bool isWater = false, bool useTooltip = true)
		{
			this.icon.sprite = icon;
			if (isWater)
				this.amount.text = CivHelper.GetWaterString(amount);
			else
				this.amount.text = (amount * GameConfig.WaterPortion).ToString("0.##");

			this.amount.text = amount.ToString();
			if (useTooltip)
			{
				tooltip.ChangeLocaID($"{LocalizationManager.ResourcePath}{resID}/name");
			}
		}
		internal void UpdateValues(int amount)
		{
			this.amount.text = amount.ToString();
		}
	}
}