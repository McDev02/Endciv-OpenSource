using System;
using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
	/// <summary>
	/// A system containing all Structure Entitites
	/// </summary>
	public class StructureSystem : EntityFeatureSystem<StructureFeature>
	{

		public Dictionary<Guid, BaseEntity> structurePool;
		public StructureSystem(GridMap gridmap, int factions) : base(factions)
		{
			gridmap.structureSystem = this;
			structurePool = new Dictionary<Guid, BaseEntity>();
		}

		public Action OnStructureAdded;
		public Action OnStructureRemoved;

		internal override void RegisterFeature(StructureFeature feature)
		{
			base.RegisterFeature(feature);
			structurePool.Add(feature.Entity.UID, feature.Entity);
			OnStructureAdded?.Invoke();
		}
		internal override void DeregisterFeature(StructureFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			structurePool.Remove(feature.Entity.UID);
			OnStructureRemoved?.Invoke();
		}

		public override void UpdateGameLoop()
		{
		}

		public override void UpdateStatistics()
		{
		}
	}
}