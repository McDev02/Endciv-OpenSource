namespace Endciv
{
    public class UseToiletAction : AIAction<WaitActionSaveData>
    {
        AIAgentFeatureBase AIAgent;
        public float Duration;
        public float timer;
        GoToToiletTask task;

        public UseToiletAction(AIAgentFeatureBase aiAgent, GoToToiletTask task, float duration)
        {
            AIAgent = aiAgent;
            Duration = duration;
            timer = Duration;
            this.task = task;
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
            //AIAgent.Unit.View.SwitchAnimationState(EAnimationState.Sleeping);
        }

        public override void Update()
        {
            if (timer > 0)
            {
                timer -= Main.deltaTimeSafe;
                Status = EStatus.Running;
            }
            else
            {
                var toilet = task.GetToilet();
                if(toilet != null)
                {
					var item = Main.Instance.GameManager.Factories.
						SimpleEntityFactory.CreateInstance(FactoryConstants.wasteOrganicID).
						GetFeature<ItemFeature>();
					item.Quantity = 1;
                    InventorySystem.AddItem(
                        toilet.Entity.GetFeature<InventoryFeature>(),
						item,
                        false);
                }
                Status = EStatus.Success;
            }                
        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
            UnityEngine.GUILayout.Label("Use Toilet action");
        }
#endif
    }
}