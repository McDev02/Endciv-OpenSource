using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
    public class NotificationFactory
    {        
        //Static Data
        //Notifications
        internal Dictionary<string, NotificationStaticData> NotificationData { get; private set; }
        //Achievements
        internal Dictionary<string, NotificationStaticData> AchievementData { get; private set; }
        //Objectives
        internal Dictionary<string, NotificationStaticData> ObjectiveData { get; private set; }
        //Milestones
        internal Dictionary<string, MilestoneStaticData> Milestones { get; private set; }

        public NotificationFactory()
        {
            var notificationPool = StaticDataIO.Instance.GetData<NotificationStaticData>("Notifications");
            Milestones = StaticDataIO.Instance.GetData<MilestoneStaticData>("Milestones");

            if (notificationPool == null)
            {
                Debug.LogError("Notification Data could not be loaded!");
            }
            else
            {
                Debug.Log("Achievements and notifications loaded : " + notificationPool.Count);
            }
            NotificationData = new Dictionary<string, NotificationStaticData>();
            AchievementData = new Dictionary<string, NotificationStaticData>();
            ObjectiveData = new Dictionary<string, NotificationStaticData>();
            foreach(var pair in notificationPool)
            {
                switch(pair.Value.notificationType)
                {
                    case ENotificationType.Notification:
                        NotificationData.Add(pair.Key, pair.Value);
                        break;

                    case ENotificationType.Objective:
                        ObjectiveData.Add(pair.Key, pair.Value);
                        break;

                    case ENotificationType.Achievement:
                        AchievementData.Add(pair.Key, pair.Value);
                        break;
                }
            }
        }              

        public Notification CreateNotification(string ID, NotificationSystem system)
        {
            if(!NotificationData.ContainsKey(ID))
            {
                Debug.LogError("Notification " + ID + " not found.");
            }
            var notification = new Notification();
            notification.Setup(system, NotificationData[ID]);
            notification.Run();
            return notification;
        }

        public Notification CreateAchievement(string ID, NotificationSystem system)
        {
            if (!AchievementData.ContainsKey(ID))
            {
                Debug.LogError("Achievement " + ID + " not found.");
            }
            var notification = new Notification();
            notification.Setup(system, AchievementData[ID]);
            notification.Run();
            return notification;
        }

        public Notification CreateObjective(string ID, NotificationSystem system)
        {
            if (!ObjectiveData.ContainsKey(ID))
            {
                Debug.LogError("Objective " + ID + " not found.");
            }
            var notification = new Notification();
            notification.Setup(system, ObjectiveData[ID]);
            return notification;
        }

        public Milestone CreateMilestone(string ID, NotificationSystem system)
        {
            if(!Milestones.ContainsKey(ID))
            {
                Debug.LogError("Milestone " + ID + " not found.");
            }
            var milestone = new Milestone();
            milestone.Setup(Milestones[ID], this, system);
            return milestone;
        }
		
        public Notification[] GetAllNotifications(NotificationSystem system)
        {
            var notifications = new Notification[NotificationData.Count];
            int i = 0;
            foreach(var pair in NotificationData)
            {
                notifications[i] = CreateNotification(pair.Key, system);
                i++;
            }
            return notifications;
        }

        public Notification[] GetAllAchievements(NotificationSystem system)
        {
            var notifications = new Notification[AchievementData.Count];
            int i = 0;
            foreach (var pair in AchievementData)
            {
                notifications[i] = CreateAchievement(pair.Key, system);
                i++;
            }
            return notifications;
        }

        public Notification[] GetAllObjectives(NotificationSystem system)
        {
            var notifications = new Notification[ObjectiveData.Count];
            int i = 0;
            foreach (var pair in ObjectiveData)
            {
                notifications[i] = CreateObjective(pair.Key, system);
                i++;
            }
            return notifications;
        }

        public Milestone[] GetAllMilestones(NotificationSystem system)
        {
            var milestones = new Milestone[Milestones.Count];
            int i = 0;
            foreach (var pair in Milestones)
            {
                milestones[i] = CreateMilestone(pair.Key, system);
                i++;
            }
            return milestones;
        }
		    }
}