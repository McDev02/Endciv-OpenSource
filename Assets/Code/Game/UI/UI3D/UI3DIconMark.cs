using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
    public class UI3DIconMark : UI3DBase
    {
        public Image m_Icon;
        public Image m_Alert;
        const int maxDistance = 80;
        const int minDistance = 50;
        public bool IsAlertActive { get { return m_Alert.gameObject.activeSelf; } }

        public override void UpdateElement(Vector3 camPos)
        {
            float dist = (transform.position - camPos).magnitude;
            if (dist <= maxDistance)
            {
                m_Icon.gameObject.SetActive(true);
                dist = CivMath.SqrtFast(Mathf.Clamp01((dist - minDistance) * (1f / (maxDistance - minDistance))));
                m_Icon.transform.localScale = Vector3.one * (1 - dist * dist);
            }
            else
            {
                m_Icon.gameObject.SetActive(false);
            }
        }

        public void HideAlert() { m_Alert.gameObject.SetActive(false); }

        protected override void Dispose()
        {
            UI3DFactory.Instance.Recycle(this);
        }
    }
}