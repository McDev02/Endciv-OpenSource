namespace Endciv
{
    public abstract class NotificationBase
    {
        public ENotificationStatus status;
        public abstract bool CheckTriggered();
        public abstract bool CheckComplete();        
    }
}
