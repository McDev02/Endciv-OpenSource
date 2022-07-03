using System;

namespace Endciv
{
    public class ComplexNotification : NotificationBase
    {
        private Func<bool> triggerPredicate;
        private Func<bool> completionPredicate;

        public ComplexNotification(Func<bool> triggerPredicate, Func<bool> completionPredicate)
        {
            this.triggerPredicate = triggerPredicate;
            this.completionPredicate = completionPredicate;
        }

        public override bool CheckTriggered()
        {
            if (triggerPredicate == null)
                return false;
            return triggerPredicate.Invoke();
        }

        public override bool CheckComplete()
        {
            if (completionPredicate == null)
                return false;
            return completionPredicate.Invoke();
        }
    }

}
