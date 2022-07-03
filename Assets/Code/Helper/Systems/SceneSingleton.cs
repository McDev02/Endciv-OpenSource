using UnityEngine;
namespace Endciv
{
	public abstract class SceneSingleton<T> : MonoBehaviour
		where T : MonoBehaviour
	{
		private static T m_Instance;

		public static T Instance
		{
			get { return m_Instance; }
		}

		public static bool InstanceExist
		{
			get { return m_Instance != null; }
		}

		protected virtual void Awake()
		{
			if (m_Instance != null)
			{
				Debug.LogError("SceneSingleton Error: " + typeof(T) + " already defined!", this);

				Destroy(this);
				return;
			}
			m_Instance = this as T;

#if UNITY_EDITOR
			Debug.Log("SceneSingleton Init: " + typeof(T), this);
#endif
		}

		protected virtual void OnDestroy()
		{
			m_Instance = null;

#if UNITY_EDITOR
			//Debug.Log("SceneSingleton Delete: " + typeof (T), this);
#endif
		}
	}
}