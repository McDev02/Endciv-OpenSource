namespace Endciv
{
	public class WeaponFeature : Feature<WeaponFeatureSaveData>
	{		
		public override ISaveable CollectData()
		{
			var data = new WeaponFeatureSaveData();
			return data;
		}

		public override void ApplyData(WeaponFeatureSaveData data)
		{
			
		}
	}
}