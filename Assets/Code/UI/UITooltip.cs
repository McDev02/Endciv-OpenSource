using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

namespace Endciv
{
	public class UITooltip : MonoBehaviour, IPointerTooltipHandler
	{
		[SerializeField]
		[LocaId]
		private string m_TooltipLocaId = null;

		public void ChangeLocaID(string newid)
		{
			if (m_TooltipLocaId == newid) return;
			m_TooltipLocaId = newid;
		}

		public string GetText
		{
			get
			{
				return LocalizationManager.GetText(m_TooltipLocaId);
			}
		}
	}
}