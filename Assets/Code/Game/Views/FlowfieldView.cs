using UnityEngine;
namespace Endciv
{
    public class FlowfieldView : MonoBehaviour
    {
        Camera Camera;
        [SerializeField] GameManager GameManager;
        [SerializeField] TerrainView TerrainView;
        [SerializeField] Color LineColor = Color.black;
        [SerializeField] Color ValueGood = Color.green;
        [SerializeField] Color ValueBad = Color.red;
        [SerializeField] bool DrawValues;

        public float MaxIntegrationCost;

        Gridfield CurrentFlowfield;
        bool IsRunning;

        Color[] Colors;

        public enum EDebugView { None, Cost, Integration, Flow }
        [SerializeField] EDebugView DebugMode;
        EDebugView oldDebugMode;

        public void Setup(Gridfield flowfield)
        {
            oldDebugMode = DebugMode = EDebugView.None;
            Camera = GameManager.CameraController.Camera;
            CurrentFlowfield = flowfield;
            Colors = new Color[CurrentFlowfield.Width * CurrentFlowfield.Length];
        }

#if USE_FLOWFIELDS
		private void OnDrawGizmos()
        {
			if (DebugMode != EDebugView.Flow && !IsRunning) return;
			if (CurrentFlowfield == null) return;

			var map = CurrentFlowfield.LastFlowMap;
			if (map == null || map.Length <= 0) return;

			const float h = 0.1f;
			const float l = GridMapView.TileSize * 3 / 4;
			Velocity2 vel;
			Gizmos.color = LineColor;
			for (int x = 0; x < CurrentFlowfield.Width; x++)
			{
				for (int y = 0; y < CurrentFlowfield.Length; y++)
				{
					vel = map[x, y];
					Vector3 A = new Vector3((x + 0.5f) * GridMapView.TileSize, h, (y + 0.5f) * GridMapView.TileSize);
					Vector3 B = new Vector3(vel.X * l, 0, vel.Y * l);
					Gizmos.DrawLine(A, A + B);

				}
			}
		}
#endif

        void OnGUI()
        {
            if (!DrawValues) return;
            if (Camera == null) return;

            float[,] map;
            if (DebugMode == EDebugView.Cost)
                map = CurrentFlowfield.Cost;
            else if (DebugMode == EDebugView.Integration)
                map = CurrentFlowfield.IntegrationCost;
            else return;

            float hwidth = 20;
            Vector3 pos = new Vector3(0, 0.5f, 0);
            for (int x = 0; x < CurrentFlowfield.Width; x++)
            {
                for (int y = 0; y < CurrentFlowfield.Length; y++)
                {
                    pos.x = (x + 0.5f) * GridMapView.TileSize;
                    pos.z = (y + 0.5f) * GridMapView.TileSize;
                    var screenPos = Camera.WorldToScreenPoint(pos);
                    screenPos.y = Screen.height - screenPos.y;
                    GUI.Label(new Rect(screenPos.x - hwidth, screenPos.y, hwidth * 2, 30), map[x, y].ToString("0.#"));
                }
            }
        }

        public void UpdateMap()
        {
            //Only on change we hide the layer
            if (oldDebugMode != DebugMode)
            {
                if (DebugMode == EDebugView.None || DebugMode == EDebugView.Flow)
                {
                    TerrainView.HideLayerMap(); return;
                }
            }

            oldDebugMode = DebugMode;
            Color col = Color.black; col.a = 0;
            float val;
            for (int x = 0; x < CurrentFlowfield.Width; x++)
            {
                for (int y = 0; y < CurrentFlowfield.Length; y++)
                {
                    switch (DebugMode)
                    {
                        case EDebugView.Cost:
                            val = CurrentFlowfield.Cost[x, y] / CurrentFlowfield.MaxCost;
                            col = Color.Lerp(ValueGood, ValueBad, val);
                            break;
                        case EDebugView.Integration:
                            val = CurrentFlowfield.IntegrationCost[x, y] / CurrentFlowfield.MaxIntegrationCost;
                            col = Color.Lerp(ValueGood, ValueBad, val);
                            break;
                    }

                    Colors[x + y * CurrentFlowfield.Width] = col;
                }
            }
            TerrainView.ShowLayerMap(Colors,true);
        }
    }
}