using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// </summary>
	public class GridMapView : MonoBehaviour
	{
		public const float TileSize = 0.5f;
		public const float GridTileSize = 1f;
		public const float GridTileFactor = TileSize / GridTileSize;
		public const float GridTileFactorInv = GridTileSize / TileSize;
		//Derrivations
		public const float InvTileSize = 1f / TileSize;
		public const float HalfTileSize = TileSize / 2f;
		//Derrivations
		public const float InvGridTileSize = 1f / GridTileSize;
		public const float HalfGridTileSize = GridTileSize / 2f;

		private int Width;
		private int Length;
		public int VertexWidth { get { return Width + 1; } }
		public int VertexLength { get { return Length + 1; } }
		bool IsReady;

		public TextureBuffer HighlightMap { get; private set; }
		[SerializeField] public Texture2D HighlightTexture;

		private GridMap GridMap;
		private RectGrid Grid;

		public void Run(GridMap gridMap, RectGrid grid)
		{
			GridMap = gridMap;
			Grid = grid;

			Width = Grid.Width;
			Length = Grid.Length;
			HighlightMap = new TextureBuffer(Width, Length, TextureFormat.ARGB32, false, true, FilterMode.Point, TextureWrapMode.Clamp);
			HighlightTexture = HighlightMap.Texture;
			IsReady = true;
		}

		void LateUpdate()
		{
			if (!IsReady) return;
			HighlightMap.Apply();

			//Clear map iin advacne for next frame.
			if (!HighlightMap.IsClear)
				HighlightMap.Clear();
		}


		internal void DrawHighlight(Vector2i mouseIndex, Color col)
		{
			if (HighlightMap.IsInRange(mouseIndex))
				HighlightMap.SetPixel(mouseIndex, col);
		}
		internal void DrawHighlight(RectBounds currentRect, Color col)
		{
			HighlightMap.SetPixels(currentRect, col);
		}

		#region Conversions

		/// <summary>
		/// Takes world position and returns the closest Tile-ID
		/// </summary>
		public Vector2i SampleTileWorld(Vector2 pos)
		{
			return SampleTileLocal(WorldToLocal(pos));
		}
		/// <summary>
		/// Takes local position and returns the closest Tile-ID
		/// </summary>
		public Vector2i SampleTileLocal(Vector2 pos)
		{
			pos.x = Mathf.Round(pos.x - 0.5f);
			pos.y = Mathf.Round(pos.y - 0.5f);
			return (Vector2i)pos;
		}
		/// <summary>
		/// Takes world position and returns the closest Point`s local position
		/// </summary>
		public Vector2i SamplePointWorld(Vector2 pos)
		{
			return SamplePointWorld(WorldToLocal(pos));
		}
		/// <summary>
		/// Takes local position and returns the closest Point`s local position
		/// </summary>
		public Vector2i SamplePointLocal(Vector2 pos)
		{
			pos.x = Mathf.Round(pos.x);
			pos.y = Mathf.Round(pos.y);
			return (Vector2i)pos;
		}
		/// <summary>
		/// Takes world position and returns the closest Point`s local position
		/// </summary>
		public Vector2 SampleEdgeWorld(Vector2 pos)
		{
			return SampleEdgeLocal(WorldToLocal(pos));
		}
		/// <summary>
		/// Takes local position and returns the closest Point`s local position
		/// </summary>
		public Vector2 SampleEdgeLocal(Vector2 pos)
		{
			int tileX = (int)Mathf.Floor(pos.x);
			int tileY = (int)Mathf.Floor(pos.y);
			pos.x = pos.x - tileX;
			pos.y = pos.y - tileY;

			Vector2 edge;
			if (pos.y <= pos.x)
			{
				if (pos.y <= (1 - pos.x))
					edge = new Vector2(tileX + 0.5f, tileY);
				else
					edge = new Vector2(tileX + 1, tileY + 0.5f);
			}
			else
			{
				if (pos.y <= (1 - pos.x))
					edge = new Vector2(tileX, tileY + 0.5f);
				else
					edge = new Vector2(tileX + 0.5f, tileY + 1);
			}
			return edge;
		}


		public Vector2 GetPointWorldPosition(Vector2i point)
		{
			return new Vector2(point.X * TileSize, point.Y * TileSize);
		}

		public Vector2 GetTileWorldPosition(Vector2i tile)
		{
			return new Vector2(tile.X * TileSize + HalfTileSize, tile.Y * TileSize + HalfTileSize);
		}

		public Vector2 LocalToWorld(Vector2 pos)
		{
			pos.x *= TileSize;
			pos.y *= TileSize;
			return pos;
		}

        public Rect LocalToWorld(RectBounds rectBounds)
        {
            return new Rect(LocalToWorld(rectBounds.BottomLeft), LocalToWorld(rectBounds.Size));            
        }

		public Vector2 WorldToLocal(Vector2 pos)
		{
			pos.x *= InvTileSize;
			pos.y *= InvTileSize;
			return pos;
		}
		#endregion
	}
}