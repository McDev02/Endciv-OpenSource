using System.Collections.Generic;

namespace Endciv
{
	public class PowerSourceElectricityFeature : Feature<PowerSourceElectricitySaveData>
	{
		//Static Data
		public PowerSourceElectricityStaticData StaticData { get; private set; }
		SystemsManager manager;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<PowerSourceElectricityStaticData>();
			consumptionFactor = 1;			
		}

		//Properties
		public float UserDefinedPriority { get; set; }
		public float Priority { get { return StaticData.Priority + UserDefinedPriority; } }

		public float consumptionFactor;
		public List<FeatureBase> consumers;		
		
		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);

			consumers = new List<FeatureBase>();
			var consumerList = StaticData.powerConsumers.Split('|');
			foreach (var consumer in consumerList)
			{
				var type = GetType().Assembly.GetType(consumer);
				if (Entity.Features.ContainsKey(type))
				{
					consumers.Add(Entity.Features[type]);
				}
			}

			manager.ElectricitySystem.RegisterConsumer(this);
		}

		public override void Stop()
		{
			base.Stop();
			manager.ElectricitySystem.DeregisterConsumer(this);
		}

		public override ISaveable CollectData()
		{
			var data = new PowerSourceElectricitySaveData();
			data.userDefinedPriority = UserDefinedPriority;
		
			return data;
		}

		public override void ApplyData(PowerSourceElectricitySaveData data)
		{
			UserDefinedPriority = data.userDefinedPriority;
	
		}
	}
}