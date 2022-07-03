using UnityEngine;
namespace Endciv
{
	public abstract class ResourceSingleton<T> : ScriptableObject
	where T : ScriptableObject
	{
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				if (ReferenceEquals(m_Instance, null))
				{
					m_Instance = Resources.Load<T>("Singletons/" + typeof(T).Name);
#if UNITY_EDITOR
					if (m_Instance == null)
					{
						Debug.LogError("ResourceSingleton Error: Fail load at " + "Singletons/" + typeof(T).Name);
					}
					else
					{
						//Debug.Log("ResourceSingleton Loaded: " + typeof (T).Name);
					}
#endif
					var inst = m_Instance as ResourceSingleton<T>;
					if (inst != null)
					{
						inst.OnInstanceLoaded();
					}
				}
				return m_Instance;
			}
		}

		protected virtual void OnInstanceLoaded()
		{
		}
	}
}