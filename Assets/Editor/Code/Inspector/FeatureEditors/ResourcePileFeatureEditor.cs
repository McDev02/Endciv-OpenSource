using UnityEditor;

namespace Endciv.Editor
{
    public class ResourcePileFeatureEditor : FeatureEditor<ResourcePileFeature>
    {
        public override void OnGUI()
        {
            EditorGUILayout.LabelField("Type", Feature.ResourcePileType.ToString());
            
            if (Feature.resources == null)
                return;
            EditorGUILayout.LabelField("Resources:");
            foreach (var resource in Feature.resources)
            {
                EditorGUILayout.LabelField(resource.ResourceID + " : " + resource.Amount);
            }
        }
    }

}
