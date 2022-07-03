using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class UI3DConstruction : UI3DBase
	{
		[SerializeField] Transform content;
		const int maxDistance = 120;
		const int minDistance = 60;

		[SerializeField] Sprite spriteConstructionIcon;
		[SerializeField] Sprite spriteDemolitionIcon;
		[SerializeField] Image spriteIcon;

		[SerializeField] Image progressResources;
		[SerializeField] Image progressConstruction;
		ConstructionFeature site;

		public void Setup(ConstructionFeature site, float heightOffset,bool demolition, bool stationary = false)
		{
			this.site = site;
			base.Setup(site.Entity.GetFeature<EntityFeature>().View.transform, heightOffset, stationary);

			spriteIcon.sprite = demolition ? spriteDemolitionIcon : spriteConstructionIcon;
		}
		public void Setup(ConstructionFeature site, bool demolition, bool stationary = false)
		{
			this.site = site;
			base.Setup(site.Entity.GetFeature<EntityFeature>().View.transform, 0,stationary);

			spriteIcon.sprite = demolition ? spriteDemolitionIcon : spriteConstructionIcon;
		}

		public void SetMode(bool demolition)
		{
			spriteIcon.sprite = demolition ? spriteDemolitionIcon : spriteConstructionIcon;
		}
		
		public override void UpdateElement(Vector3 camPos)
		{
			float dist = (transform.position - camPos).magnitude;
			if (dist <= maxDistance)
			{
				content.gameObject.SetActive(true);
				dist = CivMath.SqrtFast(Mathf.Clamp01((dist - minDistance) * (1f / (maxDistance - minDistance))));
				content.transform.localScale = Vector3.one * (1 - dist * dist);
			}
			else
			{
				content.gameObject.SetActive(false);
			}

			if (site != null)
			{
				progressResources.fillAmount = site.ResourceProgress;
				progressConstruction.fillAmount = site.ConstructionProgress;
			}
		}


		protected override void Dispose()
		{
			UI3DFactory.Instance.Recycle(this);
		}
	}
}