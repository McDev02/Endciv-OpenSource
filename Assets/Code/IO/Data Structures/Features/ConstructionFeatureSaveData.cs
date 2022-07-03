using System;

namespace Endciv
{
    [Serializable]
    public class ConstructionFeatureSaveData : ISaveable
    {
        public int constructionState;
        public bool markedForDemolition;
        public float demolitionStartingProgress;
        public float currentConstructionPoints;
        public string[] workers;
        public string[] constructors;
        public string[] transporters;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}