using System;
using System.Collections.Generic;

namespace Endciv
{
	public enum EInfobarCategory
	{
		DeadUnits, 
		HungryUnits,
		ThirstyUnits, 
		TroubledCattle,
		HomelessUnits,
		ImmigrantUnits,
		TraderUnits,
		Exploration
	}

	public class InfobarSystem : BaseGameSystem
	{		
		public Dictionary<EInfobarCategory, List<BaseEntity>> Entities { get; set; }

		public Action<EInfobarCategory, BaseEntity> OnEntitiesUpdated;

		public InfobarSystem()
		{
			Entities = new Dictionary<EInfobarCategory, List<BaseEntity>>();
			for(int i = 0; i < Enum.GetNames(typeof(EInfobarCategory)).Length; i++)
			{
				Entities.Add((EInfobarCategory)i, new List<BaseEntity>());
			}
		}

		public void RegisterEntity(EInfobarCategory category, BaseEntity entity, bool ignoreFaction = false)
		{
			if (entity.factionID != SystemsManager.MainPlayerFaction && !ignoreFaction)
				return;
			if (!Entities[category].Contains(entity))
			{
				Entities[category].Add(entity);
				OnEntitiesUpdated?.Invoke(category, entity);
			}			
		}

		public void UnregisterEntity(EInfobarCategory category, BaseEntity entity, bool ignoreFaction = false)
		{
			if (entity.factionID != SystemsManager.MainPlayerFaction && !ignoreFaction)
				return;
			if (Entities[category].Contains(entity))
			{
				Entities[category].Remove(entity);
				OnEntitiesUpdated?.Invoke(category, entity);
			}			
		}

		public override void UpdateGameLoop()
		{
			
		}

		public override void UpdateStatistics()
		{
			
		}
	}
}