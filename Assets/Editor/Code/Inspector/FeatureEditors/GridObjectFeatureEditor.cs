using UnityEditor;
using System.Text;

namespace Endciv.Editor
{
    public class GridObjectFeatureEditor : FeatureEditor<GridObjectFeature>
    {
        private StringBuilder stringBuilder;

        public override void OnEnable()
        {
            stringBuilder = new StringBuilder();
        }

        public override void OnGUI()
        {
            if (Feature.PartitionIDs.Count <= 0)
                EditorGUILayout.LabelField("No partition IDs");
            else
            {
                stringBuilder.Length = 0;
                stringBuilder.Append("Partition IDs: ");
                for (int i = 0; i < Feature.PartitionIDs.Count; i++)
                {
                    if (i > 0)
                        stringBuilder.Append(" - ");
                    stringBuilder.Append(Feature.PartitionIDs[i].ToString());
                }
                EditorGUILayout.LabelField(stringBuilder.ToString());
            }

            EditorGUILayout.LabelField("Rect:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(Feature.GridObjectData.Rect.ToString());
        }
    }
}

