using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Endciv
{
    public abstract class StaticDataReaderBase
    {
      
    }

    public class StaticDataReader<T> : StaticDataReaderBase where T : BaseStaticData, new()
    {
        protected StaticDataIO StaticDataIO;
        public Dictionary<string, T> Data { get; protected set; }

        protected string Filename = "GridObjects";
        protected const string TemplatePath = "Template/";

        public Dictionary<string, T> GetStaticData()
        {
            return Data;
        }

        public StaticDataReader(StaticDataIO staticDataIO, string filename)
        {
            StaticDataIO = staticDataIO;
            Filename = filename;            
        }        

#if UNITY_EDITOR
        public void WriteScriptableObjects()
        {
            string fullpath = StaticDataIO.StaticDataPathSerializedObjects + Filename + "/";
            if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
            string path;

            //Store each static data entry as a scriptable object by its ID as name
            foreach (var data in Data)
            {
                path = fullpath + data.Key + ".asset";
                try
                {
                    AssetDatabase.CreateAsset(data.Value, path);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Could not write " + Filename + " StaticData: " + data.Key + "\n at: "
                        + path + "\n"
                        + ex.Message);
                }
            }
            AssetDatabase.SaveAssets();
        }
#endif

        public virtual bool ReadScriptableObjects(bool debugLog = false, bool createinstance = true)
        {
            string fullpath = StaticDataIO.StaticDataFolderSerializedObjects + Filename + "/";
            T[] data = Resources.LoadAll<T>(fullpath);
            Data = new Dictionary<string, T>(data.Length);
            StringBuilder log = new StringBuilder();
            foreach (var entry in data)
            {
                entry.Init();
                if (debugLog) log.AppendLine(entry.ID);
                try
                {
                    if (createinstance)
                        Data.Add(entry.ID, GameObject.Instantiate(entry));
                    else
                        Data.Add(entry.ID, entry);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error on entry<" + typeof(T).ToString() + ">: " + entry.ID.ToString());
                    throw e;
                }
            }
            if (debugLog) Debug.Log(log.ToString());
            if (Data == null || Data.Count <= 0)
                return false;
            if (debugLog)
                Debug.Log(Filename + " Scriptable Object Imported.");
            return true;
        }        

        public T Find(string id)
        {
            if (!Data.ContainsKey(id)) return null;
            return Data[id];
        }
    }
}