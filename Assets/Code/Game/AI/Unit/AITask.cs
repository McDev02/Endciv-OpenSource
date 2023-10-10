using System;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// AI Task
	/// </summary>
	public abstract class AITask : ISaveable, ILoadable<TaskSaveData>
	{
		//public delegate void ExecutionDelegate();
		public BranchingStateMachine<AIActionBase> StateTree { get; protected set; }
		public int priority;

		public enum TaskState { Initialized, Running, Failed, Success }
		public TaskState CurrentState { get; private set; }
		public AIActionBase CurrentAction
		{
			get
			{
				if (StateTree == null) return null;
				return StateTree.CurrentAction;
			}
		}
		private Dictionary<string, object> globalMembers = new Dictionary<string, object>();

		protected BaseEntity Entity;

		public IAIJob job;
		public EWorkerType workerType;

		public abstract void Initialize();

		public abstract void InitializeStates();

		public AITask() { }
		public AITask(BaseEntity entity)
		{
			Entity = entity;
			var aIAgent = entity.GetFirstAIFeature();
			if (aIAgent == null)
				throw new InvalidOperationException("No AI features attached to entity " + entity.GetFeature<EntityFeature>().View.name);
		}

		public AITask(BaseEntity entity, IAIJob job, EWorkerType type)
			: this(entity)
		{
			this.job = job;
			workerType = type;
		}

		protected void SetState(string v)
		{
			CurrentSubState = v;
			StateTree.SetState(v);
			StateTree.CurrentAction.Status = AIActionBase.EStatus.Started;
		}

		public string CurrentSubState { get; protected set; }

		public bool Execute()
		{
			CurrentState = TaskState.Running;

			if (StateTree == null) CurrentState = TaskState.Failed;
			else if (ExecuteAction())
			{
				StateTree.CurrentAction.Reset();
				if (StateTree.CurrentAction.Status != AIActionBase.EStatus.Success)
				{
					string nextState = StateTree.GetOnFailureState();
					if (nextState == null)
					{
						OnFailure();
					}
					else
					{
						SetState(nextState);
					}
				}
				else
				{
					string nextState = StateTree.GetOnSuccessState();
					if (nextState == null)
					{
						OnSuccess();
					}
					else
					{
						SetState(nextState);
					}
				}
				OnStateEnded();
			}

			return CurrentState == TaskState.Running;
		}
		/// <summary>
		/// Returns If Action has finished
		/// </summary>
		protected bool ExecuteAction()
		{
			if (StateTree == null)
			{
				Debug.LogError($"StateTree is null of task: { GetType().ToString()}");
				return true;
			}

			if (StateTree.CurrentAction == null)
				return false;
			if (StateTree.CurrentAction.Status == AIActionBase.EStatus.Started)
			{
				StateTree.CurrentAction.OnStart();
				StateTree.CurrentAction.Status = AIActionBase.EStatus.Running;
			}
			if (StateTree.CurrentAction.Status == AIActionBase.EStatus.Running)
			{
				StateTree.CurrentAction.Update();
				return false;
			}
			return true;
		}

		protected virtual void OnFailure()
		{
			CurrentState = TaskState.Failed;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback?.Invoke(Entity.GetFirstAIFeature());
		}

		protected virtual void OnSuccess()
		{
			CurrentState = TaskState.Success;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback?.Invoke(Entity.GetFirstAIFeature());
		}

		protected virtual void OnStateEnded()
		{
			if (Entity == null || !Entity.HasFeature<UnitFeature>())
				return;
			var unitFeature = Entity.GetFeature<UnitFeature>();
			if (unitFeature.IsCarrying)
				unitFeature.View.SwitchAnimationState(EAnimationState.CarryIdle);
			else
				unitFeature.View.SwitchAnimationState(EAnimationState.Idle);
		}

		#region Member functions used by Actions
		public void SetMemberValue<T>(string name, object value)
		{
			if (globalMembers.ContainsKey(name))
			{
				globalMembers[name] = value;
			}
			else
			{
				globalMembers.Add(name, value);
			}
		}

		public T GetMemberValue<T>(string name)
		{
			if (!globalMembers.ContainsKey(name))
				return default(T);
			else
			{
				var value = globalMembers[name];

				//Handle primivite types (unbox before casting)
				if (value.GetType().IsPrimitive || value is string)
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}

				//If cast type is wrong it will throw an error which has to be fixed on SetMemberValue()
				return (T)value;
			}
		}

		public void RemoveMemberValue(string name)
		{
			if (!globalMembers.ContainsKey(name))
				return;
			globalMembers.Remove(name);
		}

		public void ClearMembers()
		{
			globalMembers.Clear();
		}
		#endregion

		protected void ResumeAction(TaskSaveData taskData)
		{
			InitializeStates();
			//Set starting state
			StateTree.ApplySaveData(taskData.stateMachineSaveData);
			var currentState = taskData.stateMachineSaveData.currentState;
			if (StateTree.States.ContainsKey(currentState))
			{
				var action = StateTree.States[taskData.stateMachineSaveData.currentState];
				if (action != null && taskData.currentAction != null)
				{
					action.ApplyData(taskData.currentAction);
				}
			}
			else
			{
				Debug.Log("State " + currentState + " not found in state tree.");
			}
		}

		public void Cancel()
		{
			OnFailure();
		}

		public ISaveable CollectData()
		{
			TaskSaveData data = new TaskSaveData();
			data.taskType = GetType();
			data.currentAction = CurrentAction.CollectData();
			if (Entity != null)
			{
				data.unitUID = Entity.UID.ToString();
			}
			else
			{
				data.unitUID = string.Empty;
			}
			data.stateMachineSaveData = StateTree.CollectData() as StateMachineSaveData;
			data.currentState = (int)CurrentState;
			data.globalMembers = new Dictionary<string, object>();
			foreach (var pair in globalMembers)
			{
				var value = pair.Value;
				if (value is FeatureBase)
				{
					data.globalMembers.Add(pair.Key, new BaseEntityReference((value as FeatureBase).Entity.UID.ToString(), value.GetType().ToString()));
				}
				else if (value is BaseEntity)
				{
					data.globalMembers.Add(pair.Key, new BaseEntityReference((value as BaseEntity).UID.ToString(), value.GetType().ToString()));
				}
				else if (value is Location)
				{
					data.globalMembers.Add(pair.Key, (value as Location).CollectData());
				}
				else
				{
					data.globalMembers.Add(pair.Key, pair.Value);
				}
			}
			return data;
		}

		public void ApplySaveData(TaskSaveData data)
		{
			if (data == null)
				return;
			if (!string.IsNullOrEmpty(data.unitUID))
			{
				var id = Guid.Parse(data.unitUID);
				if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
				{
					var entity = Main.Instance.GameManager.SystemsManager.Entities[id];
					Entity = entity;
				}
			}
			CurrentState = (TaskState)data.currentState;
			if (data.globalMembers == null || data.globalMembers.Count <= 0)
				return;
			foreach (var pair in data.globalMembers)
			{
				//Key already loaded
				if (globalMembers.ContainsKey(pair.Key))
				{
					Debug.LogError("Duplicate entry detected with key " + pair.Key + ".");
					continue;
				}
				if (pair.Value is BaseEntityReference)
				{
					var entityRef = (BaseEntityReference)pair.Value;
					var refGuid = Guid.Empty;
					if (!string.IsNullOrEmpty(entityRef.entityUID))
						refGuid = Guid.Parse(entityRef.entityUID);
					if (!Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(refGuid))
					{
						Debug.LogError("Entity with id " + entityRef.entityUID + " not found.");
						continue;
					}
					var entity = Main.Instance.GameManager.SystemsManager.Entities[refGuid];
					if (entityRef.type == typeof(BaseEntity).ToString())
					{
						globalMembers.Add(pair.Key, entity);
					}
					else
					{
						var featureType = Type.GetType(entityRef.type);
						if (!entity.HasFeature(featureType))
						{
							Debug.LogError("Entity with id " + entityRef.entityUID + " has no feature of type " + featureType + "!");
							continue;
						}
						var feature = entity.GetFeature(featureType);
						globalMembers.Add(pair.Key, feature);
					}

				}
				else if (pair.Value is LocationSaveData)
				{
					LocationSaveData reference = pair.Value as LocationSaveData;
					var loc = reference.ToLocation();
					globalMembers.Add(pair.Key, loc);
				}
				else
				{
					globalMembers.Add(pair.Key, pair.Value);
				}
			}
			InitializeStates();
			ResumeAction(data);
		}
	}
}