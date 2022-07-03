using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class WasteSystemSaveData : ISaveable
    {
        public List<string> workerUIDs;
        public List<Vector2i> tilesGathered;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}