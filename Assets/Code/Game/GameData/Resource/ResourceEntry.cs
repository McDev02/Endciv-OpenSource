using System;

namespace Endciv
{
    [Serializable]
    public class ResourceEntry
    {
        [StaticDataID("StaticData/SimpleEntities/Items")]
        public string id;
        public int min;
        public int max;
        
    }

}
