namespace Endciv
{
	public class ToolFeature : Feature<ToolFeatureSaveData>
	{		
		public override ISaveable CollectData()
		{
			var data = new ToolFeatureSaveData();
			return data;
		}

		public override void ApplyData(ToolFeatureSaveData data)
		{
			
		}
	}
}