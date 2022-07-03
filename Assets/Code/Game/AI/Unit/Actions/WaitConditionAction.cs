using System;

namespace Endciv
{
	/// <summary>
	/// Wait if condition returns false, finish when it returns true
	/// </summary>
	public class WaitConditionAction : AIAction<ActionSaveData>
	{
		public Func<bool> condition;

		public WaitConditionAction(Func<bool> condition)
		{
		this.condition= condition;
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
		}

		public override void Update()
		{
			if (!condition())
			{
				Status = EStatus.Running;
			}
			else Status = EStatus.Success;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Waiting for condition");
		}
#endif
	}
}