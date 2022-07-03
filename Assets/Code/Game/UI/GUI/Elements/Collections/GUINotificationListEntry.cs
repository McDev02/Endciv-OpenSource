using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Endciv
{
    public class GUINotificationListEntry : MonoBehaviour, IPointerClickHandler
    {
        const string ACHIEVEMENT_ICON = "World_Flag";
        const float TIMEOUT_TIMER = 10f;
        [SerializeField] private Image icon;
        [SerializeField] private Text text;

        private Notification notification;
        private float timer;

        public void Setup(Notification notification)
        {
            this.notification = notification;
            var color = Color.white;
            if (notification.StaticData.notificationType == ENotificationType.Achievement)
            {
                color = new Color32(255, 184, 22, 255);
                icon.overrideSprite = Main.Instance.resourceManager.GetIcon(ACHIEVEMENT_ICON, EResourceIconType.Notification);
            }
            else
            {
                icon.overrideSprite = Main.Instance.resourceManager.GetIcon(notification.StaticData.IconID, EResourceIconType.Notification);
            }  
            text.color = color;
            icon.color = color;            
            text.text = notification.Description;
            timer = TIMEOUT_TIMER;
        }

        void Update()
        {
            timer -= Time.unscaledDeltaTime;
            if(timer <= 0)
                Main.Instance.GameManager.GameGUIController.notificationPanel.RemoveNotificationEntry(this);
        }

        public void OnPointerClick(PointerEventData data)
        {
            if(data.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            Main.Instance.GameManager.GameGUIController.notificationPanel.RemoveNotificationEntry(this);
        }
    }

}
