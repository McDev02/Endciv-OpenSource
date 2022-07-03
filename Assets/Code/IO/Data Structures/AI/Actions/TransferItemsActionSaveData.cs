using System;

namespace Endciv
{
    [Serializable]
    public class TransferItemsActionSaveData : ActionSaveData, ISaveable
    {
        public bool canTransfer = false;

        public override ISaveable CollectData()
        {
            return this;
        }
    }
}