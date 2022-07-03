using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace Endciv
{
	public class UI3DResourcePulse : UI3DBase
	{
		const float StartHeight = 1.6f;
		const float StartScale = 0.01f;
		const float MaxScale = 0.015f;
		const float FadeTime = 0.2f;
		const float PulseSpeedFactor = 5f;

		public Image m_IconImage;
		private bool m_IsFading = false;

		private BaseEntity m_Owner;

		public void Setup(BaseEntity owner, Vector3 position, Sprite icon)
		{
			m_Owner = owner;
			transform.position = position + new Vector3(0, StartHeight, 0);
			m_IconImage.color = new Color(1f, 1f, 1f, 1f);
			m_IconImage.sprite = icon;
			base.Setup(null);
		}

        public override void UpdateElement(Vector3 camPos)
        {
			transform.localScale = Vector3.one * Mathf.Lerp(StartScale, MaxScale, (1f + Mathf.Sin(Time.time * PulseSpeedFactor)) / 2f);
			if (!m_IsFading)
			{
				//m_IconImage.color = new Color(1f, 1f, 1f, FogOfWar.Instance.IsCellVisible(m_Owner.GridIndex) ? 1f : 0f);
				m_IconImage.color = new Color(1f, 1f, 1f, 1f);
			}
		}

		public void FadeOut(float time = FadeTime)
		{
			if (m_IsFading)
				return;
			StartCoroutine(Fade(time));
			m_IsFading = true;
		}

		private IEnumerator Fade(float time)
		{
			//float rate = 1f / time;
			float delta = 0f;
			while (delta < 1f)
			{
				delta += Main.deltaTime;
				float alpha = Mathf.Lerp(1f, 0f, delta);
				//if (!FogOfWar.Instance.IsCellVisible(m_Owner.GridIndex))
				//    alpha = 0f;
				m_IconImage.color = new Color(1f, 1f, 1f, alpha);
				yield return null;
			}
			m_IsFading = false;
			Dispose();
		}

		protected override void Dispose()
		{
			UI3DFactory.Instance.Recycle(this);
		}
	}
}