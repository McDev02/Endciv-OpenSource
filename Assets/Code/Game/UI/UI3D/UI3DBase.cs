using UnityEngine;
namespace Endciv
{
    public abstract class UI3DBase : MonoBehaviour
    {
        public Transform anchoredObject { get; protected set; }
        public Vector3 anchoredPosition{ get; protected set; }

		public float HeightOffset { get; protected set; }
        protected bool m_Stationary;

		public virtual void Setup(Transform agent, float heightOffset = 0, bool stationary = false)
		{
			m_Stationary = stationary;
			anchoredObject = agent;
			anchoredPosition = Vector3.zero;
			HeightOffset = heightOffset;

		}
		public virtual void Setup(Vector3 worldPosition, float heightOffset = 0, bool stationary = false)
		{
			m_Stationary = stationary;
			anchoredObject = null;
			anchoredPosition = worldPosition;
			HeightOffset = heightOffset;
		}

		internal void Relocate(Vector3 worldPosition)
		{
			anchoredPosition = worldPosition;
		}

		public abstract void UpdateElement(Vector3 camPos);

        protected abstract void Dispose();
    }
}