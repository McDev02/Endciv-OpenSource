using UnityEngine;

namespace Endciv
{
	public class MaintainPastureAction : AIAction<ActionSaveData>
	{
		private AITask task;
		private BaseEntity citizen;
		private string pastureFeatureKey;		

		public MaintainPastureAction(BaseEntity citizen, AITask task, string pastureFeatureKey)
		{
			this.citizen = citizen;
			this.pastureFeatureKey = pastureFeatureKey;
			this.task = task;
		}

		public override void Reset()
		{

		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
			if (Status == EStatus.Started || Status == EStatus.Running)
			{
				OnStart();
			}			
		}

		public override ISaveable CollectData()
		{
			var data = new ActionSaveData();
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			citizen.GetFeature<UnitFeature>().View.
				SwitchAnimationState(EAnimationState.Working);
		}

		public override void Update()
		{
			var pasture = task.GetMemberValue<PastureFeature>(pastureFeatureKey);
			if (pasture == null || pasture.Entity == null)
			{
				Status = EStatus.Failure;
				return;
			}
			var construction = pasture.Entity.GetFeature<ConstructionFeature>();
			if (construction.MarkedForDemolition)
			{
				Status = EStatus.Failure;
				return;
			}
			pasture.Filth = Mathf.Clamp(pasture.Filth - (0.1f * Main.deltaTime), 0f, 1f);
			if (pasture.Filth <= 0.1f)
			{
				Status = EStatus.Success;
				return;
			}			
			Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{

		}

#endif
	}
}