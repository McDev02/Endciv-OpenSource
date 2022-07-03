using System;

namespace Endciv
{
    [Serializable]
    public class ResourcePileCollectionActionSaveData : ActionSaveData, ISaveable
    {
        public float collectionProgress;
    }
}