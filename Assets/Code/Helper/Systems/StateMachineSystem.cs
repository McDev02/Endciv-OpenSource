using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	internal class StateMachineSystem<SID>
	where SID : struct
	{
		public interface IState
		{
			SID ID { get; }

			void DoBeforeEntering();

			void DoBeforeLeaving();

			void Process(object context);
		}

		public abstract class State : IState
		{
			private SID m_StateID;

			public SID ID
			{
				get { return m_StateID; }
			}

			public State(SID stateID)
			{
				m_StateID = stateID;
			}

			public virtual void DoBeforeEntering()
			{
			}

			public virtual void DoBeforeLeaving()
			{
			}

			public abstract void Process(object context);
		}

		public class EmptyState : State
		{
			public EmptyState(SID stateID)
				: base(stateID)
			{
			}

			public override void Process(object context)
			{
			}
		}

		private IState m_CurrentState;
		private List<IState> m_States = new List<IState>();

		public SID CurrentStateID
		{
			get { return m_CurrentState.ID; }
		}

		public IState CurrentState
		{
			get { return m_CurrentState; }
		}

		public void AddState(IState state, bool defaultState = false)
		{
			// Check for Null reference before deleting
			if (state == null)
			{
				Debug.LogError("FSM ERROR: Null reference is not allowed");
				return;
			}

			var equals = EqualityComparer<SID>.Default;

			// Add the state to the List if it's not inside it
			var list = m_States;
			for (int i = 0, c = list.Count; i < c; i++)
			{
				var item = list[i];
				if (equals.Equals(item.ID, state.ID))
				{
					Debug.LogError("FSM ERROR: Impossible to add state " + state.ID.ToString() +
								   " because state has already been added");
					return;
				}
			}

			// First State inserted is also the Initial state,
			//   the state the machine is in when the simulation begins
			if (defaultState || m_States.Count == 0)
			{
				m_CurrentState = state;
				m_CurrentState.DoBeforeEntering();
			}
			m_States.Add(state);
		}

		public void DeleteState(SID stateID)
		{
			var equals = EqualityComparer<SID>.Default;

			// Search the List and delete the state if it's inside it
			var list = m_States;
			for (int i = 0, c = list.Count; i < c; i++)
			{
				var state = list[i];
				if (equals.Equals(state.ID, stateID))
				{
					m_States.Remove(state);
					return;
				}
			}
			Debug.LogError("FSM ERROR: Impossible to delete state " + stateID.ToString() +
						   ". It was not on the list of states");
		}

		public void ChangeState(SID stateID)
		{
			var equals = EqualityComparer<SID>.Default;

			var list = m_States;
			for (int i = 0, c = list.Count; i < c; i++)
			{
				var state = list[i];
				if (equals.Equals(state.ID, stateID))
				{
					// Do the post processing of the state before setting the new one
					m_CurrentState.DoBeforeLeaving();

					m_CurrentState = state;

					// Reset the state to its desired condition before it can reason or act
					m_CurrentState.DoBeforeEntering();
					break;
				}
			}
		}

		public void Process(object context)
		{
			m_CurrentState.Process(context);
		}
	}
}