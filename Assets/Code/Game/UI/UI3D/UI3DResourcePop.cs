using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class UI3DResourcePop : UI3DBase
	{
		const float FloatSpeed = 0.32f;
		const float FloatDuration = 3f;
		const float StartHeight = 1f;
		const float WobleDistance = 0.002f;
		const float MaxScale = 0.02f;
		private float timer;
		private BaseEntity m_Owner;

		public Image m_IconImage;
		public Text m_AmountLabel;

		private bool m_Woble;
		private bool m_scaleUp;

		public void Setup(BaseEntity owner, Vector3 position, ResourceStack item, bool woble = false, bool scaleUp = false)
		{
			m_Owner = owner;
			transform.position = position + new Vector3(0, StartHeight, 0);
			timer = FloatDuration;
			if (item.Amount > 1)
			{
				m_AmountLabel.gameObject.SetActive(true);
				m_AmountLabel.text = item.Amount.ToString();
			}
			else
				m_AmountLabel.gameObject.SetActive(false);

			m_IconImage.gameObject.SetActive(true);
			//m_IconImage.sprite = item.Icon;
			m_Woble = woble;
			m_scaleUp = scaleUp;
			base.Setup(null);
		}

		public void Setup(BaseEntity owner, Vector3 position, Sprite icon, bool woble = false, bool scaleUp = false)
		{
			m_Owner = owner;
			transform.position = position + new Vector3(0, StartHeight, 0);
			timer = FloatDuration;
			m_AmountLabel.gameObject.SetActive(false);

			m_IconImage.gameObject.SetActive(true);
			m_IconImage.sprite = icon;
			m_Woble = woble;
			m_scaleUp = scaleUp;
			base.Setup(null);
		}

        public override void UpdateElement(Vector3 camPos)
        {
			timer -= Time.deltaTime;
			float alpha = Mathf.InverseLerp(0f, FloatDuration, timer);
			//if (!FogOfWar.Instance.IsCellVisible(m_Owner.GridIndex))
			//{
			//    alpha = 0f;
			//}
			m_IconImage.color = new Color(1f, 1f, 1f, alpha);
			float x = 0f;
			if (m_Woble)
			{
				x = Mathf.Sin(Time.time) * WobleDistance;
			}
			transform.Translate(new Vector3(x, Main.deltaTimeSafe * FloatSpeed, 0));
			if (m_scaleUp)
			{
				transform.localScale = Vector3.one * Mathf.Lerp(0.01f, MaxScale, Mathf.InverseLerp(FloatDuration, 0f, timer));
			}
			if (timer <= 0)
			{
				Dispose();
			}
		}

		protected override void Dispose()
		{
			UI3DFactory.Instance.Recycle(this);
		}
	}
}