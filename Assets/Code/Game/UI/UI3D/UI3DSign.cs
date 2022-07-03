using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	//Todo: Improve pooling of icons
	public class UI3DSign : UI3DBase
	{
		[SerializeField] Image icon;
		public UITooltipText tooltip;
		public Action onClickCallback;

		public virtual void Setup(Vector3 pos, Sprite icon)
		{
			base.Setup(pos, 0, true);
			this.icon.sprite = icon;
		}

		public void OnClick()
		{
			onClickCallback?.Invoke();
		}

		public override void UpdateElement(Vector3 camPos)
		{
		}

		protected override void Dispose()
		{
		}

	}
}