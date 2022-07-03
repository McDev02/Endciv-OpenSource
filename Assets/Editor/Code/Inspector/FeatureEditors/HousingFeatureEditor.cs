using UnityEditor;

namespace Endciv.Editor
{
    public class HousingFeatureEditor : FeatureEditor<HousingFeature>
    {
        public override void OnGUI()
        {                        
            EditorHelper.ProgressBar(Feature.OccupantsProgress, EditorHelper.GetProgressBarTitle("Occupants", Feature.CurrentOccupants, Feature.MaxOccupants), 16);

            if (Feature.Occupants != null)
            {
                for (int i = 0; i < Feature.Occupants.Count; i++)
                {
                    var occupant = Feature.Occupants[i];
                    EditorGUILayout.LabelField("Occupant " + i.ToString() + ": " + occupant.GetFeature<EntityFeature>().EntityName + " (" + occupant.GetFeature<EntityFeature>().View.name + ")");
                }
            }
        }
    }
}
