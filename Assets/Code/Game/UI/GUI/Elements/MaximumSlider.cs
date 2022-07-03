using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
    public class MaximumSlider : Slider
    {
        public MinimumSlider MinSlider;
        public Text Indicator;
        public string NumberFormat;

        public float RealValue;
        private bool assignedRealValue = false;

        protected override void Start()
        {
            RealValue = maxValue;
            base.Start();
        }

        protected override void Set(float input, bool sendCallback)
        {
            if (MinSlider == null)
            {
                MinSlider = transform.parent.Find("MinSlider").GetComponent<MinimumSlider>();
            }
            if (!assignedRealValue)
            {
                RealValue = maxValue;
                assignedRealValue = true;
            }
            else
            {
                RealValue = maxValue - input + minValue;
            }

            if (wholeNumbers)
            {
                RealValue = Mathf.Round(RealValue);
            }
            if (RealValue <= MinSlider.value)
            {
                // invalid value
                return;
            }
            if (Indicator != null)
            {
                Indicator.text = RealValue.ToString(NumberFormat);
            }
            base.Set(input, sendCallback);
        }

        public void Refresh(float input)
        {
            Set(input, false);
        }
    }
}