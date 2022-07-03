using UnityEngine;

namespace Endciv
{
	public class ConstructionAction : AIAction<ConstructionActionSaveData>
	{
		CitizenAIAgentFeature citizen;
		ConstructionFeature facility;
		ConstructionTask task;
		public bool inConstruction = false;

		private bool willTakeBreak;
		private int breakTimer;

		public ConstructionAction(CitizenAIAgentFeature citizen, ConstructionFeature facility, ConstructionTask task)
		{
			this.citizen = citizen;
			this.facility = facility;
			this.task = task;
		}

		public override void Reset()
		{
			inConstruction = false;
		}

		public override void ApplySaveData(ConstructionActionSaveData data)
		{
			Status = (EStatus)data.status;
			inConstruction = data.inConstruction;
			willTakeBreak = data.willTakeBreak;
			breakTimer = data.breakTimer;
			if (inConstruction)
			{
				citizen.Entity.GetFeature<UnitFeature>().View.
					SwitchAnimationState(Random.Range(0f, 1f) < 0.5f ?
					EAnimationState.HammeringStanding : EAnimationState.HammeringKneeling);
			}

		}

		public override ISaveable CollectData()
		{
			var data = new ConstructionActionSaveData();
			data.status = (int)Status;
			data.inConstruction = inConstruction;
			data.willTakeBreak = willTakeBreak;
			data.breakTimer = breakTimer;
			return data;
		}

		public override void OnStart()
		{
			inConstruction = true;
			citizen.Entity.GetFeature<UnitFeature>().View.
				SwitchAnimationState(Random.Range(0f, 1f) < 0.5f ?
				EAnimationState.HammeringStanding : EAnimationState.HammeringKneeling);
		}

		public override void Update()
		{
			//We check the shedule of the builder and force to stop work if a break is required.
			if (!citizen.lastSheduleData.hadPause)
			{
				if (!willTakeBreak)
				{
					var shedule = CitizenAISystem.GetCurrentUnitShedule(citizen);
					if (shedule != CitizenShedule.ESheduleType.Work)
					{
						willTakeBreak = true;
						breakTimer = CivRandom.Range(5, 20);
					}
				}
				//Break countdown for some randomness
				else
				{
					//Take a brake of work
					if (breakTimer <= 0)
					{
						willTakeBreak = false;
						Status = EStatus.Failure;
						inConstruction = false;
						return;
					}
					else
					{
						breakTimer--;
					}
				}
			}

			//Facility destroyed? Stop construction.
			if (facility == null || facility.Entity == null)
			{
				Status = EStatus.Failure;
				inConstruction = false;
				return;
			}

			//Marked for demolition? Stop construction.
			if (facility.MarkedForDemolition)
			{
				Status = EStatus.Failure;
				inConstruction = false;
				return;
			}

			//Facility is constructed
			if (facility.ConstructionState != ConstructionSystem.EConstructionState.Construction)
			{
				//Debug.Log("Success Construction");
				Status = EStatus.Success;
				inConstruction = false;
				return;
			}

			//Cancelled by ConstructionFeature (not enough materials)
			if ((facility.ResourceProgress) <= facility.ConstructionProgress)   // + ConstructionSystem.EPSILON
			{
				Status = EStatus.Failure;
				inConstruction = false;
				return;
			}
			Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			//UnityEngine.GUILayout.Label("Resting: " + Unit.Resting.Progress.ToString());
		}

#endif
	}
}