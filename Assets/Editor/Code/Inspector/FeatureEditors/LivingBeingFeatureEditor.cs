using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
    public class LivingBeingFeatureEditor : FeatureEditor<LivingBeingFeature>
    {
        public override void OnGUI()
        {
            EditorHelper.ProgressBar(Feature.Thirst.Progress, EditorHelper.GetProgressBarTitle("Thirst", Feature.Thirst.Value, Feature.Thirst.maxValue), 16);
            EditorHelper.ProgressBar(Mathf.Pow(Feature.Thirst.Progress, 0.5f), EditorHelper.GetProgressBarTitle("Thirst", Feature.Thirst.Value, Feature.Thirst.maxValue), 16);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Drink 0.25l")) Feature.Thirst.Value += 0.25f;
            if (GUILayout.Button("Drink 0.5l")) Feature.Thirst.Value += 0.5f;
            EditorGUILayout.EndHorizontal();
            EditorHelper.ProgressBar(Feature.Hunger.Progress, EditorHelper.GetProgressBarTitle("Hunger", Feature.Hunger.Value, Feature.Hunger.maxValue), 16);
            if (GUILayout.Button("Eat 0.5")) Feature.Hunger.Value += 0.5f;
        }
    }
}