﻿using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class GraveyardSystemSaveData : ISaveable
    {
        public List<string> workerUIDs;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}