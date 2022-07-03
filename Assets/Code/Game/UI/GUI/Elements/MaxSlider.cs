using UnityEngine.UI;
using UnityEngine;

namespace Endciv
{
    public class MaxSlider : Slider
    {
        public MinSlider MinSlider;
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
                MinSlider = transform.parent.Find("MinSlider").GetComponent<MinSlider>();
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
