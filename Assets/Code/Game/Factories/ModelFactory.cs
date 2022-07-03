using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
    public class ModelFactory
    {
        private string DummyObject = "dummy";

        //Views
        public Dictionary<string, Dictionary<int, GameObject>> PrefabViews { get; private set; }

        private string prefabPath;

        public ModelFactory(string prefabPath)
        {
            this.prefabPath = prefabPath;            
            LoadPrefabViews();
        }

        public void LoadPrefabViews()
        {
            var dataPool = Resources.LoadAll<GameObject>(prefabPath);
            PrefabViews = new Dictionary<string, Dictionary<int, GameObject>>(dataPool.Length);
            Dictionary<int, GameObject> tmpVariations = null;
            string logstring = "";
            foreach (var item in dataPool)
            {
                tmpVariations = new Dictionary<int, GameObject>();
                string itemKey = item.name;
                if(item.name.Contains('_'))
                {
                    itemKey = item.name.Substring(0, item.name.LastIndexOf('_'));
                }
                
                if (!PrefabViews.ContainsKey(itemKey))
                {
                    logstring = "View Objects for " + itemKey + "\n";

                    string variationPath = prefabPath + itemKey + "_";
                    tmpVariations.Add(0, item);
                    logstring += "Add ViewModel:" + item.name + "\n";
                    //Find variations
                    for (int i = 1; i < 20; i++)
                    {
                        var obj = Resources.Load<GameObject>(variationPath + i);
                        if (obj != null)
                        {
                            tmpVariations.Add(i, obj);
                            logstring += "Add ViewModel:" + obj.name + "\n";
                        }
                    }
                    PrefabViews.Add(itemKey, tmpVariations);
                    Debug.Log(logstring);
                }
            }
        }

        public T GetModelObject<T>(string modelID, int variationID = -1) where T : MonoBehaviour
        {            
			var viewModel = GetModelObject(modelID, variationID);
            T component = viewModel.GetComponent<T>();
            if (component == default(T))
                component = viewModel.AddComponent<T>();
            return component;
        }

		public GameObject GetModelObject(string modelID, int variationID = -1)
		{
			var views = PrefabViews[modelID];
			if (!views.ContainsKey(variationID))
				variationID = views.Keys.ToArray()[Random.Range(0, views.Count)];

			GameObject viewModel = Object.Instantiate(views[variationID]);
			viewModel.name = modelID;
			return viewModel;
		}

        public int GetNextViewID(string modelID, int currentID)
        {
            var keys = PrefabViews[modelID].Keys.ToList();
            int pos = keys.IndexOf(currentID) + 1;
            if (pos >= keys.Count)
                pos = 0;
            return keys[pos];
        }

        public int GetRandomViewID(string modelID)
        {
            return PrefabViews[modelID].Keys.ToArray()[Random.Range(0, PrefabViews[modelID].Count)];
        }

        public T GetRandomModelObject<T>(string modelID, out int variationID) where T : MonoBehaviour
        {
            var views = PrefabViews[modelID];
            variationID = views.Keys.ToArray()[Random.Range(0, views.Count)];

            GameObject viewModel = Object.Instantiate(views[variationID]);
			viewModel.name = modelID;
			T component = viewModel.GetComponent<T>();
            if (component == default(T))
                component = viewModel.AddComponent<T>();
            return component;
        }
    }

}
