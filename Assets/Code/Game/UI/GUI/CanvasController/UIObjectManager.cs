using UnityEngine;
using System.Collections;
using System;

namespace Endciv
{
	public class UIObjectManager : MonoBehaviour
	{
		[SerializeField] TooltipText tooltipText;

		internal void HideTooltip()
		{
			tooltipText.gameObject.SetActive(false);
		}

		internal void ShowTooltip(IPointerTooltipHandler enter)
		{
			tooltipText.gameObject.SetActive(true);
			tooltipText.Setup(enter.GetText);
		}
	}
}