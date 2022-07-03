using System.Collections;
using UnityEngine;

namespace Endciv
{
	public static class CoroutineRunner
	{
		private class Iterator : MonoBehaviour
		{
			public void SetCoroutine(IEnumerator enumerator)
			{
				StartCoroutine(CoroutineExecutor(enumerator));
			}

			private IEnumerator CoroutineExecutor(IEnumerator enumerator)
			{
				yield return StartCoroutine(enumerator);
				Destroy(gameObject);
			}
		}

		public static void StartCoroutine(IEnumerator coroutine)
		{
			var go = new GameObject("CoroutineRunner");
			//go.hideFlags = HideFlags.HideInHierarchy;
			var runner = go.AddComponent<Iterator>();			
			runner.SetCoroutine(coroutine);
		}
		
	}
}