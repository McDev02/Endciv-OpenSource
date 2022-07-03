using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class CivDebug : MonoBehaviour
	{
		[SerializeField] GridMap GridMap;
		public static CivDebug Instance;
		List<Vector2i> Nodes;

		void Awake()
		{
			Instance = this;
			Nodes = new List<Vector2i>();
		}

		public void DrawNode(Vector2i node)
		{
			if (!Nodes.Contains(node))
				Nodes.Add(node);
		}

		public void ClearNodes()
		{
			Nodes.Clear();
		}

		// Update is called once per frame
		private void OnDrawGizmos()
		{
			if (Nodes == null || Nodes.Count <= 0) return;

			Vector3 size = new Vector3(GridMapView.TileSize * 0.4f, 0.1f, GridMapView.TileSize * 0.4f);
			Vector3 pos;
			for (int i = 0; i < Nodes.Count; i++)
			{
				pos = GridMap.View.GetTileWorldPosition(Nodes[i]).To3D(0.1f);
				Gizmos.DrawCube(pos, size);
			}
		}
	}
}