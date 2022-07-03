using UnityEditor;
using System.Text;

namespace Endciv.Editor
{
    public class ProductionFeatureEditor : FeatureEditor<ProductionFeature>
    {
        private StringBuilder stringBuilder;

        public override void OnEnable()
        {
            stringBuilder = new StringBuilder();
        }

        public override void OnGUI()
        {
            stringBuilder.Length = 0;
            stringBuilder.Append("Requirements: ");
            if (Feature.StaticData.NeedsLabour)
            {
                stringBuilder.Append("Labour");
                if (Feature.StaticData.NeedsEnergy)
                {
                    stringBuilder.Append(" and Energy");
                }
            }
            else if (Feature.StaticData.NeedsEnergy)
            {
                stringBuilder.Append("Energy");
            }
            else stringBuilder.Append("None");

            EditorGUILayout.LabelField(stringBuilder.ToString());
            if (Feature.StaticData.NeedsLabour)
                EditorGUILayout.LabelField("Workers: " + Feature.WorkerCount + "/" + Feature.MaxWorkers);

            var lines = Feature.StaticData.ProductionLines;
            var currentWorkers = Feature.ActiveWorkers;
            for (int i = 0; i < lines; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                var worker = currentWorkers[i];
                if (worker == null)
                    EditorGUILayout.LabelField("No current Worker");
                else
                    EditorGUILayout.LabelField("Working: " + worker.Entity.GetFeature<EntityFeature>().EntityName);
                EditorGUILayout.EndVertical();
            }
        }
    }
}
