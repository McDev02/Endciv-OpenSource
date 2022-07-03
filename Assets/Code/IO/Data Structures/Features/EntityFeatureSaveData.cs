using System;

namespace Endciv
{
    [Serializable]
    public class EntityFeatureSaveData : ISaveable
    {
        public SerVector3 position;
        public SerVector4 rotation;
        public SerVector2i gridPosition;

        //Properties
        public float health;

		public int bornTimeTick;

        public ISaveable CollectData()
        {
            return this;
        }
    }
}