using System;
using System.Collections.Generic;
using UnityEngine;

namespace McMeshMerger
{
	public class RectPacker
	{
		int[] sortedIDLookup;
		List<Rect> rectsSorted;
		bool[,] rectmap;
		int width;
		int height;


		internal void PackMaterials(int width, int height, List<MaterialData> materials)
		{
			List<Rect> rects = new List<Rect>(materials.Count);
			for (int i = 0; i < materials.Count; i++)
			{
				rects.Add(new Rect(0, 0, materials[i].bakeWidth, materials[i].bakeHeight));
			}
			PackRects(width, height, rects);
			for (int i = 0; i < rects.Count; i++)
			{
				materials[i].rect = rects[i];
				if (materials[i].hasTexture)
					materials[i].rectRelative = new Rect(rects[i].min.x / width, rects[i].min.y / height, rects[i].width / width, rects[i].height / height);
				else
				{
					var baserect = new Rect(rects[i].min.x / width, rects[i].min.y / height, rects[i].width / width, rects[i].height / height);
					baserect.min = baserect.center;
					baserect.size = Vector2.zero;
					materials[i].rectRelative = baserect;
				}
			}
		}

		internal void PackRects(int width, int height, List<Rect> rects)
		{
			this.width = width;
			this.height = height;
			rectmap = new bool[width, height];
			GetSortedRects(rects);

			for (int i = 0; i < rectsSorted.Count; i++)
			{
				var rect = rectsSorted[i];
				if (TryPlaceRect(ref rect))
				{
					rectsSorted[i] = rect;
					rects[sortedIDLookup[i]] = rect;
				}
				else
				{
					Debug.LogError("Rect #" + i.ToString() + " failed to be placed");
					break;
				}
			}
			/*
			string output = "";
			for (int y = height - 1; y >= 0; y--)
			{
				for (int x = 0; x < width; x++)
				{
					output += " " + (rectmap[x, y] ? "X" : "O") + " ";
				}
				output += "\n";
			}
			Debug.Log(output);
			*/
		}


		bool TryPlaceRect(ref Rect rect)
		{
			int w = (int)rect.width;
			int h = (int)rect.height;
			for (int y = 0; y <= height - h; y++)
			{
				for (int x = 0; x <= width - w; x++)
				{
					if (!IsMapOccupied(x, y, w, h))
					{
						rect.x = x;
						rect.y = y;
						SetMapOccupied(x, y, w, h);
						return true;
					}
				}
			}
			return false;
		}

		bool IsMapOccupied(int x, int y, int width, int height)
		{
			for (int ix = x; ix < x + width; ix++)
			{
				for (int iy = y; iy < y + height; iy++)
				{
					if (rectmap[ix, iy]) return true;
				}
			}
			return false;
		}

		bool SetMapOccupied(int x, int y, int width, int height)
		{
			for (int ix = x; ix < x + width; ix++)
			{
				for (int iy = y; iy < y + height; iy++)
				{
					rectmap[ix, iy] = true;
				}
			}
			return false;
		}

		void GetSortedRects(List<Rect> rects)
		{
			rectsSorted = new List<Rect>(rects.Count);
			sortedIDLookup = new int[rects.Count];
			for (int i = 0; i < rects.Count; i++)
			{
				bool found = false;
				var rect = rects[i];
				float area = rect.width * rect.height;
				for (int s = 0; s < rectsSorted.Count; s++)
				{
					if (area >= rectsSorted[s].width * rectsSorted[s].height)
					{
						found = true;
						rectsSorted.Insert(s, rect);
						AddIndexToSortedList(s);
						sortedIDLookup[s] = i;
						break;
					}
				}
				if (!found)
				{
					AddIndexToSortedList(rectsSorted.Count);
					sortedIDLookup[rectsSorted.Count] = i;
					rectsSorted.Add(rect);
				}
			}
		}
		void AddIndexToSortedList(int start)
		{
			for (int i = sortedIDLookup.Length - 1; i > start; i--)
				sortedIDLookup[i] = sortedIDLookup[i - 1];
		}
	}
}