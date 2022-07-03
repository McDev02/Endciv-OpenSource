using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Endciv
{
	public enum ENotificationStatus
	{
		Untriggered,
		Triggered,
		Complete
	}	

	public class Notification : NotificationBase, ISaveable, ILoadable<NotificationSaveData>
	{
		public NotificationStaticData StaticData { get; private set; }

		public NotificationCondition[] Trigger;
		public NotificationCondition[] Completion;

		private NotificationSystem system;

		private Dictionary<Match, string> replacement;
		private string finalDescription = null;
		public string Description
		{
			get
			{
				if (status == ENotificationStatus.Complete && finalDescription != null)
					return finalDescription;
				var description = StaticData.Description;
				var matches = Regex.Matches(description, @"{([^{}]*)}");
				replacement = new Dictionary<Match, string>();
				foreach (Match match in matches)
				{
					if (!replacement.ContainsKey(match))
					{
						var valueName = match.Groups[1].Value;
						if (string.IsNullOrEmpty(valueName)) continue;

						Type type;
						if (system.TryGetType(valueName, out type))
						{
							NotificationCondition condition = null;
							NotificationCondition staticCondition = null;
							if (HasValue(valueName, out condition, out staticCondition))
							{
								switch (condition.valueType)
								{
									case EValueType.Int:
										{
											int currentValue = system.GetVariable<int>(valueName);
											replacement.Add(match, (currentValue - condition.intOffset).ToString());
										}
										break;

									case EValueType.Double:
										{
											double currentValue = system.GetVariable<double>(valueName);
											replacement.Add(match, (currentValue - condition.doubleOffset).ToString());
										}
										break;

									case EValueType.Float:
										{
											float currentValue = system.GetVariable<float>(valueName);
											replacement.Add(match, (currentValue - condition.floatOffset).ToString());
										}
										break;

									default:
										replacement.Add(match, condition.GetValueRaw().ToString());
										break;
								}
							}
							else
							{
								replacement.Add(match, system.GetVariable<object>(valueName).ToString());
							}
						}
						else
						{
							NotificationCondition condition;
							NotificationCondition staticCondition;
							if (HasValue(valueName, out condition, out staticCondition))
							{
								switch (condition.valueType)
								{
									case EValueType.Int:
										replacement.Add(match, "0");
										break;

									case EValueType.Double:
										replacement.Add(match, "0");
										break;

									case EValueType.Float:
										replacement.Add(match, "0");
										break;

									case EValueType.String:
										replacement.Add(match, "null");
										break;

									case EValueType.Bool:
										replacement.Add(match, "false");
										break;

									case EValueType.Vector2:
										replacement.Add(match, Vector2.zero.ToString());
										break;

									case EValueType.Vector3:
										replacement.Add(match, Vector3.zero.ToString());
										break;

									case EValueType.Vector4:
										replacement.Add(match, Vector4.zero.ToString());
										break;

									case EValueType.Vector2Int:
										replacement.Add(match, Vector2Int.zero.ToString());
										break;

									case EValueType.Vector3Int:
										replacement.Add(match, Vector3Int.zero.ToString());
										break;

									default:
										replacement.Add(match, "null");
										break;
								}
							}
							else
							{
								replacement.Add(match, "null");
							}
						}
					}
				}
				foreach (var pair in replacement)
				{
					description = description.Replace(pair.Key.Value, pair.Value);
				}
				if (status == ENotificationStatus.Complete)
					finalDescription = description;
				return description;
			}
		}

		public bool ContainsVariable(string variableName)
		{
			if (Trigger != null)    //prevent pre Run() calls
				for (int i = 0; i < Trigger.Length; i++)
				{
					if (Trigger[i].valueName == variableName)
						return true;
				}
			if (Completion != null) //prevent pre Run() calls
				for (int i = 0; i < Completion.Length; i++)
				{
					if (Completion[i].valueName == variableName)
						return true;
				}
			if (replacement != null)
			{
				foreach (var pair in replacement)
				{
					if (pair.Key.Groups[1].Value == variableName)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HasValue(string valueName, out NotificationCondition condition, out NotificationCondition staticCondition)
		{
			for (int i = 0; i < Completion.Length; i++)
			{
				var trigger = Completion[i];
				if (trigger.valueName == valueName)
				{
					condition = trigger;
					staticCondition = StaticData.Completion[i];
					return true;
				}
			}
			condition = null;
			staticCondition = null;
			return false;
		}

		public void Setup(NotificationSystem system, NotificationStaticData staticData)
		{
			this.system = system;
			StaticData = staticData;
		}

		public void Run()
		{
			if (Trigger == null)
				Trigger = AssignConditions(StaticData.Trigger);
			if (Completion == null)
				Completion = AssignConditions(StaticData.Completion);
		}

		private NotificationCondition[] AssignConditions(NotificationCondition[] target)
		{
			var conditions = new NotificationCondition[target.Length];
			for (int i = 0; i < conditions.Length; i++)
			{
				var condition = target[i].Copy();
				if (condition.isRelative)
				{
					if (condition.valueType == EValueType.Int)
					{
						condition.intValue += system.GetVariable<int>(condition.valueName);
						condition.intOffset = condition.intValue - target[i].intValue;
					}
					else if (condition.valueType == EValueType.Float)
					{
						condition.floatValue += system.GetVariable<float>(condition.valueName);
						condition.floatOffset = condition.floatValue - target[i].floatValue;
					}
					else if (condition.valueType == EValueType.Double)
					{
						condition.doubleValue += system.GetVariable<double>(condition.valueName);
						condition.doubleOffset = condition.doubleValue - target[i].doubleValue;
					}
				}
				conditions[i] = condition;
			}
			return conditions;
		}

		public override bool CheckTriggered()
		{
			return CheckCondition(Trigger);
		}

		public override bool CheckComplete()
		{
			return CheckCondition(Completion);
		}

		private bool CheckCondition(NotificationCondition[] conditions)
		{
			if (conditions == null || conditions.Length <= 0)
				return false;
			foreach (var condition in conditions)
			{
				bool isMatch = false;
				switch (condition.valueType)
				{
					case EValueType.Bool:
						isMatch = CompareValues(system.GetVariable<bool>(condition.valueName), condition.GetValue<bool>(), condition.conditionOperator);
						break;

					case EValueType.Double:
						isMatch = CompareValues(system.GetVariable<double>(condition.valueName), condition.GetValue<double>(), condition.conditionOperator);
						break;

					case EValueType.Float:
						isMatch = CompareValues(system.GetVariable<float>(condition.valueName), condition.GetValue<float>(), condition.conditionOperator);
						break;

					case EValueType.Int:
						isMatch = CompareValues(system.GetVariable<int>(condition.valueName), condition.GetValue<int>(), condition.conditionOperator);
						break;

					case EValueType.String:
						isMatch = CompareValues(system.GetVariable<string>(condition.valueName), condition.GetValue<string>(), condition.conditionOperator);
						break;

					case EValueType.Vector2:
						{
							Vector2 vectorA = system.GetVariable<Vector2>(condition.valueName);
							Vector2 vectorB = condition.GetValue<Vector2>();
							switch (condition.conditionOperator)
							{
								case EConditionOperator.EQUAL:
									isMatch = CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator);
									break;

								case EConditionOperator.NOT_EQUAL:
									isMatch = !(CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator));
									break;

								default:
									isMatch = CompareValues(vectorA.magnitude, vectorB.magnitude, condition.conditionOperator);
									break;
							}

						}
						break;

					case EValueType.Vector3:
						{
							Vector3 vectorA = system.GetVariable<Vector3>(condition.valueName);
							Vector3 vectorB = condition.GetValue<Vector3>();
							switch (condition.conditionOperator)
							{
								case EConditionOperator.EQUAL:
									isMatch = CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator);
									break;

								case EConditionOperator.NOT_EQUAL:
									isMatch = !(CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator));
									break;

								default:
									isMatch = CompareValues(vectorA.magnitude, vectorB.magnitude, condition.conditionOperator);
									break;
							}

						}
						break;

					case EValueType.Vector4:
						{
							Vector4 vectorA = system.GetVariable<Vector4>(condition.valueName);
							Vector4 vectorB = condition.GetValue<Vector4>();
							switch (condition.conditionOperator)
							{
								case EConditionOperator.EQUAL:
									isMatch = CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator) && CompareValues(vectorA.w, vectorB.w, condition.conditionOperator);
									break;

								case EConditionOperator.NOT_EQUAL:
									isMatch = !(CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator) && CompareValues(vectorA.w, vectorB.w, condition.conditionOperator));
									break;

								default:
									isMatch = CompareValues(vectorA.magnitude, vectorB.magnitude, condition.conditionOperator);
									break;
							}

						}
						break;

					case EValueType.Vector2Int:
						{
							Vector2Int vectorA = system.GetVariable<Vector2Int>(condition.valueName);
							Vector2Int vectorB = condition.GetValue<Vector2Int>();
							switch (condition.conditionOperator)
							{
								case EConditionOperator.EQUAL:
									isMatch = CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator);
									break;

								case EConditionOperator.NOT_EQUAL:
									isMatch = !(CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator));
									break;

								default:
									isMatch = CompareValues(vectorA.magnitude, vectorB.magnitude, condition.conditionOperator);
									break;
							}

						}
						break;

					case EValueType.Vector3Int:
						{
							Vector3Int vectorA = system.GetVariable<Vector3Int>(condition.valueName);
							Vector3Int vectorB = condition.GetValue<Vector3Int>();
							switch (condition.conditionOperator)
							{
								case EConditionOperator.EQUAL:
									isMatch = CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator);
									break;

								case EConditionOperator.NOT_EQUAL:
									isMatch = !(CompareValues(vectorA.x, vectorB.x, condition.conditionOperator) && CompareValues(vectorA.y, vectorB.y, condition.conditionOperator) && CompareValues(vectorA.z, vectorB.z, condition.conditionOperator));
									break;

								default:
									isMatch = CompareValues(vectorA.magnitude, vectorB.magnitude, condition.conditionOperator);
									break;
							}

						}
						break;
				}

				if (!isMatch)
				{
					return false;
				}
			}
			return true;
		}


		private bool CompareValues<T>(T first, T second, EConditionOperator conditionOperator) where T : IComparable
		{
			switch (conditionOperator)
			{
				case EConditionOperator.EQUAL:
					return first.CompareTo(second) == 0;

				case EConditionOperator.NOT_EQUAL:
					return first.CompareTo(second) != 0;

				case EConditionOperator.GREATER_EQUAL_THAN:
					return first.CompareTo(second) >= 0;

				case EConditionOperator.GREATER_THAN:
					return first.CompareTo(second) > 0;

				case EConditionOperator.LESS_EQUAL_THAN:
					return first.CompareTo(second) <= 0;

				case EConditionOperator.LESS_THAN:
					return first.CompareTo(second) < 0;

				default:
					return false;
			}
		}

		public ISaveable CollectData()
		{
			var data = new NotificationSaveData();
			data.status = (int)status;
			data.triggers = new List<NotificationConditionSaveData>();
			data.finalDescription = finalDescription;
			foreach (var trigger in Trigger)
			{
				var triggerData = trigger.CollectData();
				if (triggerData == null)
					continue;
				data.triggers.Add((NotificationConditionSaveData)triggerData);
			}
			data.completions = new List<NotificationConditionSaveData>();
			foreach (var completion in Completion)
			{
				var completionData = completion.CollectData();
				if (completionData == null)
					continue;
				data.completions.Add((NotificationConditionSaveData)completionData);
			}
			return data;
		}

		public void ApplySaveData(NotificationSaveData data)
		{
			if (data == null)
				return;
			status = (ENotificationStatus)data.status;
			if (status != ENotificationStatus.Untriggered)
				Run();
			foreach (var trigger in data.triggers)
			{
				var condition = Trigger.FirstOrDefault(x => x.valueName == trigger.valueName);
				if (condition == null)
					continue;
				condition.ApplySaveData(trigger);
			}
			foreach (var completion in data.completions)
			{
				var condition = Completion.FirstOrDefault(x => x.valueName == completion.valueName);
				if (condition == null)
					continue;
				condition.ApplySaveData(completion);
			}
			if (!string.IsNullOrEmpty(data.finalDescription))
			{
				finalDescription = data.finalDescription;
			}
		}

		public string[] GetPages()
		{
			if (StaticData.objectiveWindowPages == null)
				return null;
			string[] pages = new string[StaticData.objectiveWindowPages.Length];
			for (int i = 0; i < pages.Length; i++)
			{
				pages[i] = StaticData.objectiveWindowPages[i];
			}
			return pages;
		}
	}

}
