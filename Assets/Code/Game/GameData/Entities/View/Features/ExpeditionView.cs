using UnityEngine;

namespace Endciv
{
	public sealed class ExpeditionView : FeatureView<ExpeditionFeature>
	{
		private UI3DSign uiSign;
		private Vector3 previousPosition; 

		public override void Setup(FeatureBase feature)
		{
			base.Setup(feature);
			uiSign = UI3DFactory.Instance.GetUI3DSign(transform.position, UI3DFactory.ESignIcon.Expedition);
			uiSign.tooltip.text = LocalizationManager.GetText("#UI/Game/Tooltip/Exploration/Gathering");
			previousPosition = transform.position;
		}

		public void SetTooltip(string text)
		{
			if (uiSign == null)
				return;
			uiSign.tooltip.text = text;
		}

		public string GetTooltip()
		{
			if (uiSign == null)
				return string.Empty;
			return uiSign.tooltip.text;
		}

		private void Update()
		{
			if (uiSign == null)
				return;
			if(transform.position != previousPosition)
			{
				previousPosition = transform.position;
				uiSign.Relocate(transform.position);
			}
		}

		public override void ShowHide(bool vissible)
		{
			base.ShowHide(vissible);
			if (uiSign == null)
				return;
			uiSign.gameObject.SetActive(vissible);
		}

		public override void UpdateView()
		{
			
		}				

		private void OnDestroy()
		{
			UI3DFactory.Instance.Recycle(uiSign);
		}
	}
}
