using System.Collections.Generic;

namespace Endciv
{
	public class PowerSourceRenewableFeature : Feature<PowerSourceRenewableSaveData>
	{
		//Static Data
		public PowerSourceRenewableStaticData StaticData { get; private set; }
		SystemsManager manager;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<PowerSourceRenewableStaticData>();
		}

		//Properties
		public List<FeatureBase> consumers;

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			base.Run(manager);
			manager.PowerSourceSystem.RegisterPowerSource(this);

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
		}

		public override void Stop()
		{
			base.Stop();
			manager.PowerSourceSystem.DeregisterPowerSource(this);
		}

		public override ISaveable CollectData()
		{
			var data = new PowerSourceRenewableSaveData();

			return data;
		}

		public override void ApplyData(PowerSourceRenewableSaveData data)
		{
		}
	}
}