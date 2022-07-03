using System.ComponentModel;
using UnityEngine;

namespace Endciv
{
    public class EntityProperty : INotifyPropertyChanged
    {
        private float value;
        public float Value
        {
            get { return value; }
            set
            {
                if (value != this.value)
                {
                    this.value = Mathf.Min(maxValue, value);
                    Progress = maxValue <= 0 ? 0 : CivMath.Clamp01(value * oneByMaxValue);
                    NotifyPropertyChanged();
                }
            }
        }
        public float maxValue;
        public float oneByMaxValue;

        public float Progress
        {
            get; private set;
        }

        public EntityProperty(float maxValue)
        {
            this.maxValue = maxValue;
            oneByMaxValue = 1f / maxValue;
            Value = this.maxValue / 2f;
        }

        public EntityProperty(float maxValue, float startingValue)
        {
            this.maxValue = maxValue;
            oneByMaxValue = 1f / maxValue;
            Value = startingValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}