using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIProgressBar : GUIInteractable
	{
		[SerializeField] private float value;
		public float Value
		{
			get { return value; }
			set
			{
				this.value = Mathf.Clamp01(value);
				progressBar.fillAmount = this.value;
			}
		}
		public Image progressBar;

	protected virtual void Awake()
		{
			Value = value;
		}
	}
}