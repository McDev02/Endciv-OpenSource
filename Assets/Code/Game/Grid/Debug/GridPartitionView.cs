using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// </summary>
	public class GridPartitionView : MonoBehaviour
	{
		[SerializeField] Transform TestObject;
		[SerializeField] GameManager GameManager;
		[SerializeField] [Range(0, 4)] int sampleRadius = 0;

		[SerializeField] Color ColorNormal;
		[SerializeField] Color ColorAdjacent;
		[SerializeField] Color ColorCurrent;

		bool IsReady;
		Vector2i CurrentPartition;
		List<Vector2i> AdjacentPartitions;

		PartitionSystem context;

		private void Awake()
		{
#if !UNITY_EDITOR
			enabled = false;
#endif
			if (enabled)
				GameManager.OnGameRun += Run;
		}

		public void Run()
		{
			context = GameManager.SystemsManager.PartitionSystem;
			IsReady = true;
		}

		void LateUpdate()
		{
			if (!IsReady || TestObject == null) return;

			Vector2 position = TestObject.position.To2D() * GridMapView.InvTileSize;
			CurrentPartition = context.SamplePartitionID(position);
			var rel = context.GetRelativePartitionPosition(CurrentPartition, position);
			AdjacentPartitions = context.GetAdjacentPartitions(CurrentPartition, rel, sampleRadius, false);
		}

		void OnDrawGizmos()
		{
			if (!IsReady) return;
			const float height = 0.05f;
			float viewSize = context.PartitionSize * GridMapView.TileSize;
			float viewHalfSize = viewSize / 2f;
			Vector3 size = new Vector3(viewSize - GridMapView.TileSize / 3f, height * 2 + 0.04f, viewSize - GridMapView.TileSize / 3f);
			Vector3 center = new Vector3(-viewHalfSize, height, -viewHalfSize);
			Vector2i curID = Vector2i.Zero;
			for (int x = 0; x < context.PartitionsX; x++)
			{
				center.x += viewSize;
				center.z = -viewHalfSize;
				for (int y = 0; y < context.PartitionsY; y++)
				{
					center.z += viewSize;
					curID.X = x;
					curID.Y = y;
					if (ContainsAdjacent(curID))
						Gizmos.color = ColorAdjacent;
					else if (curID == CurrentPartition)
						Gizmos.color = ColorCurrent;
					else
						Gizmos.color = ColorNormal;
					Gizmos.DrawWireCube(center, size);
				}
			}
		}

		bool ContainsAdjacent(Vector2i id)
		{
			for (int i = 0; i < AdjacentPartitions.Count; i++)
			{
				if (AdjacentPartitions[i] == id) return true;
			}
			return false;
		}
	}
}