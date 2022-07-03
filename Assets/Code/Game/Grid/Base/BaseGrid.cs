using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// This is the root for all calculations and operations on the grid.
	/// The BaseGridModel is shared with the pathfinder and the controllers to manipulate and generate the grid.
	/// 
	/// All kind of grids such as rectangular, hexagonal and multi dimensional freeform grids can be generated using the base model.
	/// </summary>
	public abstract class BaseGrid
	{
		internal Dictionary<int, Node> Nodes;
		internal Dictionary<int, Link> Links;

		internal Stack<Node> CachedNodes;
		internal Stack<Link> CachedLinks;

		public int NodeCount { get { return Nodes.Count; } }
		public int LinkCount { get { return Links.Count; } }

		protected Stack<int> FreeGroupIDs;
		protected int HighestFreeGroupID;
		/// <summary>
		/// All Nodes of an island ID
		/// </summary>
		public Dictionary<int, List<Node>> NodeGroups;

		public BaseGrid()
		{
			Nodes = new Dictionary<int, Node>();
			Links = new Dictionary<int, Link>();
			NodeGroups = new Dictionary<int, List<Node>>(4);
			FreeGroupIDs = new Stack<int>(4);
			HighestFreeGroupID = 0;

			CachedNodes = new Stack<Node>();
			CachedLinks = new Stack<Link>();
		}

		internal void ClearGroups()
		{
			NodeGroups.Clear();
			FreeGroupIDs.Clear();
			HighestFreeGroupID = 0;

			List<Node> allnodes = new List<Node>(Nodes.Count);
			Node node;
			for (int i = 0; i < Nodes.Count; i++)
			{
				node = Nodes[i];
				node.GroupID = HighestFreeGroupID;
				allnodes.Add(node);
			}

			NodeGroups.Add(HighestFreeGroupID, allnodes);
			HighestFreeGroupID++;
		}

		protected int GetNewGroupID()
		{
			if (FreeGroupIDs.Count > 0) return FreeGroupIDs.Pop();
			return HighestFreeGroupID;
		}

		public void FloodFillAll()
		{
			if (NodeCount <= 0) return;
			foreach (var group in NodeGroups)
			{
				FreeGroupIDs.Push(group.Key);
			}
			NodeGroups.Clear();

			//Setup
			List<Node> remainingNodes = new List<Node>(NodeCount);

			var nodes = Nodes.ToList();
			Node node;
			for (int i = 0; i < nodes.Count; i++)
			{
				node = nodes[i].Value;
				if (node.LayerID >= 0)
					remainingNodes.Add(node);
			}

			if (remainingNodes.Count > 0)
				FloodFillOperation(remainingNodes);
		}

		protected void FloodFillOperation(List<Node> remainingNodes)
		{
			List<int> openGroups;
			List<Node> visitedNodes = new List<Node>(NodeCount);
			int count = 0;

			while (remainingNodes.Count > 0)
			{
				if (count++ > 10)
				{
					UnityEngine.Debug.Log("Too many iterations");
					break;
				}
				var newGroupID = GetNewGroupID();
				var batch = FloodFill(remainingNodes[0], remainingNodes, visitedNodes, newGroupID, out openGroups);
				if (openGroups.Count > 0)
				{
					//Integrate batch in adjacent group
					for (int i = 0; i < openGroups.Count; i++)
					{
						if (i == 0) MergeNodeGroups(openGroups[0], batch);
						else MergeNodeGroups(openGroups[0], openGroups[i]);
					}
				}
				else
				{
					//Create New Group
					if (HighestFreeGroupID <= newGroupID) HighestFreeGroupID = newGroupID + 1;
					NodeGroups.Add(newGroupID, batch);
				}
			}
		}

		List<Node> FloodFill(Node startNode, List<Node> remainingNodes, List<Node> visitedNodes, int groupID, out List<int> openGroups)
		{
			openGroups = new List<int>();

			if (remainingNodes.Contains(startNode))
				remainingNodes.Remove(startNode);
			visitedNodes.Add(startNode);

			List<Node> found = new List<Node>();
			Stack<Node> seekers = new Stack<Node>();
			seekers.Push(startNode);
			//Todo: handle non passable, startnode is null
			startNode.GroupID = groupID;
			found.Add(startNode);

			int count = 0;
			Node seeker;
			while (seekers.Count > 0 && remainingNodes.Count > 0)
			{
				if (count++ > 10000)
				{
					UnityEngine.Debug.Log("Too many iterations");
					break;
				}
				seeker = seekers.Pop();
				Node other;
				for (int i = 0; i < seeker.Links.Count; i++)
				{
					other = seeker.Links[i].GetOther(seeker);
					if (!remainingNodes.Contains(other))
					{
						if (!visitedNodes.Contains(other) && other.GroupID >= 0)
						{
							if (!openGroups.Contains(other.GroupID))
								openGroups.Add(other.GroupID);
						}
						continue;
					}
					remainingNodes.Remove(other);
					visitedNodes.Add(other);
					seekers.Push(other);
					other.GroupID = groupID;
					found.Add(other);
				}
			}

			return found;
		}

		protected void MergeNodeGroups(int groupID, List<Node> nodes)
		{
#if SAFE_DEBUG
			if (!NodeGroups.ContainsKey(groupID)) UnityEngine.Debug.LogError("MergeNodeGroups(): Node Group groupID: " + groupID + " is not defined.");
#endif
			var a = NodeGroups[groupID];
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].GroupID = groupID;
			}
			a.AddRange(nodes);
		}

		protected void MergeNodeGroups(int idA, int idB)
		{
#if SAFE_DEBUG
			if (!NodeGroups.ContainsKey(idA)) UnityEngine.Debug.LogError("MergeNodeGroups(): Node Group idA: " + idA + " is not defined.");
			if (!NodeGroups.ContainsKey(idB)) UnityEngine.Debug.LogError("MergeNodeGroups(): Node Group idB: " + idB + " is not defined.");
#endif

			var a = NodeGroups[idA];
			var b = NodeGroups[idB];

			for (int i = 0; i < b.Count; i++)
			{
				b[i].GroupID = idA;
			}
			a.AddRange(b);
			NodeGroups.Remove(idB);
		}

		internal Node AddNode(Vector3 coords)
		{
			Node node;
			if (CachedNodes.Count > 0)
			{
				node = CachedNodes.Pop();
				node.Setup(coords);
			}
			else
				node = new Node(Nodes.Count, coords);

			Nodes.Add(node.ID, node);
			return node;
		}
		internal Link CreateLink(Node A, Node B)
		{
			for (int i = 0; i < A.Links.Count; i++)
			{
				if (A.Links[i].HasConnection(B))
					return Links[A.Links[i].ID];
			}
			Link link;
			if (CachedLinks.Count > 0)
			{
				link = CachedLinks.Pop();
				link.Setup(A, B);
			}
			else
				link = new Link(Links.Count, A, B);

			A.Links.Add(link);
			B.Links.Add(link);
			Links.Add(link.ID, link);
			return link;
		}

		internal void RemoveNode(int id)
		{
			if (!Nodes.ContainsKey(id))
			{
				UnityEngine.Debug.LogError("Node ID " + id.ToString() + " is not registered");
				return;
			}
			var node = Nodes[id];
			//UnityEngine.Debug.Log("Remove Node " + id.ToString() + " : " + node.ID.ToString());
			Nodes.Remove(id);
			CachedNodes.Push(node);

			int c = node.Links.Count;
			for (int i = 0; i < c; i++)
			{
				RemoveLink(node.Links[0].ID);
			}
		}

		internal void RemoveLink(int id)
		{
			if (!Links.ContainsKey(id))
			{
				UnityEngine.Debug.LogError("Link ID " + id.ToString() + " is not registered");
				return;
			}
			var link = Links[id];
			//UnityEngine.Debug.Log("Remove Link " + id.ToString() + " : " + link.ID.ToString() + "(" + link.A.ID.ToString() + " - " + link.B.ID.ToString() + ")");
			link.A.Links.Remove(link);
			link.B.Links.Remove(link);
			Links.Remove(id);
			CachedLinks.Push(link);
		}

		internal void RemoveLink(Node node, Node other)
		{
			int id;
			if (node.HasLink(other, out id)) RemoveLink(id);
		}


	}

	public class Node
	{
		public int ID;
		public int GroupID;
		public int LayerID;
		public Vector2i Index;
		public Vector3 Coordinates;
		public List<Link> Links;
		public int LinkCount { get { return Links.Count; } }

		public Node()
		{
			Links = new List<Link>(4);
			GroupID = -1;
			LayerID = 0;
		}
		public Node(int id) : this()
		{
			ID = id;
			Coordinates = Vector3.zero;
		}
		public Node(int id, Vector3 coords) : this()
		{
			ID = id;
			Coordinates = coords;
		}
		public void Setup(Vector3 coords)
		{
			Coordinates = coords;
		}

		public bool HasLink(Node other)
		{
			for (int i = 0; i < Links.Count; i++)
			{
				if (Links[i].HasConnection(other)) return true;
			}
			return false;
		}
		public bool HasLink(Node other, out int linkID)
		{
			linkID = -1;
			for (int i = 0; i < Links.Count; i++)
			{
				if (Links[i].HasConnection(other))
				{
					linkID = Links[i].ID;
					return true;
				}
			}
			return false;
		}
		public bool HasLink(Node other, out Link link)
		{
			link = null;
			for (int i = 0; i < Links.Count; i++)
			{
				if (Links[i].HasConnection(other))
				{
					link = Links[i];
					return true;
				}
			}
			return false;
		}
	}
	public class Link
	{
		public int ID;
		public Node A;
		public Node B;
		public float Weight;

		public Vector3 Center { get { return (A.Coordinates + B.Coordinates) * (1f / 2f); } }

		public Link() { }
		public Link(int id, Node a, Node b, float weight = 1)
		{
			ID = id;
			A = a;
			B = b;
			Weight = weight;
		}
		public void Setup(Node a, Node b, float weight = 1)
		{
			A = a;
			B = b;
			Weight = weight;
		}

		/// <summary>
		/// Returns the opposite of node A or B, there is no check if node is A or B.
		/// </summary>
		public Node GetOther(Node node)
		{
			return node == A ? B : A;
		}
		public bool HasConnection(Node node)
		{
			return A == node || B == node;
		}
		public float GetDistance()
		{
			return (B.Coordinates - A.Coordinates).magnitude;
		}
	}
}