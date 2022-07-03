using UnityEngine;

namespace Endciv
{
    public class CropView : FeatureView<CropFeature>
    {
        public void SetProgress(float progress)
        {
            transform.localScale = Vector3.one * (progress <= 0 ? 0.01f : progress);
        }

        public override void UpdateView()
        {
            
        }
    }
}
