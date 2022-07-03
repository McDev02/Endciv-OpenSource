using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class FarmlandFeatureSaveData : ISaveable
    {
        public EntitySaveData[,] cropModels;
        public string[,] assignedFarmerIDs;
        public string assignedWaterTransporterID;
        public List<List<SerVector2i>> CropGroupIDs;
        public int outputChamberID;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}