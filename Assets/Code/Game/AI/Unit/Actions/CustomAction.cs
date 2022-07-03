using System;

namespace Endciv
{
	/// <summary>
	/// Wait if condition returns false, finish when it returns true
	/// </summary>
	public class CustomAction : AIAction<ActionSaveData>
	{
		public Action onStart;
		public Func<bool> onUpdate;

		public CustomAction(Action onStart, Func<bool> onUpdate)
		{
			this.onStart = onStart;
			this.onUpdate = onUpdate;
		}

		public override void Reset()
		{
		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
		}

		public override ISaveable CollectData()
		{
			var data = new ActionSaveData();
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			onStart?.Invoke();
		}

		public override void Update()
		{
			if (onUpdate != null && !onUpdate())
			{
				Status = EStatus.Running;
			}
			else Status = EStatus.Success;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Custom Action");
		}
#endif
	}
}