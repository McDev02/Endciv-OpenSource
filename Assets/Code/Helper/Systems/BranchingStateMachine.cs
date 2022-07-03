using System.Collections.Generic;

namespace Endciv
{
	public class BranchingStateMachine<T> : SimpleStateMachine<T>, ISaveable, ILoadable<StateMachineSaveData>
	{
		Dictionary<string, string> OnSuccessStates;
		Dictionary<string, string> OnFailureStates;
		string lastState = null;

		public BranchingStateMachine() : base()
		{
			OnSuccessStates = new Dictionary<string, string>();
			OnFailureStates = new Dictionary<string, string>();
		}
		public BranchingStateMachine(int v) : base(v)
		{
			OnSuccessStates = new Dictionary<string, string>(v);
			OnFailureStates = new Dictionary<string, string>(v);
		}

		public void AddState(string state, T action, string successState = null, string failureState = null)
		{
			if (!States.ContainsKey(state))
			{
				lastState = state;
				States.Add(state, action);
				if (successState != null)
					OnSuccessStates.Add(state, successState);
				if (failureState != null)
					OnFailureStates.Add(state, failureState);
			}
			else UnityEngine.Debug.LogError("State (" + state + ") already registered! Choose a unique name!");
		}
		public void AddNextState(string state, T action, string successState = null, string failureState = null)
		{
			if (lastState != null)
			{
				if (!OnSuccessStates.ContainsKey(lastState))
					OnSuccessStates.Add(lastState, state);
			}
			lastState = state;
			if (!States.ContainsKey(state))
			{
				States.Add(state, action);
				if (successState != null)
					OnSuccessStates.Add(state, successState);
				if (failureState != null)
					OnFailureStates.Add(state, failureState);
			}
			else UnityEngine.Debug.LogError("State (" + state + ") already registered! Choose a unique name!");
		}

		internal string GetOnSuccessState()
		{
			if (OnSuccessStates.ContainsKey(CurrentState))
				return OnSuccessStates[CurrentState];
			else return null;
		}
		internal string GetOnFailureState()
		{
			if (OnFailureStates.ContainsKey(CurrentState))
				return OnFailureStates[CurrentState];
			else return null;
		}

		public ISaveable CollectData()
		{
			var data = new StateMachineSaveData();
			data.currentState = CurrentState;
			data.lastState = lastState;
			return data;
		}

		public void ApplySaveData(StateMachineSaveData data)
		{
			if (data == null)
				return;
			lastState = data.lastState;
			SetState(data.currentState);

		}
	}
}