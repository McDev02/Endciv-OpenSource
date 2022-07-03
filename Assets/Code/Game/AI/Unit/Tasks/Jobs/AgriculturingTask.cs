namespace Endciv
{
	/// <summary>
	/// What is this doing? Describe.
	/// </summary>
	public abstract class AgriculturingTask : AITask
	{
		protected const string plantIndexKey = "NextPlantIndex";
		protected const string plantLocationKey = "NextPlantLocation";
		protected const string farmKey = "Farm";

		protected FarmlandFeature farm;
		protected Vector2i plantIndex;

		public AgriculturingTask() { }
		public AgriculturingTask(BaseEntity farmer, FarmlandFeature farm, Vector2i plantIndex)
			: base(farmer)   //shall be an AIJob overlaod: farm, EWorkerType.Worker, 
		{
			this.farm = farm;
			this.plantIndex = plantIndex;
		}

		public override void Initialize()
		{
			SetMemberValue<BaseEntity>(farmKey, farm.Entity);
		}

		public void UnassignFarmer()
		{
			var index = GetMemberValue<Vector2i>(plantIndexKey);
            var farmEntity = GetMemberValue<BaseEntity>(farmKey);
            farmEntity.GetFeature<FarmlandFeature>().
                UnassignFarmer(Entity.GetFeature<CitizenAIAgentFeature>(), index);
		}
	}

}
