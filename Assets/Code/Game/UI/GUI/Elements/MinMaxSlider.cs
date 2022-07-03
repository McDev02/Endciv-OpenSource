using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
    public class MinMaxSlider : MonoBehaviour
    {
        [Header("Common settings")]
        public int DecimalPlaces = 2;
        public float MinimumValue = 0;
        public float MaximumValue = 1;
        public bool UseWholeNumbers = false;

        [Header("Configuration for MinSlider")]
        public MinimumSlider MinSlider;

        [Header("Configuration for MaxSlider")]
        public MaximumSlider MaxSlider;

        // Properties
        public float CurrentLowerValue
        {
            get { return MinSlider.value; }
        }
        public float CurrentUpperValue
        {
            get{ return MaxSlider.RealValue; }
        }

        void Awake()
        {
            MinSlider.minValue = MinimumValue;
            MinSlider.maxValue = MaximumValue;
            MinSlider.wholeNumbers = UseWholeNumbers;

            MaxSlider.minValue = MinimumValue;
            MaxSlider.maxValue = MaximumValue;
            MaxSlider.wholeNumbers = UseWholeNumbers;
        }
    }
}