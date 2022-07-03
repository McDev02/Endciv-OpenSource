using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Endciv
{
	/// <summary>
	/// Model Class for a Rectangular Grid
	/// </summary>
	public class RectGrid : BaseGrid
	{
		public enum EConnectionMode { None, Perpendicular, Diagonal, All }
		public EConnectionMode ConnectionMode;

		public Dictionary<Node, Vector2i> NodePositionLookup;
		public Dictionary<int, Vector2i> NodeLookupID;
		public Node[,] NodeLookup { get; private set; }

		public int Width { get; private set; }
		public int Length { get; private set; }
		public int Area { get; private set; }

		public int VertexWidth { get { return Width + 1; } }
		public int VertexLength { get { return Length + 1; } }

		public int[,] Data;



		public RectGrid(int width, int length)
		{
			NodeLookupID = new Dictionary<int, Vector2i>();
			NodeLookup = new Node[width, length];

			Width = width;
			Length = length;
			Area = width * length;

			Data = new int[Width, length];
		}

		public void FloodFill(RectBounds rect)
		{
			if (rect.Area <= 0) return;

			//Setup
			List<Node> remainingNodes = new List<Node>(rect.Area);
			var nodes = Nodes.ToList();
			Node node;
			for (int x = rect.X; x <= rect.Maximum.X; x++)
			{
				for (int y = rect.Y; y <= rect.Maximum.Y; y++)
				{
					node = NodeLookup[x, y];
					if (node.LayerID >= 0)
						remainingNodes.Add(node);
				}
			}
			if (remainingNodes.Count > 0)
				FloodFillOperation(remainingNodes);
		}

		public RectBounds FloodFillFindRect(int layer, Vector2i startPos, out List<Node> visitedNodes)
		{
			visitedNodes = new List<Node>(32);
			RectBounds rect = new RectBounds(startPos);
			int counter = 0;

			Stack<Node> seekers = new Stack<Node>();
			Node startNode = NodeLookup[startPos.X, startPos.Y];
			seekers.Push(startNode);
			visitedNodes.Add(startNode);
			Node seeker, other;
			while (seekers.Count > 0 && counter < 100000)
			{
				counter++;
				seeker = seekers.Pop();
				visitedNodes.Add(seeker);
				rect.Insert(NodeLookupID[seeker.ID]);
				for (int i = 0; i < seeker.Links.Count; i++)
				{
					other = seeker.Links[i].GetOther(seeker);
					if (!seekers.Contains(other) && !visitedNodes.Contains(other))
					{
						seekers.Push(other);
					}
				}
			}

			return rect;
		}

		internal bool HasNode(int x, int y)
		{
			return NodeLookup[x, y] != null;
		}
		internal bool HasLink(int x, int y)
		{
			return NodeLookup[x, y] != null;
		}

		public Node AddNode(int x, int y, Vector3 coords)
		{
			var node = AddNode(coords);
			NodeLookup[x, y] = node;
			NodeLookupID.Add(node.ID, new Vector2i(x, y));
			return node;
		}
		public void RemoveNode(int x, int y)
		{
			var node = NodeLookup[x, y];
			int id = node.ID;
			RemoveNodeLinks(node);
			RemoveNode(id);
			NodeLookup[x, y] = null;
		}

		void RemoveNodeLinks(Node node)
		{
			while (node.Links.Count > 0)
			{
				RemoveLink(node.Links[0].ID);
			}
		}


		public Node GetNode(Vector2i index)
		{
			return NodeLookup[index.X, index.Y];
		}

		/// <summary>
		/// Find Link between two Nodes and return link
		/// </summary>
		public bool FindLink(Vector2i A, Vector2i B, out Link link)
		{
			link = new Link();
			var nodeA = GetNode(A);
			for (int i = 0; i < nodeA.LinkCount; i++)
			{   //Go through all Links of A and check for B
				link = nodeA.Links[i];
				if (link.HasConnection(GetNode(B)))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Find random position in whole map
		/// </summary>
		public Vector2i GetRandomPosition()
		{
			return new Vector2i(CivRandom.Range(0, Width), CivRandom.Range(0, Length));
		}


		/// <summary>
		/// Find random position inside NodeGroup
		/// </summary>
		internal bool GetRandomPassablePosition(Vector2i gridID, out Vector2i goal)
		{
			goal = gridID;
			Vector2i pos = gridID;
			var islandID = NodeLookup[gridID.X, gridID.Y].GroupID;

			if (islandID < 0 && islandID >= NodeGroups.Count || !NodeGroups.ContainsKey(islandID))
				return false;

			var nodes = NodeGroups[islandID];
			var node = nodes[CivRandom.Range(0, nodes.Count)];

			return true;
		}
		/// <summary>
		/// Find random position inside NodeGroup and radius
		/// </summary>
		internal bool GetRandomPassablePosition(Vector2i gridID, float maxDistance, out Vector2i goal)
		{
			return GetRandomPassablePosition(gridID, 0, maxDistance, out goal);
		}
		/// <summary>
		/// Find random position inside NodeGroup and radius
		/// </summary>
		internal bool GetRandomPassablePosition(Vector2i gridID, float minDistance, float maxDistance, out Vector2i goal)
		{
			goal = gridID;
			Vector2i pos = gridID;
			var myNode = NodeLookup[gridID.X, gridID.Y];
			var islandID = myNode.GroupID;

			if (true)
			{
				for (int i = 0; i < 400; i++)
				{
					var nodeID = gridID + (Vector2i)(CivRandom.OnUnitCircle * (CivRandom.Range(minDistance, maxDistance) * GridMapView.TileSize));
					if (!IsInRange(nodeID)) continue;
					var node = NodeLookup[nodeID.X, nodeID.Y];
					if (node.GroupID == islandID)
					{
						goal = nodeID;
						return true;
					}
				}
			}
			else
			{
				//Find passable ID
				if (islandID < 0 || !NodeGroups.ContainsKey(islandID))
					islandID = FindNearbyPassableIslandID(gridID);
				if (islandID < 0 || !NodeGroups.ContainsKey(islandID))
					return false;
				var nodes = NodeGroups[islandID];
				var nodePos = myNode.Coordinates.To2D();
				int lastID = myNode.ID;

				for (int i = 0; i < 400; i++)
				{
					var node = nodes[CivRandom.Range(0, nodes.Count)];
					lastID = node.ID;
					if ((node.Coordinates.To2D() - nodePos).magnitude <= maxDistance)
					{
						goal = NodeLookupID[lastID];
						return true;
					}
				}
			}
			//No path found
			return false;
		}

		/// <summary>
		/// Returns a passable IslandID in neigbouring cells
		/// If fail returns current ID
		/// </summary>
		int FindNearbyPassableIslandID(Vector2i origin)
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (IsInRange(origin.X + x, origin.Y + y))
					{
						var newID = NodeLookup[origin.X + x, origin.Y + y].GroupID;
						if (newID > 0) return newID;
					}
				}
			}
			return NodeLookup[origin.X, origin.Y].GroupID;
		}
		/// <summary>
		/// Will return FindNearbyPassableIslandID(Vector2i origin)
		/// Should be updated to find any passable cell
		/// </summary>
		int FindClosestPassableIslandID(Vector2i origin)
		{
			return FindNearbyPassableIslandID(origin);
		}

		/// <summary>
		/// Not entirely shire, but seems to provide the orthogonal link
		/// </summary>
		public Link GetVertexLink(Vector2i a, Vector2i b)
		{
			Vector2 mid = Vector2.Lerp(a, b, 0.5f);
			Vector2 off = Vector2.one * 0.2f;
			Vector2i nA = (Vector2i)(mid - off);
			Vector2i nB = (Vector2i)(mid + off);

			var nodeA = NodeLookup[nA.X, nA.Y];
			Link link;
			if (nodeA.HasLink(NodeLookup[nB.X, nB.Y], out link))
				return link;

			UnityEngine.Debug.LogError("Nothing found, wrong indecies");
			return null;
		}


		/// <summary>
		/// Only works with diagonal lines
		/// </summary>
		//public static void SwapIndicies(Link origin, out Vector2i indA, out Vector2i indB)
		//{
		//	Vector2i diff = origin.B.Index - origin.A.Index;
		//	indA = origin.A.Index + new Vector2i(diff.X, 0);
		//	indB = origin.B.Index + new Vector2i(-diff.X, 0);
		//}

		public bool IsInRange(int x, int y)
		{
			return x >= 0 && x < Width && y >= 0 && y < Length;
		}
		public bool IsInRange(RectBounds rect)
		{
			return rect.X >= 0 && rect.Maximum.X < Width && rect.Y >= 0 && rect.Maximum.Y < Length;
		}
		public bool IsInRange(Vector2i id)
		{
			return IsInRange(id.X, id.Y);
		}

		public bool AreaInRange(RectBounds rect)
		{
			return rect.X >= 0 && rect.Y >= 0 && rect.Maximum.X < Width && rect.Maximum.Y < Length;
		}
		public bool VertexInRange(Vector2i index)
		{
			return index.X >= 0 && index.Y >= 0 && index.X < VertexWidth && index.Y < VertexLength;
		}
		public void ClampID(ref int x, ref int y)
		{
			x = x < 0 ? 0 : (x >= Width ? Width - 1 : x);
			y = y < 0 ? 0 : (y >= Length ? Length - 1 : y);
		}
		public void ClampID(ref Vector2i id)
		{
			id.X = id.X < 0 ? 0 : (id.X >= Width ? Width - 1 : id.X);
			id.Y = id.Y < 0 ? 0 : (id.Y >= Length ? Length - 1 : id.Y);
		}
		public Vector2i ClampID(int x, int y)
		{
			return new Vector2i(x < 0 ? 0 : (x >= Width ? Width - 1 : x), y < 0 ? 0 : (y >= Length ? Length - 1 : y));
		}
		public Vector2i ClampID(Vector2i id)
		{
			id.X = id.X < 0 ? 0 : (id.X >= Width ? Width - 1 : id.X);
			id.Y = id.Y < 0 ? 0 : (id.Y >= Length ? Length - 1 : id.Y);
			return id;
		}

	}
}