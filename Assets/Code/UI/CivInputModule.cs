using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endciv
{
	public class CivInputModule : StandaloneInputModule
	{
		[SerializeField] UIObjectManager uiObjectManager;

		private IPointerTooltipHandler lastPointer;
		private float m_Timer = -2f;

		[SerializeField] private float m_TooltipDelay = 0.5f;

		public override void Process()
		{
			base.Process();

			var pointerEvent = GetLastPointerEventData(PointerInputModule.kMouseLeftId);
			GameObject enter = pointerEvent.pointerCurrentRaycast.gameObject;
			IPointerTooltipHandler pointer = null;
			if (enter != null)
				pointer = enter.GetComponentInParent<IPointerTooltipHandler>();
			HandlePointerTooltip(pointerEvent, pointer);
		}

		private void HandlePointerTooltip(PointerEventData pointerData, IPointerTooltipHandler pointer)
		{
			if (lastPointer != pointer)
			{
				if (lastPointer != null && m_Timer == -1f)
				{
					// send tooltip exit event
					uiObjectManager.HideTooltip();
				}

				if (pointer != null)
				{
					// mouse enter target changed, reset timer
					m_Timer = 0;
				}
				else
				{
					m_Timer = -2f;
				}

				lastPointer = pointer;
			}

			if (pointerData.dragging)
				uiObjectManager.HideTooltip();
			else if (pointer != null && m_Timer >= 0)
			{
				m_Timer += Time.unscaledDeltaTime;
				if (m_Timer > m_TooltipDelay)
				{
					m_Timer = -1f;

					// send tooltip enter event
					uiObjectManager.ShowTooltip(pointer);
				}
			}
			else if (pointer == null)
				uiObjectManager.HideTooltip();
		}
	}

	public interface IPointerTooltipHandler : IEventSystemHandler
	{
		string GetText { get; }
	}
}