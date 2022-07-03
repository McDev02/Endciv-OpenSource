namespace Endciv.Editor
{
	public class CattleFeatureEditor : FeatureEditor<CattleFeature>
	{
		public override void OnGUI()
		{
			EditorHelper.ProgressBar((float)(Feature.ProducedGoods / (float)Feature.staticData.ProductionCapacity), EditorHelper.GetProgressBarTitle("Produced Items", Feature.ProducedGoods, Feature.staticData.ProductionCapacity), 16);			
		}
	}
}