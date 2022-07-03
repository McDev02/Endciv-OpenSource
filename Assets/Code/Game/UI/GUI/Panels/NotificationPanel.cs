using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class NotificationPanel : GUIAnimatedPanel
	{
		const int MAX_VISIBLE_NOTIFICATIONS = 8;

		AudioManager audioManager;
		[SerializeField] private Transform notificationRoot;
		[SerializeField] private GUINotificationListEntry notificationPrefab;

		private List<GUINotificationListEntry> notificationPool;
		private List<GUINotificationListEntry> notifications;

		private NotificationSystem notificationSystem;

		public void Setup(NotificationSystem notificationSystem, AudioManager audioManager)
		{
			this.audioManager = audioManager;

			this.notificationSystem = notificationSystem;
			notificationPool = new List<GUINotificationListEntry>();
			notifications = new List<GUINotificationListEntry>();
			notificationSystem.OnNotificationComplete -= AddNotification;
			notificationSystem.OnNotificationComplete += AddNotification;
			OnClose();
		}

		public void AddNotification(Notification notification)
		{
			if (notification == null)
				return;
			if (notification.StaticData.notificationType == ENotificationType.Objective)
				return;
			GUINotificationListEntry entry = null;
			if (notifications.Count < MAX_VISIBLE_NOTIFICATIONS)
			{
				if (notificationPool.Count <= 0)
				{
					entry = Instantiate(notificationPrefab, notificationRoot);
				}
				else
				{
					entry = notificationPool[0];
					entry.gameObject.SetActive(true);
					notificationPool.RemoveAt(0);
				}
				notifications.Add(entry);
			}
			else
			{
				entry = notifications.OrderByDescending(x => x.transform.GetSiblingIndex()).FirstOrDefault(x => x.gameObject.activeInHierarchy);
			}
			if (entry == null)
				return;
			entry.transform.SetAsFirstSibling();
			entry.Setup(notification);
			OnOpen();
			audioManager.PlaySound("notificationEvent");
		}

		public void RemoveNotificationEntry(GUINotificationListEntry entry)
		{
			if (notifications.Contains(entry))
			{
				notifications.Remove(entry);
			}
			if (!notificationPool.Contains(entry))
			{
				notificationPool.Add(entry);
			}
			entry.gameObject.SetActive(false);
			if (notifications.Count <= 0)
				OnClose();
		}

	}

}
