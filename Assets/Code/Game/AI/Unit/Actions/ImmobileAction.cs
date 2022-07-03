namespace Endciv
{
	public class ImmobileAction : AIAction<ActionSaveData>
	{
		LivingBeingFeature being;

		public ImmobileAction(LivingBeingFeature being)
		{
			this.being = being;
		}

		public override void Reset()
		{

		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
			if (Status != EStatus.Success && Status != EStatus.Failure)
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
			being.Entity.GetFeature<UnitFeature>().View.SwitchAnimationState(EAnimationState.Sleeping);
		}

		public override void Update()
		{
			//Rest until vitality increases
			if (being.vitality.Mood <= -0.99f)
				Status = EStatus.Running;
			else
				Status = EStatus.Success;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Immobile");
		}
#endif
	}
}