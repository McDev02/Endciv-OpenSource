using UnityEngine.UI;
using UnityEngine;

namespace Endciv
{
    public class MinSlider : Slider
    {
        public MaxSlider MaxSlider;
        public Text Indicator;
        public string NumberFormat;

        protected override void Set(float input, bool sendCallback)
        {
            if (MaxSlider == null)
            {
                MaxSlider = transform.parent.Find("MaxSlider").GetComponent<MaxSlider>();
            }

            float newValue = input;
            if (wholeNumbers)
            {
                newValue = Mathf.Round(newValue);
            }
            if (newValue >= MaxSlider.RealValue && MaxSlider.RealValue != MaxSlider.minValue)
            {
                return;
            }
            if (Indicator != null)
            {
                Indicator.text = newValue.ToString(NumberFormat);
            }
            base.Set(input, sendCallback);
        }

        public void Refresh(float input)
        {
            Set(input, false);
        }
    }
}
