using System.Collections.Generic;
namespace Endciv
{
	public class SimpleStateMachine<T>
	{
		public string CurrentState { get; private set; }
		public Dictionary<string, T> States { get; protected set; }

		public SimpleStateMachine()
		{
			States = new Dictionary<string, T>();
		}
		public SimpleStateMachine(int v)
		{
			States = new Dictionary<string, T>(v);
		}

		public T CurrentAction { get { return States[CurrentState]; } }

		public void AddState(string state, T action)
		{
			if (!States.ContainsKey(state))
				States.Add(state, action);
		}

		public void SetState(string state)
		{
			if (!States.ContainsKey(state))
				UnityEngine.Debug.LogError($"State {state} does not exist in StateTree");
			else CurrentState = state;
		}
	}
}