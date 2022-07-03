using UnityEngine;
namespace Endciv
{
    public class UserToolsView : MonoBehaviour
    {
        public MeshRenderer RectIndicatorPrefab;
        public MeshRenderer EntranceIndicatorPrefab;
        public MeshRenderer RadiusIndicatorPrefab;
		public Color ValidColor;
        public Color InvalidColor;
        public Color PartialColor;

		public Color WaterColor;
		public Color PollutionColor;

		public string ColorName = "_Color";
    }
}