using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Class that calculates the smallest amount of rectangles in a shape
	/// May only work for Quadratic Grids
	/// </summary>
	public class RectanglePacker
	{
		RectGrid Grid;
		int Width;
		int Length;
		int m_DataCount;

		List<RectBounds>[] m_Rectangles;
		bool[][,] m_Vertecies;
		List<Vector2i>[] m_ConcaveVerts;
		List<Line>[] m_CutLines;

		public RectanglePacker(int width, int length, int dataCount)
		{
			Width = width;
			Length = length;
			m_DataCount = dataCount;// * 4;
			Setup();
		}

		struct Line
		{
			public Vector2i A;
			public Vector2i B;
			public int Length { get { return (int)(B - A).Magnitude; } }
			public Vector2i Direction { get { return (B - A).Normalized; } }
			public int Intersections;

			public Line(Vector2i a, Vector2i b)
			{
				A = a;
				B = b;
				Intersections = 0;
			}
		}

		private void Setup()
		{
			m_Rectangles = new List<RectBounds>[m_DataCount];
			m_Vertecies = new bool[m_DataCount][,];
			m_ConcaveVerts = new List<Vector2i>[m_DataCount];
			m_CutLines = new List<Line>[m_DataCount];
			for (int i = 0; i < m_DataCount; i++)
			{
				m_Rectangles[i] = new List<RectBounds>();
				m_Vertecies[i] = new bool[Width + 1, Length + 1];
				m_ConcaveVerts[i] = new List<Vector2i>();
				m_CutLines[i] = new List<Line>();
			}
		}

		public List<RectBounds>[] CalculateRectangles(ref RectGrid grid)
		{
			Grid = grid;
			ResetData();
			ProcessLinks();

			FindConcaveEdges();
			CloseOpposingEdges();

			FindMaximumIndependetSet();
			CutAlongLines();

			CloseEdges();

			CreateRectangles();

			return m_Rectangles;
		}

		void ProcessLinks()
		{
			for (int i = 0; i < Grid.Length; i++)
			{
				var link = Grid.Links[i];
				var nA = Grid.NodePositionLookup[link.A];
				var nB = Grid.NodePositionLookup[link.B];

				if (Grid.Data[nA.X, nA.Y] == Grid.Data[nB.X, nB.Y])
				{
					link.Weight = 1;
				}
				else
					link.Weight = 0;
			}
		}

		void CutAlongLines()
		{
			for (int i = 0; i < m_DataCount; i++)
			{
				for (int d = 0; d < m_CutLines[i].Count; d++)
				{
					CutAlongLine(m_CutLines[i][d]);
				}
			}
		}

		void CutAlongLine(Line line)
		{
			var vA = line.A;
			//var vB = line.B;
			var dir = line.Direction;
			for (int i = 0; i < line.Length; i++)
			{
				Vector2i indexA = vA + dir * i;
				Vector2i indexB = vA + dir * (i + 1);
				var link = Grid.GetVertexLink(indexA, indexB);
				if (link != null)
				{
					link.Weight = 0;
				}
				else
					UnityEngine.Debug.LogError("No Link found, should not happen.");
			}
		}


		void CloseOpposingEdges()
		{
			bool[,] verts;
			List<Vector2i> cVerts;

			for (int i = 0; i < m_DataCount; i++)
			{
				verts = m_Vertecies[i];
				cVerts = m_ConcaveVerts[i];

				//n : n
				while (cVerts.Count > 1)
				{
					IterateConcaveEdges(i, ref cVerts, ref verts);
					cVerts.RemoveAt(0);
				}

				//Recalculation could be prevented.
				m_ConcaveVerts[i].Clear();
				for (int x = 0; x < Width; x++)
				{
					for (int y = 0; y < Length; y++)
					{
						if (verts[x, y]) m_ConcaveVerts[i].Add(new Vector2i(x, y));
					}
				}

				m_Vertecies[i] = verts;
			}
		}

		void IterateConcaveEdges(int dataID, ref List<Vector2i> cVerts, ref bool[,] verts)
		{
			Vector2i vert = cVerts[0];
			for (int i = 1; i < cVerts.Count; i++)
			{
				if (AreEdgesOpposing(vert, cVerts[i], dataID))
				{
					m_CutLines[dataID].Add(new Line(vert, cVerts[i]));
					cVerts.RemoveAt(i);
					return;
				}
			}
		}

		void FindMaximumIndependetSet()
		{
			bool ready = false;
			int count = 0;
			while (!ready && count < 99)
			{
				count++;
				ready = true;
				//for ( int i = 0; i < m_DataCount; i++ )
				{
					if (!RemoveMostIntersections(0))
						ready = false;
				}
			}
			for (int i = 0; i < m_DataCount; i++)
			{
				var cverts = m_ConcaveVerts[i];
				var verts = m_Vertecies[i];
				for (int l = 0; l < m_CutLines[i].Count; l++)
				{
					var line = m_CutLines[i][l];
					cverts.Remove(line.A);
					cverts.Remove(line.B);

					verts[line.A.X, line.A.Y] = false;
					verts[line.B.X, line.B.Y] = false;
				}
				m_ConcaveVerts[i] = cverts;
				m_Vertecies[i] = verts;
			}
		}

		bool RemoveMostIntersections(int dataID)
		{
			List<Line> tmpList;

			if (m_CutLines[dataID].Count <= 1)
				return true;


			var list = new List<Line>(m_CutLines[dataID]);
			tmpList = m_CutLines[dataID];

			for (int l = 0; l < tmpList.Count; l++)
			{
				var line = tmpList[l];
				line.Intersections = 0;
				tmpList[l] = line;
			}

			int cID = 0;
			Vector2 s;
			while (list.Count > 1)
			{
				for (int l = cID + 1; l < tmpList.Count; l++)
				{
					var line = tmpList[l];
					if (CivMath.LineSegmentIntersection(list[0].A, list[0].B, line.A, line.B, out s))
					{
						line.Intersections++;
						tmpList[l] = line;

						line = tmpList[cID];
						line.Intersections++;
						tmpList[cID] = line;
					}
				}
				cID++;
				list.RemoveAt(0);
			}

			tmpList.Sort((a, b) => a.Intersections.CompareTo(b.Intersections));

			if (tmpList[tmpList.Count - 1].Intersections > 0)
			{
				tmpList.RemoveAt(tmpList.Count - 1);
				return false;
			}

			m_CutLines[dataID] = tmpList;

			return true;
		}

		bool AreEdgesOpposing(Vector2i a, Vector2i b, int dataID, bool debug = false)
		{
			Vector2i dir = Vector2i.Zero;

			dir = b - a;
			if (dir.Magnitude == 0 || (dir.X != 0 && dir.Y != 0))
			{
				return false;   //Edges are the same or are no opposite of each other.
			}

			int length = (int)dir.Magnitude;

			dir.X = dir.X == 0 ? 0 : dir.X / Mathf.Abs(dir.X);
			dir.Y = dir.Y == 0 ? 0 : dir.Y / Mathf.Abs(dir.Y);
			Vector2 norm = new Vector2(-dir.Y, dir.X);

			Vector2 start = a + (Vector2)(dir - norm) * 0.2f;

			for (int i = 0; i < length; i++)
			{
				Vector2i tile;
				//Sample Vertex Line (2 GridTiles)
				tile = (Vector2i)start + dir * i;
				if (Grid.Data[tile.X, tile.Y] != dataID) return false;
				tile = (Vector2i)(tile + norm);
				if (Grid.Data[tile.X, tile.Y] != dataID) return false;
			}

			return true;
		}

		void CloseEdges()
		{
			bool[,] verts;
			List<Vector2i> cVerts;
			for (int i = 0; i < m_DataCount; i++)
			{
				verts = m_Vertecies[i];
				cVerts = m_ConcaveVerts[i];
				while (cVerts.Count > 0)
				{
					Vector2i vert = cVerts[0];
					Vector2i vA = vert;
					Vector2i vB;
					int length = Length * 5;
					int tmpLength;
					vB = CastLine(vert, Vector2i.Up, i, out tmpLength);
					if (tmpLength < length && tmpLength > 0)
					{
						vA = vB;
						length = tmpLength;
					}
					vB = CastLine(vert, Vector2i.Left, i, out tmpLength);
					if (tmpLength < length && tmpLength > 0)
					{
						vA = vB;
						length = tmpLength;
					}
					vB = CastLine(vert, Vector2i.Down, i, out tmpLength);
					if (tmpLength < length && tmpLength > 0)
					{
						vA = vB;
						length = tmpLength;
					}
					vB = CastLine(vert, Vector2i.Right, i, out tmpLength);
					if (tmpLength < length && tmpLength > 0)
					{
						vA = vB;
						length = tmpLength;
					}

					var line = new Line(vert, vA);
					m_CutLines[i].Add(line);
					CutAlongLine(line);
					cVerts.RemoveAt(0);
					verts[vert.X, vert.Y] = false;
				}
				m_Vertecies[i] = verts;
				m_ConcaveVerts[i] = cVerts;
			}
		}

		Vector2i CastLine(Vector2i origin, Vector2i direction, int dataID, out int length, bool debug = false)
		{
			Vector2i norm = new Vector2i(-Mathf.Abs(-direction.Y), -Mathf.Abs(direction.X));

			Vector2i start = origin + (Vector2i)((Vector2)(direction - norm) * 0.2f);

			bool negative = (direction.X < 0 || direction.Y < 0);
			int startIndex = negative ? 1 : 0;
			bool startNode = true;

			Vector2i tile = start;
			Vector2i tile2 = start;
			Vector2i lastTile = start;
			//Vector2i lastTile2 = start;

			length = 0;
			while (length < Length + 1)
			{
				//Sample Vertex Line (2 GridTiles)
				tile = start + direction * (length + startIndex);
				if (IsTileInRange(tile)) { if (Grid.Data[tile.X, tile.Y] != dataID || (!startNode && !TileAccessable(tile, lastTile))) break; } else break;

				tile2 = (tile + norm);
				if (IsTileInRange(tile2)) { if (Grid.Data[tile2.X, tile2.Y] != dataID) break; } else break;
				length++;

				lastTile = tile;
				//lastTile2 = tile2;
				startNode = false;
			}
			if (negative) tile = lastTile;
			length = (int)(tile - origin).Magnitude;
			return tile;
		}

		bool TileAccessable(Vector2i to, Vector2i from)
		{
			//m_Graph.m_Data[to.X, to.Y] != m_Graph.m_Data[from.X, from.Y];
			Link link;
			var toNode = Grid.NodeLookup[to.X, to.Y];
			if (toNode.HasLink(Grid.NodeLookup[from.X, from.Y], out link))
			{
				return link.Weight > 0;
			}
			else
			{
				UnityEngine.Debug.LogError("No link found, schould not happen");
				return false;
			}
		}

		bool IsTileInRange(Vector2i i)
		{
			return (i.X >= 0 && i.X < Grid.Width && i.Y >= 0 && i.Y < Grid.Length);
		}

		void ResetData()
		{
			bool[,] verts;
			for (int i = 0; i < m_DataCount; i++)
			{
				m_CutLines[i].Clear();
				m_Rectangles[i].Clear();

				verts = m_Vertecies[i];
				for (int x = 1; x < Grid.VertexWidth - 1; x++)
				{
					for (int y = 1; y < Grid.VertexLength - 1; y++)
					{
						verts[x, y] = false;
					}
				}
				m_Vertecies[i] = verts;
			}

			var count = Grid.LinkCount;
			for (int i = 0; i < count; i++)
			{
				Grid.Links[i].Weight = 1;
			}
		}

		void FindConcaveEdges()
		{
			bool[,] verts;
			List<Vector2i> cVerts;
			for (int i = 0; i < m_DataCount; i++)
			{
				verts = m_Vertecies[i];
				cVerts = m_ConcaveVerts[i];
				cVerts.Clear();
				for (int x = 1; x < Grid.VertexWidth - 1; x++)
				{
					for (int y = 1; y < Grid.VertexLength - 1; y++)
					{
						int count = 0;
						if (Grid.Data[x, y] == i) count++;
						if (Grid.Data[x - 1, y] == i) count++;
						if (Grid.Data[x, y - 1] == i) count++;
						if (Grid.Data[x - 1, y - 1] == i) count++;

						if (count == 3) //Concave?
						{
							verts[x, y] = true;
							cVerts.Add(new Vector2i(x, y));
						}
						else
							verts[x, y] = false;
					}
				}
				m_Vertecies[i] = verts;
				m_ConcaveVerts[i] = cVerts;
			}
		}

		void CreateRectangles()
		{
			int dataID;
			Stack<Vector2i> stackBuffer = new Stack<Vector2i>();
			HashSet<Vector2i> visitedBuffer = new HashSet<Vector2i>();
			int c = 0;
			for (int x = 0; x < Grid.Width; x++)
			{
				for (int y = 0; y < Grid.Length; y++)
				{
					dataID = Grid.Data[x, y];
					RectBounds rect;
					if (FloodFillRectangle(ref visitedBuffer, ref stackBuffer, new Vector2i(x, y), out rect))
					{
						m_Rectangles[dataID].Add(rect); c++;
					}
				}
			}
		}

		bool FloodFillRectangle(ref HashSet<Vector2i> visitedBuffer, ref Stack<Vector2i> stackBuffer, Vector2i start, out RectBounds rect)
		{
			rect = new RectBounds();

			if (!visitedBuffer.Add(start))
				return false;

			stackBuffer.Push(start);

			rect.Minimum = start;
			rect.Maximum = start + Vector2i.One;

			while (stackBuffer.Count > 0)
			{
				Vector2i p = stackBuffer.Pop();
				rect.Minimum.X = rect.Minimum.X > p.X ? p.X : rect.Minimum.X;
				rect.Minimum.Y = rect.Minimum.Y > p.Y ? p.Y : rect.Minimum.Y;
				rect.Maximum.X = rect.Maximum.X < (p.X + 1) ? (p.X + 1) : rect.Maximum.X;
				rect.Maximum.Y = rect.Maximum.Y < (p.Y + 1) ? (p.Y + 1) : rect.Maximum.Y;

				var nodeP = Grid.NodeLookup[p.X, p.Y];
				for (int i = 0; i < nodeP.Links.Count; i++)
				{
					var link = nodeP.Links[i];
					if (link.Weight > 0)
					{
						var other = link.GetOther(nodeP);
						var index = Grid.NodePositionLookup[other];
						if (visitedBuffer.Add(index))
						{
							stackBuffer.Push(index);
						}
					}
				}
			}
			return true;
		}
	}
}