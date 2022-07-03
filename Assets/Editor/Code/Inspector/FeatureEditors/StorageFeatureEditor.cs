using UnityEditor;

namespace Endciv.Editor
{
    public class StorageFeatureEditor : FeatureEditor<StorageFeature>
    {
        public override void OnGUI()
        {
            Feature.policy = (EStoragePolicy)EditorGUILayout.EnumPopup("Storage Policy", Feature.policy);
        }
    }

}
