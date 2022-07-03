using System.Collections.Generic;

namespace Endciv
{
    public abstract class AIAgentFeatureBase : FeatureBase
    {
        public BaseEntity UnitEntity { get; private set; }

        public AITask CurrentTask;

        //StaticData

        //Properties
        public float mood;

        public EntityNeed HealthNeed;
        public EntityNeed HungerNeed;
        public EntityNeed ThirstNeed;
        public EntityNeed StressNeed;
        public EntityNeed CleaningNeed;
        public List<EntityNeed> dailyReducedNeeds;
        public List<EntityNeed> needsLookup;
		/// <summary>
		/// How much the needs thresholds changed over the lifetime of the citizen
		/// </summary>
		public float moodChangeOverLifetime;

        public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
        {
			base.Setup(entity);
			UnitEntity = entity;
            
            
        }

		public virtual void SetAIAgentSettings(AiAgentSettings aiAgentSettings)
		{
			HealthNeed = new EntityNeed("Health", aiAgentSettings.HealthSatisfaction, 3);
			needsLookup = new List<EntityNeed>() { HealthNeed };
			dailyReducedNeeds = new List<EntityNeed>();

			if (Entity.HasFeature<LivingBeingFeature>())
			{
				StressNeed = new EntityNeed("Stress", aiAgentSettings.StressSatisfaction, 1.5f, 1);
				HungerNeed = new EntityNeed("Hunger", aiAgentSettings.HungerSatisfaction, 3);
				ThirstNeed = new EntityNeed("Thirst", aiAgentSettings.ThirstSatisfaction, 3);
				CleaningNeed = new EntityNeed("Cleaning", aiAgentSettings.CleaningSatisfaction, 1);

				needsLookup.AddRange(new EntityNeed[] { HealthNeed, HungerNeed, ThirstNeed, StressNeed, CleaningNeed });
				dailyReducedNeeds.AddRange(new EntityNeed[] { CleaningNeed });

			}
		}

        public override void Run(SystemsManager manager)
        {
            base.Run(manager);
            manager.AIAgentSystem.RegisterFeature(this);
        }

        public override void Stop()
        {
            base.Stop();
            if (CurrentTask != null)
                CurrentTask.Cancel();
            if (SystemsManager == null || SystemsManager.AIAgentSystem == null)
                UnityEngine.Debug.Log("");
            SystemsManager.AIAgentSystem.DeregisterFeature(this);
        }

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.AIAgentSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.AIAgentSystem.RegisterFeature(this);
		}

		public override void ApplySaveData(object data)
        {

        }
    }

	/// <summary>
	/// Base AI Agent
	/// </summary>
	public abstract class AIAgentFeature<T> : AIAgentFeatureBase
        where T : AIAgentFeatureSaveData
	{
        public override void ApplySaveData(object data)
        {
            if (data == null)
                return;
            var saveData = (T)data;
            if (saveData == null)
                return;

            object taskData = saveData.taskData;
            TaskSaveData baseTask = null;
            baseTask = (TaskSaveData)taskData;
            if (baseTask != null)
                CurrentTask = baseTask.ToTask(baseTask.taskType);

            ApplyData(saveData);
        }

        public abstract void ApplyData(T data);
    }
}