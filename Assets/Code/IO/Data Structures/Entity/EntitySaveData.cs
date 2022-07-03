using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Endciv
{
    [Serializable]
    public sealed class EntitySaveData : ISaveable
    {
        private static Type[] AIDerivedTypes { get; set; }

        //Base data
        public string id;
        public string UID;
        public int factionID;

        //Features
        public Dictionary<string, object> featureSaveData;

        public AIAgentFeatureSaveData GetFirstAIFeature()
        {
            if (featureSaveData == null)
            {
                return null;
            }

            if(AIDerivedTypes == null)
            {
                AIDerivedTypes = Assembly.GetExecutingAssembly().GetTypes().
                Where(t => t != typeof(AIAgentFeatureSaveData)
                && typeof(AIAgentFeatureSaveData).IsAssignableFrom(t)).ToArray();
            }            
            
            foreach(var pair in featureSaveData)
            {           
                foreach(var type in AIDerivedTypes)
                {
                    if(pair.Key == type.ToString())
                    {                        
                        return pair.Value as AIAgentFeatureSaveData;
                    }                    
                }                
            }
            return null;
        }

        public object GetSaveData(Type type)
        {
            if (featureSaveData == null)
            {
                return null;
            }
            if (featureSaveData.ContainsKey(type.ToString()))
            {
                return featureSaveData[type.ToString()];
            }
            return null;
        }

        public T GetSaveData<T>() where T : ISaveable
        {
            if (featureSaveData == null)
            {
                return default(T);
            }
            var type = typeof(T);
            if (featureSaveData.ContainsKey(type.ToString()))
            {
                return (T)featureSaveData[type.ToString()];
            }
            return default(T);
        }

        public ISaveable CollectData()
        {
            return this;
        }
    }
}