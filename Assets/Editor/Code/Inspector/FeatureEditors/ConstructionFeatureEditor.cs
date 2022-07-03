using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
    public class ConstructionFeatureEditor : FeatureEditor<ConstructionFeature>
    {
        public override void OnGUI()
        {
            EditorGUILayout.LabelField("Construction State : " + Feature.ConstructionState);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorHelper.ProgressBar(Feature.ConstructionProgress, EditorHelper.GetProgressBarTitle("Construction", Feature.CurrentConstructionPoints, Feature.StaticData.MaxConstructionPoints), 16);
            EditorHelper.ProgressBar(Feature.ResourceProgress, EditorHelper.GetProgressBarTitle("Resource Progress", Feature.ResourceProgress * 100f, 100f), 16);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Finish", GUILayout.Width(60)))
            {
                ConstructionSystem.FinishConstructionSite(Feature);
            }
            EditorGUILayout.EndHorizontal();
            var cost = Feature.StaticData.Cost;
            EditorGUILayout.LabelField("Building materials", EditorStyles.boldLabel);
            foreach (var item in cost)
            {
                EditorGUILayout.LabelField(item.ResourceID + " : " + item.Amount);
            }
        }
    }
}
