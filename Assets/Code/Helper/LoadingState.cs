
using System;

namespace Endciv
{
	public class LoadingState
	{
		public enum EState { LoadScene, MapGenerator, Done }

		private float m_CurrentProgress;
		public string m_CurrentMessage = "Not yet initialized";

		public System.Action OnStateChanged;

		public EState CurrentState
		{
			get;
			private set;
		}

		public float CurrentProgress
		{
			get { return m_CurrentProgress; }
			set { m_CurrentProgress = CivMath.Clamp01(value); }
		}

		public float TotalProgress
		{
			get
			{
				const float SCALE = (1f / (float)EState.Done);
				return ((float)CurrentState + m_CurrentProgress) * SCALE;
			}
		}

		public void SetState(EState state)
		{
			SetMessage("Loading state: " + state.ToString());
			CurrentState = state;
			m_CurrentProgress = 0;
			if (CurrentState >= EState.Done)
			{
				CurrentState = EState.Done - 1;
				m_CurrentProgress = 1f;
			}
		}

		public void SetMessage(string message)
		{
			m_CurrentMessage = message;
			OnStateChanged?.Invoke();

			Logger.Log($"Loading State: {message}");
		}
	}
}