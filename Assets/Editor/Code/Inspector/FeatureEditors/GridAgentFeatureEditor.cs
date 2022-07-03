using UnityEditor;

namespace Endciv.Editor
{
    public class GridAgentFeatureEditor : FeatureEditor<GridAgentFeature>
    {
        public override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Has Path");
			
			EditorGUILayout.LabelField("Current Speed: " + Feature.CurrentSpeed.ToString());
            EditorGUILayout.LabelField("Speed Modifer: " + Feature.speedModifer.ToString());
			

			EditorGUILayout.LabelField("State: " + Feature.State.ToString());
            if (Feature.Job == null)
                EditorGUILayout.LabelField("No pathfinding Job");
            else
            {
                EditorGUILayout.LabelField("Job isReady: " + Feature.Job.IsReady.ToString());
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Destination", EditorStyles.boldLabel);
            if (Feature.Destination != null)
            {
                EditorGUILayout.LabelField("Type: " + Feature.Destination.Type.ToString());
                if (Feature.Destination.Type == Location.EDestinationType.Waypoint)
                    EditorGUILayout.LabelField("PosID: " + Feature.Destination.currentPositionID.ToString());
                EditorGUILayout.LabelField("Target Index: " + Feature.Destination.Index.ToString());
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

}
