namespace Endciv
{
	public class ProductionAction : AIAction<ActionSaveData>
	{
		private CitizenAIAgentFeature citizen;
		private ProductionFeature facility;
        private RecipeFeature myOrder;
		private ProductionSystem productionSystem;

		public ProductionAction(CitizenAIAgentFeature citizen, ProductionFeature facility)
		{
			this.citizen = citizen;
			this.facility = facility;
			productionSystem = Main.Instance.GameManager.SystemsManager.ProductionSystem;
		}

        public override void Reset()
        {
            
        }

        public override void ApplySaveData(ActionSaveData data)
        {
            Status = (EStatus)data.status;
            if(Status == EStatus.Started || Status == EStatus.Running)
            {
                OnStart();
            }
            else
            {
                if(facility != null)
                {
                    myOrder = productionSystem.GetProductionLineOf(facility, citizen);
                    if(myOrder != null)
                    {
                        myOrder.InProduction = false;
                    }
                }
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
            myOrder = productionSystem.GetProductionLineOf(facility, citizen);
            myOrder.InProduction = true;
            citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Working);
		}

		public override void Update()
		{
            //Facility destroyed?
            if(myOrder == null)
            {
                Status = EStatus.Failure;
                return;
            }
            if(facility == null || facility.Entity == null)
            {
                Status = EStatus.Failure;
                return;
            }
            var construction = facility.Entity.GetFeature<ConstructionFeature>();
            if(construction.MarkedForDemolition)
            {
                Status = EStatus.Failure;
                return;
            }

            //All items produced
            if(myOrder.amountCompleted == myOrder.targetAmount) 
            {
                myOrder.InProduction = false;
                Status = EStatus.Success;
                return;
            }

            //Cancelled by ProductionSystem (not enough materials)
            if(!myOrder.InProduction)
            {
                Status = EStatus.Failure;
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