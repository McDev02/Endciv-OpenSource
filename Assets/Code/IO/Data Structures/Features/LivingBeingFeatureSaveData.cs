using System;

namespace Endciv
{
    [Serializable]
    public class LivingBeingFeatureSaveData : ISaveable
    {
        public ELivingBeingGender gender;
        public ELivingBeingAge age;

        public float hunger;
        public float thirst;

        public float hungerConsumption;
        public float thirstConsumption;
        public float waterConsumed;
        public float nutritionConsumed;

        public bool isGettingBurried;

        public int ageDayCounter;
        public int childAge;
        public int adultAge;
        public int deathAge;        

        public ISaveable CollectData()
        {
            return this;
        }
    }
}