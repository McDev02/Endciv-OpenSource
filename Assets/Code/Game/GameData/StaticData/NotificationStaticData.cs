using UnityEngine;

namespace Endciv
{
    /// <summary>
    /// The objects of this class describe Achievements.
    /// </summary>
    [CreateAssetMenu(fileName = "Notification", menuName = "StaticData/Notification", order = 1)]
    public class NotificationStaticData : BaseStaticData
	{
		public string Title;
        public string IconID;
        public string Description;
        [LocaId]
        public string[] objectiveWindowPages;
        public bool displayObjectiveWindowAutomatically;
        public ENotificationType notificationType;
        public NotificationCondition[] Trigger;
        public NotificationCondition[] Completion;
    }
}