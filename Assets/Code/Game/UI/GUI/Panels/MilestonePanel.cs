using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Endciv
{
	public class MilestonePanel : GUIAnimatedPanel
	{
		[SerializeField] private Text title;
		[SerializeField] private Transform objectiveRoot;
		[SerializeField] private GUIObjectiveListEntry objectivePrefab;

		private List<GUIObjectiveListEntry> objectivePool;
		private List<GUIObjectiveListEntry> objectives;

		private AudioManager audioManager;

		private NotificationSystem notificationSystem;

		public void Setup(NotificationSystem notificationSystem, AudioManager audioManager)
		{
			this.audioManager = audioManager;
			this.notificationSystem = notificationSystem;
			objectivePool = new List<GUIObjectiveListEntry>();
			objectives = new List<GUIObjectiveListEntry>();
			notificationSystem.OnMilestoneUpdated -= UpdateMilestone;
			notificationSystem.OnMilestoneUpdated += UpdateMilestone;
			OnClose();
		}

		public void UpdateMilestone(Milestone milestone, Notification notification)
		{
			if (milestone == null)
			{
				OnClose();
				return;
			}
			OnOpen();
			title.text = milestone.StaticData.Title;
			for (int i = objectives.Count; i < milestone.objectives.Length; i++)
			{
				GUIObjectiveListEntry objective = null;
				if (objectivePool.Count > 0)
				{
					objective = objectivePool[0];
					objectivePool.RemoveAt(0);
				}
				else
				{
					objective = Instantiate(objectivePrefab, objectiveRoot);
				}
				objectives.Add(objective);
				objective.gameObject.SetActive(true);
			}
			for (int i = milestone.objectives.Length; i < objectives.Count; i++)
			{
				var objective = objectives[0];
				objectives.RemoveAt(0);
				objective.gameObject.SetActive(false);
				objectivePool.Add(objective);
			}
			if (notification == null)
			{
				for (int i = 0; i < milestone.objectives.Length; i++)
				{
					objectives[i].Setup(milestone.objectives[i]);
					objectives[i].transform.SetSiblingIndex(i);
				}
			}
			else
			{
				var uiElement = objectives.FirstOrDefault(x => x.objective == notification);
				if (uiElement != null)
					uiElement.UpdateText();
				audioManager.PlaySound("objectiveCompleted");
			}
		}
	}
}