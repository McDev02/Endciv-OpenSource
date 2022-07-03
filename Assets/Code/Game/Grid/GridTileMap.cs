using System;
using UnityEngine;

namespace Endciv
{
    [Serializable]
    public class GridTileMap<T> 
    {
        [SerializeField] public T[] Single;
        [SerializeField] public T[] Strip;
        [SerializeField] public T[] Line;
        [SerializeField] public T[] Corner;
        [SerializeField] public T[] Tris;
        [SerializeField] public T[] Cross;

        public void GetTile(GridMap.ETile tileID, out T obj, int id = -1)
        {
            switch (tileID)
            {
                case GridMap.ETile.Single:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Single.Length);
                        obj = Single[id];
                        break;
                    }
                case GridMap.ETile.Strip:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Strip.Length);
                        obj = Strip[id];
                        break;
                    }
                case GridMap.ETile.Line:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Line.Length);
                        obj = Line[id];
                        break;
                    }
                case GridMap.ETile.Corner:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Corner.Length);
                        obj = Corner[id];
                        break;
                    }
                case GridMap.ETile.Tris:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Tris.Length);
                        obj = Tris[id];
                        break;
                    }
                case GridMap.ETile.Cross:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Cross.Length);
                        obj = Cross[id];
                        break;
                    }
                default:
                    {
                        if (id < 0)
                            id = CivRandom.Range(0, Single.Length);
                        obj = Single[id];
                        break;
                    }
            }
        }

        public int GetID(bool north, bool east, bool south, bool west)
        {
            int id = 0;
            if (north) id += 1;
            if (east) id += 2;
            if (south) id += 4;
            if (west) id += 8;
            return id;
        }

        public void GetTile(int ID, out T obj, out Quaternion rotation)
        {
            switch (ID)
            {
                case 0:
                    GetTile(GridMap.ETile.Single, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                case 1:
                    GetTile(GridMap.ETile.Strip, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                case 2:
                    GetTile(GridMap.ETile.Strip, out obj);
                    rotation = Quaternion.AngleAxis(90, Vector3.up);
                    break;

                case 3:
                    GetTile(GridMap.ETile.Corner, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                case 4:
                    GetTile(GridMap.ETile.Strip, out obj);
                    rotation = Quaternion.AngleAxis(180, Vector3.up);
                    break;

                case 5:
                    GetTile(GridMap.ETile.Line, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                case 6:
                    GetTile(GridMap.ETile.Corner, out obj);
                    rotation = Quaternion.AngleAxis(90, Vector3.up);
                    break;

                case 7:
                    GetTile(GridMap.ETile.Tris, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                case 8:
                    GetTile(GridMap.ETile.Strip, out obj);
                    rotation = Quaternion.AngleAxis(270, Vector3.up);
                    break;

                case 9:
                    GetTile(GridMap.ETile.Corner, out obj);
                    rotation = Quaternion.AngleAxis(270, Vector3.up);
                    break;

                case 10:
                    GetTile(GridMap.ETile.Line, out obj);
                    rotation = Quaternion.AngleAxis(90, Vector3.up);
                    break;

                case 11:
                    GetTile(GridMap.ETile.Tris, out obj);
                    rotation = Quaternion.AngleAxis(270, Vector3.up);
                    break;

                case 12:
                    GetTile(GridMap.ETile.Corner, out obj);
                    rotation = Quaternion.AngleAxis(180, Vector3.up);
                    break;

                case 13:
                    GetTile(GridMap.ETile.Tris, out obj);
                    rotation = Quaternion.AngleAxis(180, Vector3.up);
                    break;

                case 14:
                    GetTile(GridMap.ETile.Tris, out obj);
                    rotation = Quaternion.AngleAxis(90, Vector3.up);
                    break;

                case 15:
                    GetTile(GridMap.ETile.Cross, out obj);
                    rotation = Quaternion.AngleAxis(0, Vector3.up);
                    break;

                default:
                    {
                        GetTile(GridMap.ETile.Single, out obj);
                        rotation = Quaternion.identity;
                        break;
                    }
            }
        }
    }
}