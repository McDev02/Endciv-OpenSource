using UnityEngine;

namespace Endciv
{
    public enum ECropState
    {
        Unplanted,
        Growing,
        Mature
    }

    public class CropFeature : Feature<CropFeatureSaveData>, 
        IViewController<CropView>
    {
        public ECropState cropState;
        public CropView View;

        public CropFeatureStaticData staticData;

        public float growFactor;
        public float fruitGrowFactor;
        public float fruits;
        public float seeds;

        public int Fruits { get { return (int)fruits; } }

        public bool CanHarvest
        {
            get
            {
                return fruits >= 1f || seeds >= 1f;
            }
        }
        /// <summary>
        /// Amount of Water
        /// </summary>
        public EntityProperty humidity;

        private float progress;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = Mathf.Clamp(value, 0f, 1f);
                if (View != null)
                {
                    View.SetProgress(progress);
                }
            }

        }

        public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
        {
            base.Setup(entity, args);
            staticData = entity.StaticData.GetFeature<CropFeatureStaticData>();                        
            if (staticData.growTime < 1) staticData.growTime = 1;//hack, put this validation in StaticDataIO
            fruitGrowFactor = Random.Range(staticData.fruitAmount.min, staticData.fruitAmount.max) / (float)staticData.growTime;
            growFactor = 1f / staticData.growTime;
            humidity = new EntityProperty(staticData.waterMaxValue);
            var featureParams = (CropFeatureParams)args;
            if(featureParams != null)
            {
                CurrentViewID = featureParams.CurrentViewID;
            }
        }

        public override void Run(SystemsManager manager)
        {
            base.Run(manager);
        }

        #region IViewController

        public int CurrentViewID { get; set; }

        public void SetView(FeatureViewBase view)
        {
            View = (CropView)view;
            View.SetProgress(0f);
        }

        public void ShowView()
        {
            if (View != null)
            {
                View.ShowHide(true);
            }
        }

        public void HideView()
        {
            if (View != null)
            {
                View.ShowHide(false);
            }
        }

        public void UpdateView()
        {
            if (View != null)
            {
                View.UpdateView();
            }
        }

        public void SelectView()
        {
            if (View != null)
            {
                View.OnViewSelected();
            }
        }

        public void DeselectView()
        {
            if (View != null)
            {
                View.OnViewDeselected();
            }
        }
        #endregion

        public override void ApplyData(CropFeatureSaveData data)
        {
            cropState = (ECropState)data.cropState;
            growFactor = data.growFactor;
            fruitGrowFactor = data.fruitGrowFactor;
            fruits = data.fruits;
            seeds = data.seeds;
            humidity = new EntityProperty(data.humidityMax, data.humidityCurrent);
            progress = data.progress;
            View.SetProgress(progress);
        }

        public override ISaveable CollectData()
        {
            var data = new CropFeatureSaveData();
            data.cropState = (int)cropState;
            data.growFactor = growFactor;
            data.fruitGrowFactor = fruitGrowFactor;
            data.fruits = fruits;
            data.seeds = seeds;
            data.humidityCurrent = humidity.Value;
            data.humidityMax = humidity.maxValue;
            data.progress = progress;
            data.variationID = CurrentViewID;
            return data;
        }
    }

}
