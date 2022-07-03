using System;
using System.Collections.Generic;

namespace Endciv
{
    public class ImmigrationGroup : ISaveable, ILoadable<ImmigrationGroupSaveData>
    {
		public List<ImmigrantAIAgentFeature> immigrants;
        public int timeRemaining;

        public void Setup(List<ImmigrantAIAgentFeature> immigrants, int timeRemaining)
        {
            this.immigrants = immigrants;
            this.timeRemaining = timeRemaining;
        }

        public ISaveable CollectData()
        {
            var saveData = new ImmigrationGroupSaveData();
            saveData.immigrantUIDs = new List<string>();
            if(immigrants != null)
            {
                foreach(var immigrant in immigrants)
                {
                    saveData.immigrantUIDs.Add(immigrant.Entity.UID.ToString());
                }
            }
            saveData.timeRemaining = timeRemaining;
            return saveData;
        }

        public void ApplySaveData(ImmigrationGroupSaveData data)
        {
            if (data == null)
                return;
            if(data.immigrantUIDs != null && data.immigrantUIDs.Count > 0)
            {
                immigrants = new List<ImmigrantAIAgentFeature>();
                foreach(var id in data.immigrantUIDs)
                {
					var guid = Guid.Parse(id);
					var immigrant = Main.Instance.GameManager.SystemsManager.Entities[guid];
					immigrants.Add(immigrant.GetFeature<ImmigrantAIAgentFeature>());                    
                }
            }
            timeRemaining = data.timeRemaining;
        }
    }

}
