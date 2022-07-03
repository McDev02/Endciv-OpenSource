
using UnityEngine;
namespace Endciv
{
    /// <summary>
    /// Controller of Grid Map, including Layers and Pathfinding data.
    /// </summary>
    public partial class GridMap : MonoBehaviour
    {
        public enum ETile
        {
            Single,
            Strip,
            Line,
            Corner,
            Tris,
            Cross
        }

        public static int GetID(bool north, bool east, bool south, bool west)
        {
            int id = 0;
            if (north) id += 1;
            if (east) id += 2;
            if (south) id += 4;
            if (west) id += 8;
            return id;
        }
        public static ETile GetTile(bool north, bool east, bool south, bool west, out int rotation)
        {
            return GetTile(GetID(north, east, south, west), out rotation);
        }

        public static ETile GetTile(int ID, out int rotation)
        {
            ETile tile;
            switch (ID)
            {
                case 0:
                    tile = ETile.Single;
                    rotation = 0;
                    break;

                case 1:
                    tile = ETile.Strip;
                    rotation = 0;
                    break;

                case 2:
                    tile = ETile.Strip;
                    rotation = 1;
                    break;

                case 3:
                    tile = ETile.Corner;
                    rotation = 0;
                    break;

                case 4:
                    tile = ETile.Strip;
                    rotation = 2;
                    break;

                case 5:
                    tile = ETile.Line;
                    rotation = 0;
                    break;

                case 6:
                    tile = ETile.Corner;
                    rotation = 1;
                    break;

                case 7:
                    tile = ETile.Tris;
                    rotation = 0;
                    break;

                case 8:
                    tile = ETile.Strip;
                    rotation = 3;
                    break;

                case 9:
                    tile = ETile.Corner;
                    rotation = 3;
                    break;

                case 10:
                    tile = ETile.Line;
                    rotation = 1;
                    break;

                case 11:
                    tile = ETile.Tris;
                    rotation = 3;
                    break;

                case 12:
                    tile = ETile.Corner;
                    rotation = 2;
                    break;

                case 13:
                    tile = ETile.Tris;
                    rotation = 2;
                    break;

                case 14:
                    tile = ETile.Tris;
                    rotation = 1;
                    break;

                case 15:
                    tile = ETile.Cross;
                    rotation = 0;
                    break;

                default:
                    {
                        tile = ETile.Single;
                        rotation = 0;
                        break;
                    }
            }
            return tile;
        }
    }
}