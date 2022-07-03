using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIProgressBar2Values : GUIProgressBar
	{
		[SerializeField] private float value2;
		[SerializeField] bool invert2;
		public float Value2
		{
			get { return value2; }
			set
			{
				this.value2 = Mathf.Clamp01(value);
				progressBar2.fillAmount = invert2 ? 1 - this.value2 : this.value2;
			}
		}
		[SerializeField] Image progressBar2;

		protected override void Awake()
		{
			base.Awake();
			Value2 = value2;
		}
	}
}