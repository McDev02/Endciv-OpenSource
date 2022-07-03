using System.Collections.Generic;

namespace Endciv
{
	public class PowerSourceCombustionFeature : Feature<PowerSourceCombustionSaveData>, 
		IViewController<PowerSourceCombustionFeatureView>
	{
		//Static Data
		public PowerSourceCombustionStaticData StaticData { get; private set; }		
		
		public PowerSourceCombustionFeatureView View { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<PowerSourceCombustionStaticData>();
		}

		//Properties
		public float currentFuel;
		public float powerOutput;
		public List<FeatureBase> consumers;

		public override void Run(SystemsManager manager)
		{
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
			SystemsManager.PowerSourceSystem.DeregisterPowerSource(this);
		}

		public override ISaveable CollectData()
		{
			var data = new PowerSourceCombustionSaveData();

			return data;
		}

		public override void ApplyData(PowerSourceCombustionSaveData data)
		{
		}

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (PowerSourceCombustionFeatureView)view;
		}

		public void ShowView()
		{
			if (View != null)
				View.ShowHide(true);
		}

		public void HideView()
		{
			if (View != null)
				View.ShowHide(false);
		}

		public void UpdateView()
		{
			if (View != null)
				View.UpdateView();
		}

		public void SelectView()
		{
			if (View != null)
				View.OnViewSelected();
		}

		public void DeselectView()
		{
			if (View != null)
				View.OnViewDeselected();
		}
	}
}