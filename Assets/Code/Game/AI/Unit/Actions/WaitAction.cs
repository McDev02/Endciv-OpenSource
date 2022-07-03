namespace Endciv
{
	public class WaitAction : AIAction<WaitActionSaveData>
	{
		public float Duration;
		public float timer;

		public WaitAction(float duration)
		{
			Duration = duration;
		}

        public override void Reset()
        {
            timer = Duration;
        }

        public override void ApplySaveData(WaitActionSaveData data)
        {
            Status = (EStatus)data.status;
            Duration = data.duration;
            timer = data.timer;
        }

        public override ISaveable CollectData()
        {
            var data = new WaitActionSaveData();
            data.status = (int)Status;
            data.duration = Duration;
            data.timer = timer;
            return data;
        }

        public override void OnStart()
		{
			timer = Duration;
		}

		public override void Update()
		{
			if (timer > 0)
			{
				timer -= Main.deltaTimeSafe;
				Status = EStatus.Running;
			}
			else Status = EStatus.Success;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Waiting: " + timer.ToString("0.00") + " / " + Duration.ToString("0.00"));
		}
#endif
	}
}