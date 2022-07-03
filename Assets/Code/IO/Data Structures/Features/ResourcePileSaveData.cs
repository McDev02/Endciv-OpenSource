using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class ResourcePileSaveData : ISaveable
    {
        public bool markedAsGathering;
        public bool canCancelGathering;
        public float collectionProgress;
        public float fullResources;
        public string overlappingConstructionSiteID;
        public string assignedGathererID;
        public int startResources;
        public List<ResourceSaveData> resources;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}