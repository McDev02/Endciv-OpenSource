using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Endciv
{
	public class UITooltipText : MonoBehaviour,IPointerTooltipHandler
	{
		public string text = null;

		public string GetText
		{
			get
			{
				return text;
			}
		}
	}
}