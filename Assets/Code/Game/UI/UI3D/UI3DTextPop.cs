using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	public class UI3DTextPop : UI3DBase
	{
		const float FloatSpeed = 0.32f;
		const float FloatDuration = 3f;
		const float StartHeight = 1f;
		const float WobleDistance = 0.002f;
		const float MaxScale = 0.02f;
		float timer;
		BaseEntity m_Owner;

		public Text m_AmountLabel;

		private bool m_Woble;
		private bool m_scaleUp;

		public void Setup(BaseEntity owner, Vector3 position, string text, Color textColor, bool woble = false, bool scaleUp = false)
		{
			m_Owner = owner;
			transform.position = position + new Vector3(0, StartHeight, 0);
			timer = FloatDuration;
			m_AmountLabel.gameObject.SetActive(true);
			m_AmountLabel.text = text;
			m_AmountLabel.color = textColor;

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