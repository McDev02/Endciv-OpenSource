namespace Endciv
{
	public class ItemFeatureStaticData : FeatureStaticData<ItemFeature>
	{			
		[ReadOnly]
		public bool IsStackable = true;
		public int Mass = 1;
		public float Value = 1;
		public EStoragePolicy Category;

#if UNITY_EDITOR
		public override void OnFeatureStaticDataChanged()
		{
			bool canStack = true;
			foreach(var feature in entity.FeatureStaticData)
			{
				if (feature is INonStackableFeature)
				{
					canStack = false;
					break;
				}					
			}
			IsStackable = canStack;
		}
#endif
	}
}