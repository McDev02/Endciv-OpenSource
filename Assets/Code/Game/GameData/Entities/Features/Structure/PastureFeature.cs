using System.Collections.Generic;
using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class PastureFeature : Feature<PastureFeatureSaveData>
	{
		//Static Data
		public PastureStaticData StaticData { get; private set; }

		public HashSet<string> AnimalPolicy { get; private set; }

		public List<CattleFeature> Cattle { get; private set; }
		public List<CattleFeature> ReservedCattle { get; private set; }

		public int MaxCapacity { get; private set; }		
		public int CapacityLeft { get { return MaxCapacity - Load; } }
		public int Load { get; private set; }
		public float LoadProgress { get { return MaxCapacity <= 0 ? 0 : Load / (float)MaxCapacity; } }

		public float CurrentNutrition { get; set; }
		public float CurrentWater { get; set; }

		//Used for maintainance
		//Range 0-1
		public float Filth { get; set; }

		public BaseEntity Cleaner { get; set; }

		public float NutritionProgress
		{
			get
			{
				if (StaticData.maxNutrition == 0f)
					return 0f;
				return CurrentNutrition / StaticData.maxNutrition;
			}
		}

		public float WaterProgress
		{
			get
			{
				if (StaticData.maxWater == 0f)
					return 0f;
				return CurrentWater / StaticData.maxWater;

			}
		}

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<PastureStaticData>();
			AnimalPolicy = new HashSet<string>();
			MaxCapacity = StaticData.maxCattleMass;
			Cattle = new List<CattleFeature>();
			ReservedCattle = new List<CattleFeature>();
			foreach (var animal in StaticData.Cattle)
			{
				AnimalPolicy.Add(animal.ID);
			}
		}

		private SystemsManager manager;

		//Methods
		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			manager.PastureSystem.RegisterFeature(this);
			base.Run(manager);
		}

		public override void Stop()
		{
			manager.PastureSystem.DeregisterFeature(this);
			base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.PastureSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.PastureSystem.RegisterFeature(this);
		}

		public void SetLoad(int load)
		{
			if (MaxCapacity < Load)
				Debug.LogError("Load greater than Capacity! " + Load.ToString() + " / " + CapacityLeft.ToString());
			else if (Load < 0)
				Debug.LogError("Load below Zero! " + Load.ToString() + " / " + CapacityLeft.ToString());
			Load = load;
		}

		public void SetMaxCapacity(int capacity)
		{
			MaxCapacity = capacity;
		}

		public void AddLoad(int load)
		{
			Load += load;
			if (MaxCapacity < Load)
				Debug.LogError("Load greater than Capacity! " + Load.ToString() + " / " + MaxCapacity.ToString());
			else if (Load < 0)
				Debug.LogError("Load below Zero! " + Load.ToString() + " / " + MaxCapacity.ToString());

		}

		public override ISaveable CollectData()
		{
			var data = new PastureFeatureSaveData();
			data.cattle = new List<string>();
			data.reservedCattle = new List<string>();
			foreach(var cattle in Cattle)
			{
				data.cattle.Add(cattle.Entity.UID.ToString());
			}
			foreach (var cattle in ReservedCattle)
			{
				data.cattle.Add(cattle.Entity.UID.ToString());
			}
			data.load = Load;
			data.currentNutrition = CurrentNutrition;
			data.currentWater = CurrentWater;
			data.filth = Filth;
			data.cleaner = string.Empty;
			if (Cleaner != null)
				data.cleaner = Cleaner.UID.ToString();
			return data;
		}

		public override void ApplyData(PastureFeatureSaveData data)
		{
			var systemsManager = Main.Instance.GameManager.SystemsManager;
			if (data.cattle != null)
			{
				foreach(var cattleUID in data.cattle)
				{
					var guid = Guid.Parse(cattleUID);
					var cattle = systemsManager.Entities[guid].GetFeature<CattleFeature>();
					Cattle.Add(cattle);
					cattle.Pasture = this;
				}
			}
			if (data.reservedCattle != null)
			{
				foreach (var cattleUID in data.reservedCattle)
				{
					var guid = Guid.Parse(cattleUID);
					var cattle = systemsManager.Entities[guid].GetFeature<CattleFeature>();
					ReservedCattle.Add(cattle);
					cattle.Pasture = this;
				}
			}
			Load = data.load;
			CurrentNutrition = data.currentNutrition;
			CurrentWater = data.currentWater;
			Filth = data.filth;
			if(!string.IsNullOrEmpty(data.cleaner))
			{
				var guid = Guid.Parse(data.cleaner);
				Cleaner = systemsManager.Entities[guid];
			}
		}
	}
}