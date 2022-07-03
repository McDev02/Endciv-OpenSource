using System.Linq;
using UnityEngine;
namespace Endciv
{
	[RequireComponent(typeof(GridDebugController))]
	public class GridDebugView : MonoBehaviour
	{
		BaseGrid Grid;

		[SerializeField] GameObject NodePrefab;
		[SerializeField] GameObject LinkPrefab;

		[SerializeField] Color NodeColor = Color.white;
		[SerializeField] Color LinkColor = Color.blue;

		private GlobalGameObjectPool<Transform> CachedNodes;
		private GlobalGameObjectPool<Transform> CachedLinks;

		private Transform NodeContainer;
		private Transform LinkContainer;

		[SerializeField] float NodeSize = 1;
		private float nodeSize;
		private float linkSize;


		[SerializeField] bool DrawMeshes = false;
		[SerializeField] bool DrawGizmos = false;
		[SerializeField] bool ShowNodes = true;
		[SerializeField] bool ShowLinks = true;

		void Awake()
		{
			CachedNodes = new GlobalGameObjectPool<Transform>();
			CachedLinks = new GlobalGameObjectPool<Transform>();
		}

		public void Setup(BaseGrid grid)
		{
			Grid = grid;

			nodeSize = GridMapView.TileSize / 2f;
			linkSize = nodeSize / 3f;
		}

		internal void Clear()
		{
			int count;
			if (NodeContainer != null)
			{
				count = NodeContainer.childCount;
				for (int i = 0; i < count; i++)
				{
					CachedNodes.Recycle(NodeContainer.GetChild(0));
				}
			}
			if (LinkContainer != null)
			{
				count = LinkContainer.childCount;
				for (int i = 0; i < count; i++)
				{
					CachedLinks.Recycle(LinkContainer.GetChild(0));
				}
			}
		}

		public void DrawGrid()
		{
			if (!DrawMeshes) return;
			if (NodeContainer == null)
			{
				NodeContainer = new GameObject("Nodes").transform;
				NodeContainer.SetParent(transform, false);
			}
			if (LinkContainer == null)
			{
				LinkContainer = new GameObject("Links").transform;
				LinkContainer.SetParent(transform, false);
			}

			var nodes = Grid.Nodes.ToList();
			var links = Grid.Links.ToList();
			Transform obj;
			UnityEngine.Debug.Log("Nodes: " + nodes.Count.ToString());
			if (ShowNodes)
			{
				//Generate Nodes
				for (int i = 0; i < nodes.Count; i++)
				{
					var node = nodes[i].Value;
					if (CachedNodes.HasObjects)
					{
						obj = CachedNodes.Get();
						obj.gameObject.SetActive(true);
						obj.transform.SetParent(NodeContainer);
						obj.transform.position = node.Coordinates;
					}
					else
						obj = Instantiate(NodePrefab, node.Coordinates, Quaternion.identity, NodeContainer).transform;
					obj.name = "Node " + node.ID.ToString();
					obj.transform.localScale = new Vector3(nodeSize, nodeSize, nodeSize);
				}
			}

			if (ShowLinks)
			{
				//Generate Links
				for (int i = 0; i < links.Count; i++)
				{
					var link = links[i].Value;
					if (CachedLinks.HasObjects)
					{
						obj = CachedLinks.Get();
						obj.gameObject.SetActive(true);
						obj.transform.SetParent(LinkContainer);
						obj.transform.position = link.Center;

					}
					else
						obj = Instantiate(LinkPrefab, link.Center, Quaternion.identity, LinkContainer).transform;
					obj.name = "Link " + link.ID.ToString();
					obj.transform.LookAt(link.B.Coordinates);
					obj.transform.localScale = new Vector3(linkSize, linkSize, link.GetDistance());
				}
			}
		}

		void OnDrawGizmos()
		{
			if (Grid == null || !DrawGizmos) return;
			if (ShowNodes)
			{
				var nodeScale = new Vector3(nodeSize, nodeSize, nodeSize);

				if (Grid.NodeGroups.Count > 0)
				{
                    foreach (var nodes in Grid.NodeGroups)
                    {
						Gizmos.color = RandomColorPool.Instance.GetColor(nodes.Key);// Helpers.RandomColorPool.Instance.GetColor(g);			
						for (int i = 0; i < nodes.Value.Count; i++)
						{
							Gizmos.DrawCube(nodes.Value[i].Coordinates, nodeScale);
						}
					}
				}
				else
				{
					Gizmos.color = NodeColor;
					var nodes = Grid.Nodes.ToList();
					for (int i = 0; i < nodes.Count; i++)
					{
						Gizmos.DrawCube(nodes[i].Value.Coordinates, nodeScale);
					}
				}
			}
			if (ShowLinks)
			{
				Vector3 offset = new Vector3(0, 0.1f, 0);
				Gizmos.color = LinkColor;
				var links = Grid.Links.ToList();
				for (int i = 0; i < links.Count; i++)
				{
					Gizmos.DrawLine(links[i].Value.A.Coordinates + offset, links[i].Value.B.Coordinates + offset);
				}
			}
		}
	}
}