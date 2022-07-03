namespace Endciv
{
    public class AnimalEatingAction : AIAction<WaitActionSaveData>
    {
        AIAgentFeatureBase AIAgent;
        public float Duration;
        public float timer;

        public AnimalEatingAction(AIAgentFeatureBase aiAgent, float duration)
        {
            AIAgent = aiAgent;
            Duration = duration;
            timer = Duration;
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
            if (Status != EStatus.Success && Status != EStatus.Failure)
            {
                OnStart();
            }
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
            AIAgent.Entity.GetFeature<UnitFeature>().View.SwitchAnimationState(EAnimationState.Idle);
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
            UnityEngine.GUILayout.Label("Eat action");
        }
#endif
    }
}