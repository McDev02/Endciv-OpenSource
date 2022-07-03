using UnityEditor;

namespace Endciv.Editor
{
	public class PastureFeatureEditor : FeatureEditor<PastureFeature>
	{
		public override void OnGUI()
		{
			EditorHelper.ProgressBar(Feature.Filth, EditorHelper.GetProgressBarTitle("Filth", Feature.Filth, 1f), 16);
			EditorHelper.ProgressBar(Feature.WaterProgress, EditorHelper.GetProgressBarTitle("Water", Feature.CurrentWater, Feature.StaticData.maxWater), 16);
			EditorHelper.ProgressBar(Feature.NutritionProgress, EditorHelper.GetProgressBarTitle("Nutrition", Feature.CurrentNutrition, Feature.StaticData.maxNutrition), 16);
			EditorGUILayout.LabelField("Cattle Count: " + Feature.Cattle.Count);
			if(Feature.Cattle.Count > 0)
			{
				EditorGUI.indentLevel++;
				foreach (var cattle in Feature.Cattle)
				{
					EditorGUILayout.LabelField(cattle.Entity.StaticData.ID);
				}
				EditorGUI.indentLevel--;
			}
			EditorHelper.ProgressBar((Feature.MaxCapacity - Feature.CapacityLeft) / (float)Feature.MaxCapacity, EditorHelper.GetProgressBarTitle("Cattle Capacity", Feature.MaxCapacity - Feature.CapacityLeft, Feature.MaxCapacity), 16);
		}
	}
}