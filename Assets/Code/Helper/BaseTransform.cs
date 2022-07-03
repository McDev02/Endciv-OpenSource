using UnityEngine;
namespace Endciv
{
	public class BaseTransform : MonoBehaviour
	{
		public Transform cachedTransform;

		public virtual void Awake()
		{
			cachedTransform = transform;
		}
	}
}