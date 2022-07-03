using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TooltipText : TooltipBase
	{
		[SerializeField] private Vector2 padding;
		[SerializeField] private int m_MaxWidth = 300;
		[SerializeField] private int m_MinHeight = 64;
		[SerializeField] private Text m_Text = null;

		public override void Setup(object obj)
		{
			base.Setup(obj);

			m_Text.text = (string)obj;

			var settings = m_Text.GetGenerationSettings(new Vector2(m_MaxWidth, 0));
			settings.horizontalOverflow = HorizontalWrapMode.Wrap;
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.updateBounds = true;

			m_Text.cachedTextGenerator.Populate((string)obj, settings);

			var size = m_Text.cachedTextGenerator.rectExtents.size;

			var pWidth = m_Text.preferredWidth;

			if (size.x > pWidth)
			{
				size.x = pWidth;
			}

			size = size - m_Text.rectTransform.sizeDelta + 2 * padding;
			size.y = Mathf.Max(size.y, m_MinHeight);
			//Make sure size is a multiple of 4
			size.x = CivMath.GetNextMultipleOf(size.x, 4);
			size.y = CivMath.GetNextMultipleOf(size.y, 4);
			rectTransform.sizeDelta = size;
		}
	}
}