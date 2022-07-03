using UnityEngine;
using System;

namespace Endciv
{
    [Serializable]
    public class GraveSaveData : ISaveable
    {
        public float constructionProgress;
        public int variationID;
        public int positionIndex;

        public ISaveable CollectData()
        {
            return this;
        }
    }

    public class GraveModelView : MonoBehaviour, ISaveable, ILoadable<GraveSaveData>
    {
        private float constructionProgress = 0f;

        public float ConstructionProgress
        {
            get
            {
                return constructionProgress;
            }
            set
            {
                if (value != constructionProgress)
                {
                    constructionProgress = value;
                    OnConstructionUpdated();
                }
            }
        }

        private int variationID;
        private int positionIndex;

        public void Setup(int positionIndex, int variationID, float constructionProgress = 0f)
        {
            ConstructionProgress = constructionProgress;
            this.positionIndex = positionIndex;
            this.variationID = variationID;
        }

        private void OnConstructionUpdated()
        {
            transform.localScale = new Vector3(1f, constructionProgress, 1f);
        }

        public ISaveable CollectData()
        {
            var data = new GraveSaveData();
            data.variationID = variationID;
            data.constructionProgress = constructionProgress;
            data.positionIndex = positionIndex;
            return data;
        }

        public void ApplySaveData(GraveSaveData data)
        {
            if (data == null)
                return;
            variationID = data.variationID;
            ConstructionProgress = data.constructionProgress;
            positionIndex = data.positionIndex;
        }

    }
}

