using UnityEngine;
namespace Endciv
{
	public class RandomColorPool : ResourceSingleton<RandomColorPool>
	{
		public Color[] m_Colors;

		public Color GetColor(int id)
		{
			if (id < 0)
				return Color.magenta;

			id = id % m_Colors.Length;
			return m_Colors[id];
		}
	}
}