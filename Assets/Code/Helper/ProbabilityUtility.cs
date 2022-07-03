using System.Collections.Generic;

namespace Endciv
{
	public class ProbabilityUtility<T>
	{
		List<KeyValuePair<T, float>> m_Items = new List<KeyValuePair<T, float>>();
		float m_FullChances;

		public void RegisterItem(T item, float frequency)
		{
			m_Items.Add(new KeyValuePair<T, float>(item, frequency));
			m_FullChances += frequency;
		}

		public T PickItem()
		{
			float baseId = 0;
			float seed = UnityEngine.Random.Range(0, m_FullChances);

			for (int i = 0; i < m_Items.Count; i++)
			{
				var item = m_Items[i];
				float chance = item.Value;
				if (seed >= baseId && seed < baseId + chance)
				{
					return item.Key;
				}

				baseId += chance;
			}
			return default(T);
		}
	}
}