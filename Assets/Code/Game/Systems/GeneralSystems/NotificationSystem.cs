using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Endciv
{
	public class NotificationSystem : BaseGameSystem, ISaveable, ILoadable<NotificationSystemSaveData>
	{
		private Dictionary<string, object> notificationVariables;

		private Notification[] combinedPool;
		private Notification[] notifications;
		private Notification[] achievements;
		private List<ComplexNotification> complexNotifications;
		private List<ScenarioBase> scenarios;
		private int currentScenarioID = 0;

		private List<string> completedAchievements;

		public Action<Notification> OnNotificationComplete;
		public Action<NotificationBase> OnNotificationStatusChanged;
		public Action<Milestone, Notification> OnMilestoneUpdated;

		private bool propagateMilestoneEvents;

		public void Setup(NotificationFactory factory, ScenarioBase[] scenarioPrefabs)
		{
			complexNotifications = new List<ComplexNotification>();
			notifications = factory.GetAllNotifications(this);
			achievements = factory.GetAllAchievements(this);
			combinedPool = new Notification[notifications.Length + achievements.Length];
			Array.Copy(notifications, combinedPool, notifications.Length);
			Array.Copy(achievements, 0, combinedPool, notifications.Length, achievements.Length);
			completedAchievements = new List<string>(achievements.Length);
			notificationVariables = new Dictionary<string, object>();
			scenarios = new List<ScenarioBase>(scenarioPrefabs.Length);
			for (int i = 0; i < scenarioPrefabs.Length; i++)
			{
				if (scenarioPrefabs[i] == null) continue;
				var scenario = GameObject.Instantiate(scenarioPrefabs[i]);
				scenario.Setup(Main.Instance.GameManager, this);
				scenario.gameObject.SetActive(false);
				scenarios.Add(scenario);
			}


			//StartingTechs
			var c = Main.Instance.GameManager.SystemsManager.ConstructionSystem;
			c.AddTech(ETechnologyType.Crops);
#if DEV_MODE
			c.AddTech(ETechnologyType.TechElectricity);
			c.AddTech(ETechnologyType.TechSolar);
#endif

			if (scenarioPrefabs.Length <= 0)
				c.AddTech(ETechnologyType.BuildingEnabled);
		}
		public override void UpdateStatistics()
		{
		}

		/// <summary>
		/// Used to start scenario objectives
		/// </summary>
		public void Run()
		{
			if (scenarios != null && scenarios.Count > 0)
			{
				scenarios[currentScenarioID].Run();
			}
			IsRunning = true;
		}

		public void StartMilestone(Milestone milestone)
		{
			OnMilestoneUpdated?.Invoke(milestone, null);
		}

		/// <summary>
		/// Used to add or update existing variables related to notification updating
		/// Looked up by notification conditions
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetVariable<T>(string name, object value)
		{
			if (!IsValidType<T>())
				Debug.LogError("Invalid type provided for notification variable " + name);
			if (notificationVariables.ContainsKey(name))
			{
				if (notificationVariables[name] != value)
				{
					notificationVariables[name] = value;
					OnVariableChanged(name);
				}
			}
			else
			{
				if (!value.Equals(default(T)))
				{
					notificationVariables.Add(name, value);
					OnVariableChanged(name);
				}
			}
		}

		public void IncreaseInteger(string name)
		{
			int value;
			if (TryGetVariable(name, out value))
			{
				value++;
				SetVariable<int>(name, value);
			}
		}

		public void DecreaseInteger(string name)
		{
			int value;
			if (TryGetVariable(name, out value))
			{
				value--;
				SetVariable<int>(name, value);
			}
		}


		/// <summary>
		/// Used by notification conditions to get notification variable values to compare
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public T GetVariable<T>(string name)
		{
			if (!IsValidType<T>())
				Debug.LogError("Invalid type provided for notification variable " + name);
			if (!notificationVariables.ContainsKey(name))
				return default(T);
			else
			{
				var value = notificationVariables[name];

				//Handle primivite types (unbox before casting)
				if (value.GetType().IsPrimitive || value is string)
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}
				return (T)value;
			}

		}

		/// <summary>
		/// Functions like GetVariable but in a safe context
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool TryGetVariable<T>(string name, out T result)
		{
			result = default(T);
			if (!IsValidType<T>())
			{
				Debug.LogError("Invalid type provided for notification variable " + name);
				return false;
			}
			if (!notificationVariables.ContainsKey(name))
			{
				return true;
			}
			else
			{
				var value = notificationVariables[name];
				try
				{
					//Handle primivite types (unbox before casting)
					if (value.GetType().IsPrimitive || value is string)
					{
						result = (T)Convert.ChangeType(value, typeof(T));
						return true;
					}
					else
					{
						Debug.LogError("Invalid variable type for " + name + ".");
						return false;
					}
				}
				catch
				{
					Debug.LogError("Error converting " + name + " from object to " + typeof(T));
					return false;
				}
			}
		}

		/// <summary>
		/// Clears up variable from notification variable pool
		/// </summary>
		/// <param name="name"></param>
		public void RemoveVariable(string name)
		{
			if (!notificationVariables.ContainsKey(name))
				return;
			notificationVariables.Remove(name);
			OnVariableChanged(name);
		}

		/// <summary>
		/// Cleans up notification variable pool
		/// </summary>
		public void ClearVariables()
		{
			if (notificationVariables.Count > 0)
			{
				notificationVariables.Clear();
				OnVariableChanged(null);
			}
		}

		/// <summary>
		/// Used to update complex notifications on every frame
		/// that don't depend on notification variables
		/// </summary>
		public override void UpdateGameLoop()
		{
			if (!IsRunning) return;
			if (currentScenarioID >= 0 && scenarios != null && scenarios.Count > currentScenarioID && scenarios[currentScenarioID] != null)
				scenarios[currentScenarioID].UpdateGameLoop();

			foreach (var notification in complexNotifications)
			{
				EvaluateComplexNotification(notification);
			}
		}

		/// <summary>
		/// Event triggered when a notification variable changes in order 
		/// to run notification condition checks
		/// </summary>
		private void OnVariableChanged(string variableName)
		{
			//Debug.Log($"OnVariableChanged: {variableName} value: {notificationVariables[variableName].ToString()}");
			EvaluateNotifications(variableName);
		}

		/// <summary>
		/// Manual constraint method for notification variables
		/// since they can't have the same base type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private bool IsValidType<T>()
		{
			if (typeof(T) == typeof(object) ||
				typeof(T) == typeof(int) ||
				typeof(T) == typeof(string) ||
				typeof(T) == typeof(double) ||
				typeof(T) == typeof(bool) ||
				typeof(T) == typeof(float) ||
				typeof(T) == typeof(Vector2) ||
				typeof(T) == typeof(Vector3) ||
				typeof(T) == typeof(Vector4) ||
				typeof(T) == typeof(Vector2Int) ||
				typeof(T) == typeof(Vector3Int))
				return true;
			return false;
		}

		public bool TryGetType(string variableName, out Type type)
		{
			type = default(Type);
			var value = GetVariable<object>(variableName);
			if (value == null)
				return false;
			type = value.GetType();
			return true;
		}

		/// <summary>
		/// Executed every time one of the notification variables changes value
		/// Responsible to change the status of notifications and trigger the associated event on status change
		/// </summary>
		private void EvaluateNotifications(string variableName)
		{
			foreach (var notification in combinedPool)
			{
				if (!notification.ContainsVariable(variableName))
					continue;
				EvaluateNotification(notification);
			}
			if (scenarios != null && scenarios.Count > 0 && currentScenarioID < scenarios.Count && scenarios[currentScenarioID] != null)
			{
				var currentMilestone = scenarios[currentScenarioID].GetCurrentMilestone();
				propagateMilestoneEvents = true;
				foreach (var objective in currentMilestone.objectives)
				{
					if (!objective.ContainsVariable(variableName))
						continue;
					EvaluateNotification(objective);
				}
				propagateMilestoneEvents = false;
				if (currentMilestone.Status == EMilestoneStatus.Completed)
				{
					currentMilestone.OnMilestoneComplete();
					currentMilestone = scenarios[currentScenarioID].GetΝextMilestone();
					OnMilestoneUpdated?.Invoke(currentMilestone, null);
					if (currentMilestone == null)
					{
						currentScenarioID++;
						if (currentScenarioID < scenarios.Count)
						{
							scenarios[currentScenarioID].Run();
							OnMilestoneUpdated?.Invoke(scenarios[currentScenarioID].GetCurrentMilestone(), null);
						}
					}
					else
					{
						currentMilestone.Run();
						OnMilestoneUpdated?.Invoke(currentMilestone, null);
						StartMilestone(currentMilestone);
					}
				}
			}
		}

		/// <summary>
		/// Returns true if notification is complete
		/// </summary>
		/// <param name="notification"></param>
		/// <returns></returns>
		private bool EvaluateNotification(Notification notification)
		{
			bool result = false;
			switch (notification.status)
			{
				case ENotificationStatus.Untriggered:
					if (notification.CheckTriggered())
					{
						notification.status = ENotificationStatus.Triggered;
						OnNotificationStatusChanged?.Invoke(notification);
						goto case ENotificationStatus.Triggered;
					}
					result = false;
					break;

				case ENotificationStatus.Triggered:
					if (notification.CheckComplete())
					{
						notification.status = ENotificationStatus.Complete;
						OnNotificationComplete?.Invoke(notification);
						OnNotificationStatusChanged?.Invoke(notification);
						if (notification.StaticData.notificationType != ENotificationType.Notification)
						{
							if (!completedAchievements.Contains(notification.StaticData.ID))
								completedAchievements.Add(notification.StaticData.ID);
						}
						if (propagateMilestoneEvents)
						{
							OnMilestoneUpdated?.Invoke(scenarios[currentScenarioID].GetCurrentMilestone(), notification);
						}
						goto case ENotificationStatus.Complete;
					}
					result = false;
					break;

				case ENotificationStatus.Complete:
					if (notification.StaticData.notificationType == ENotificationType.Notification)
					{
						notification.status = ENotificationStatus.Untriggered;
						OnNotificationStatusChanged?.Invoke(notification);
					}
					result = true;
					break;
			}

			return result;
		}

		/// <summary>
		/// Evaluates complex notifications based on their provided functions
		/// </summary>
		/// <param name="notification"></param>
		/// <returns></returns>
		private bool EvaluateComplexNotification(ComplexNotification notification)
		{
			switch (notification.status)
			{
				case ENotificationStatus.Untriggered:
					if (notification.CheckTriggered())
					{
						notification.status = ENotificationStatus.Triggered;
						OnNotificationStatusChanged?.Invoke(notification);
						goto case ENotificationStatus.Triggered;
					}
					return false;

				case ENotificationStatus.Triggered:
					if (notification.CheckComplete())
					{
						notification.status = ENotificationStatus.Complete;
						OnNotificationStatusChanged?.Invoke(notification);
						goto case ENotificationStatus.Complete;
					}
					return false;

				case ENotificationStatus.Complete:
					return true;
			}
			return false;
		}

		public ISaveable CollectData()
		{
			var data = new NotificationSystemSaveData();
			data.currentScenarioID = currentScenarioID;
			data.notifications = new Dictionary<string, NotificationSaveData>();
			foreach (var notification in combinedPool)
			{
				data.notifications.Add(notification.StaticData.ID, (NotificationSaveData)notification.CollectData());
			}
			data.scenarios = new List<ScenarioSaveData>();
			foreach (var scenario in scenarios)
			{
				data.scenarios.Add((ScenarioSaveData)scenario.CollectData());
			}
			data.notificationVariables = new Dictionary<string, object>();
			foreach (var pair in notificationVariables)
			{
				data.notificationVariables.Add(pair.Key, pair.Value);
			}
			return data;
		}

		public void ApplySaveData(NotificationSystemSaveData data)
		{
			notificationVariables = new Dictionary<string, object>();
			if (data == null)
				return;
			currentScenarioID = data.currentScenarioID;
			if (data.notificationVariables != null && data.notificationVariables.Count > 0)
			{
				foreach (var pair in data.notificationVariables)
				{
					//Key already loaded
					if (notificationVariables.ContainsKey(pair.Key))
					{
						Debug.LogError("Duplicate entry detected with key " + pair.Key + ".");
						continue;
					}
					notificationVariables.Add(pair.Key, pair.Value);
				}
			}
			if (data.notifications != null && data.notifications.Count > 0)
			{
				foreach (var pair in data.notifications)
				{
					var notification = combinedPool.FirstOrDefault(x => x.StaticData.ID == pair.Key);
					if (notification == null)
						continue;
					notification.ApplySaveData(pair.Value);
					if (notification.StaticData.notificationType != ENotificationType.Notification && notification.status == ENotificationStatus.Complete)
					{
						completedAchievements.Add(notification.StaticData.ID);
					}
				}
			}
			if (data.scenarios != null && data.scenarios.Count > 0)
			{
				for (int i = 0; i < data.scenarios.Count; i++)
				{
					var scenario = scenarios[i];
					if (scenario == null)
						continue;
					scenario.ApplySaveData(data.scenarios[i]);
				}
			}
		}
	}
}