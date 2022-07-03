using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUILoadingScreen : GUIAnimatedPanel
	{
		[SerializeField] GUIProgressBar loadingProgress;
		[SerializeField] Text loadingState;

		private LoadingState state;

		public void Setup(LoadingState state)
		{
			this.state = state;
			state.OnStateChanged -= Update;
			state.OnStateChanged += Update;
		}

		public void Update()
		{
			if (state == null)
			{
				return;
			}
			loadingProgress.Value = state.TotalProgress;
			loadingState.text = state.m_CurrentMessage;
		}

		public void Exit()
		{
			state = null;
			gameObject.SetActive(false);
		}
	}
}