using System.Collections.Generic;
using System;

namespace Endciv
{
	public enum EUtilityType { Toilet, Shower, Campfire };
	//Provides shelter for Units, animals and humanoids

	public class UtilityFeature : Feature<UtilityFeatureSaveData>
	{		
		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<UtilityStaticData>();
			Occupants = new List<BaseEntity>(MaxOccupants);
		}

		//Static Data
		public int CurrentOccupants { get { return Occupants.Count; } }
		public List<BaseEntity> Occupants { get; private set; }
		public int MaxOccupants { get { return StaticData.MaxOccupants; } }
		public UtilityStaticData StaticData { get; private set; }

		//Properties
		public float condition;

        public override void Run(SystemsManager manager)
        {
            base.Run(manager);
            manager.UtilitySystem.RegisterFeature(this);
        }

        public override void Stop()
        {
            base.Stop();
            SystemsManager.UtilitySystem.DeregisterFeature(this);
        }

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.UtilitySystem.DeregisterFeature(this, oldFaction);
			SystemsManager.UtilitySystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new UtilityFeatureSaveData();
			data.occupants = new List<string>();
			foreach (var occupant in Occupants)
			{
				data.occupants.Add(occupant.UID.ToString());
				data.condition = condition;
			}
			return data;
		}

		public override void ApplyData(UtilityFeatureSaveData data)
		{
			condition = data.condition;
			if (data.occupants != null && data.occupants.Count > 0)
			{
				var unitPool = Main.Instance.GameManager.SystemsManager.Entities;
				foreach (var id in data.occupants)
				{
                    if (string.IsNullOrEmpty(id))
                        continue;
                    var guid = Guid.Parse(id);
					if (unitPool.ContainsKey(guid))
					{
						Occupants.Add(unitPool[guid]);
					}
				}
			}
		}
	}
}