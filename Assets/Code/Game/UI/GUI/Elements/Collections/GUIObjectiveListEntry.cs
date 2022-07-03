using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Endciv
{
	public class GUIObjectiveListEntry : MonoBehaviour
	{
		[SerializeField] private Text text;
		public Notification objective;

		public void Setup(Notification objective)
		{
			this.objective = objective;
			text.color = Color.white;
			text.text = objective.Description;
		}

		internal void UpdateText()
		{
			text.text = objective.Description;
			text.color = objective.status == ENotificationStatus.Complete ? Color.grey : Color.white;			
		}

        public void OnClick()
        {
            Main.Instance.GameManager.GameGUIController.ShowObjectiveWindow(objective.GetPages(), objective.StaticData.Title);
        }
	}
}