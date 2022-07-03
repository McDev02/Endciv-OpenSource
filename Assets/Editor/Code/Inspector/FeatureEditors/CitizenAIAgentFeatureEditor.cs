using UnityEditor;

namespace Endciv.Editor
{
    public class CitizenAIAgentFeatureEditor : FeatureEditor<CitizenAIAgentFeature>
    {
        public override void OnGUI()
        {            
            EditorGUILayout.LabelField("Occupation : " + Feature.Occupation.ToString());
        }
    }
}
