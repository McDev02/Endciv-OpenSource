using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public class AIGroupSystemSaveData : ISaveable
    {
		public EntitySaveData expedition;
        public List<string> workerIDs;
        public ISaveable CollectData()
        {
            return this;
        }
    }
}