using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class LookupQuadTable<T> where T : MonoBehaviour
	{
		public float TileWidth;
		public float TileLength;
		public int TilesX;
		public int TilesY;
		List<T>[] Content;
		public Dictionary<T, int> ContentLookup { get; private set; }

		public LookupQuadTable(float tileSizeX, float tileSizeY, int tilesX, int tilesY)
		{
			TileWidth = tileSizeX;
			TileLength = tileSizeY;
			TilesX = tilesX;
			TilesY = tilesY;

			Content = new List<T>[TilesX * TilesY];
			ContentLookup = new Dictionary<T, int>();
		}

		/// <summary>
		/// Swaps an entry from one Tile to another. There is no error checking, values must be valid.
		/// </summary>
		public void SwapEntry(T entry, int from, int to)
		{
			ContentLookup[entry] = to;
			Content[from].Remove(entry);
			Content[to].Add(entry);
		}

		/// <summary>
		/// Remove Entry from List
		/// </summary>
		public void RemoveEntry(T entry)
		{
			int id = ContentLookup[entry];
			ContentLookup.Remove(entry);
			Content[id].Remove(entry);
		}

		void OnDrawGizmos()
		{
			for (int i = 0; i < Content.Length; i++)
			{
				var tile = Content[i];
				Gizmos.color = RandomColorPool.Instance.GetColor(i);

				for (int e = 0; e < tile.Count; e++)
				{
					Gizmos.DrawSphere(tile[e].transform.position, 0.2f);
				}
			}
		}
	}
}