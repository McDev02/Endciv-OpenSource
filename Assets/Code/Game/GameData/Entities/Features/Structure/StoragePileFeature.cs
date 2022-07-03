namespace Endciv
{
	public class StoragePileFeature : Feature<StoragePileFeatureSaveData>
	{
		public override void ApplyData(StoragePileFeatureSaveData data)
		{
			
		}

		public override ISaveable CollectData()
		{
			return new StoragePileFeatureSaveData();
		}
	}

}
