using UnityEngine;
using System;
using System.Linq;

namespace Endciv
{
	public static class Utilities
	{
		/// <summary>
		/// Convert 2D to 3D point (xy to x yHeight z)
		/// </summary>
		public static Vector3 To3D(this Vector2 pos, float yHeight = 0)
		{
			return new Vector3(pos.x, yHeight, pos.y);
		}

		///// <summary>
		///// Convert 2D to 3D point (xy to x yHeight z)
		///// </summary>
		//public static Vector3 To3D(this Vector2i pos, float yHeight = 0)
		//{
		//	return new Vector3(pos.X, yHeight, pos.Y);
		//}

		/// <summary>
		/// Convert 3D to 2D point (xyz to xz)
		/// </summary>
		public static Vector2 To2D(this Vector3 pos)
		{
			return new Vector2(pos.x, pos.z);
		}

        /// <summary>
        /// Gets Type attribute by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type)
        {
            var attribute = (T)type.GetCustomAttributes(
                typeof(T), true
            ).FirstOrDefault();
            return attribute;
        }
    }
}