using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIUnitListEntry : MonoBehaviour
	{
		public Button button;
		[SerializeField] private  Text nameLbl;

		BaseEntity unit;

		public void Setup(BaseEntity unit)
		{
			this.unit = unit;
			UpdateValues();
		}

		public void UpdateValues()
		{
			if (unit == null)
			{
				gameObject.SetActive(false);
				return;
			}

			//We chache values to limit GC allocation
			nameLbl.text = $"{unit.GetFeature<EntityFeature>().EntityName}, {unit.GetFeature<LivingBeingFeature>().age.ToString()}";
		}
	}
}