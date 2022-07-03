using UnityEngine;

namespace Endciv
{
	public class ResourcePileView : FeatureView<ResourcePileFeature>
	{
		[SerializeField] float minPosition = -0.5f;
		[HideInInspector]
		public EStoragePolicy storagePolicy;

		public override void Setup(FeatureBase feature)
		{
            base.Setup(feature);
			storagePolicy = Feature.StoragePolicy;
        }

		public override void UpdateView()
		{
			int amount = 0;
			foreach (var res in Feature.resources)
			{
				amount += res.Amount;
			}
			if (amount > Feature.startResources)
				Debug.LogError($"wrong, starting resources too low {amount}/{Feature.startResources}");
			var pos = transform.localPosition;
			pos.y = minPosition * (1 - amount / (float)Feature.startResources);
			transform.localPosition = pos;
		}
	}
}