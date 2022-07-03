using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class GraveyardFeatureSaveData : ISaveable
    {
        public List<GraveSaveData> occupiedGraves;
        public int[] reservedPlotIDs;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}