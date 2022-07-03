using System;
using UnityEngine;

namespace Endciv
{
    public class MouseCursorManager : ResourceSingleton<MouseCursorManager>
    {
        [Serializable]
        public class CursorIcon
        {
            public ECursorType CursorType;
            public Texture2D Normal;
            public Texture2D Active;
            public Vector2 hotSpot;
        }

        [Serializable]
        public enum ECursorType
        {
            Default,
            Select,
            Grab,
            Cut,
            Build,
            Attack,
            Demolish,
            Watering,
            Collect,
			Reservation,
            MAX
        }

        [Serializable]
        public enum ECursorState
        {
            Normal,
            Active
        }

        ECursorType m_CurrentType;
        ECursorState m_CurrentState;

        [SerializeField]
        public CursorIcon[] CursorIcons = ArrayUtil<CursorIcon>.Empty;

        public void SetCurrentCursor(ECursorType cursorType, ECursorState cursorState)
        {
            if (m_CurrentType == cursorType && m_CurrentState == cursorState) return;

            m_CurrentType = cursorType;
            m_CurrentState = cursorState;
            CursorIcon icon = null;
            for (int i = 0; i < CursorIcons.Length; i++)
            {
                if (CursorIcons[i].CursorType == cursorType)
                {
                    icon = CursorIcons[i];
                    break;
                }
            }
            if (icon == null)
            {
                Debug.Log(ToString() + " : Icon not found.");
                return;
            }
            switch (cursorState)
            {
                case ECursorState.Normal:
                    if (icon.Normal != null)
                    {
                        Cursor.SetCursor(icon.Normal, icon.hotSpot, CursorMode.Auto);
                    }
                    break;

                case ECursorState.Active:
                    if (icon.Active != null)
                    {
                        Cursor.SetCursor(icon.Active, icon.hotSpot, CursorMode.Auto);
                    }
                    break;
            }
        }

		internal void HideCursor()
		{
			Cursor.visible = false;
		}

		internal void ShowCursor()
		{
			Cursor.visible = true;
		}
	}
}