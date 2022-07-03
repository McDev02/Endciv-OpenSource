using UnityEditor;

namespace Endciv.Editor
{
    public class EntityFeatureEditor : FeatureEditor<EntityFeature>
    {       
        public override void OnGUI()
        {
            EditorGUILayout.LabelField("Name: " + Feature.EntityName);
            EditorHelper.EntityPropertyProgressBar("Health", Feature.Health);
        }
    }

}
