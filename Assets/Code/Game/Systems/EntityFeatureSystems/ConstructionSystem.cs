using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Endciv
{
	public class ConstructionSystem : EntityFeatureSystem<ConstructionFeature>
	{
		public const float EPSILON = 0.00001f;
		public const int ConstructionInventoryLoad = 999;

		public enum EConstructionType { Single, Area }
		public enum EConstructionState { Construction, Demolition, Ready }

		private List<List<ConstructionFeature>> UnfinishedSites;
		private List<List<ConstructionFeature>> FinishedSites;
		private List<Stack<ConstructionFeature>> ChangingSites;
		private List<ETechnologyType> TechPool;

		public static Action<string> OnBuildingPlaced;
		public static Action<string> OnBuildingBuilt;
		public static Action<string> OnBuildingDemolished;
		public Action OnTechChanged;

		EntitySystem entitySystem;
		static GridMap gridMap;

		float wearTimer;

		public ConstructionSystem(int factions, EntitySystem entitySystem, GridMap gridMap) : base(factions)
		{
			UpdateStatistics();

			ConstructionSystem.gridMap = gridMap;
			this.entitySystem = entitySystem;

			FinishedSites = new List<List<ConstructionFeature>>(factions);
			UnfinishedSites = new List<List<ConstructionFeature>>(factions);
			ChangingSites = new List<Stack<ConstructionFeature>>(factions);
			for (int i = 0; i < factions; i++)
			{
				FinishedSites.Add(new List<ConstructionFeature>(i == 0 ? 128 : 16));
				UnfinishedSites.Add(new List<ConstructionFeature>(i == 0 ? 64 : 8));
				ChangingSites.Add(new Stack<ConstructionFeature>(i == 0 ? 32 : 4));
			}

			TechPool = new List<ETechnologyType>();
		}

		private InventoryStaticData constructionSiteInventoryData;
		public InventoryStaticData ConstructionSiteInventoryData
		{
			get
			{
				if (constructionSiteInventoryData == null)
				{
					constructionSiteInventoryData = ScriptableObject.
						CreateInstance<InventoryStaticData>();
					constructionSiteInventoryData.MaxCapacity = 999;
				}
				return constructionSiteInventoryData;
			}
		}

		internal void AddTech(ETechnologyType tech)
		{
			//if (!TechPool.Contains(tech))
			{
				TechPool.Add(tech);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<bool>("buildingTech_" + tech.ToString(), true);
				OnTechChanged?.Invoke();
			}
		}
		internal void RemoveTech(ETechnologyType tech)
		{
			if (TechPool.Contains(tech))
			{
				TechPool.Remove(tech);
				OnTechChanged?.Invoke();
			}
		}

		/// <summary>
		/// Call to check if tech level exists before builing structure
		/// Will return false if even one keyword isn't available
		/// </summary>
		/// <param name="structureData"></param>
		/// <returns></returns>
		public bool CanBuild(StructureFeatureStaticData structureData)
		{
			foreach (var tech in structureData.requiredTech)
			{
				if (!HasTech(tech))
					return false;
			}
			return true;
		}
		public bool HasTech(ETechnologyType tech)
		{
#if UNITY_EDITOR
			return Main.Instance.UnlockAllTech || TechPool.Contains(tech);
#else
			return  TechPool.Contains(tech);
#endif
		}

		internal override void RegisterFeature(ConstructionFeature feature)
		{
			base.RegisterFeature(feature);
			if (feature.ConstructionState == EConstructionState.Ready)
			{
				var sid = feature.Entity.StaticData.ID;
				var tech = feature.Entity.GetFeature<StructureFeature>().StaticData.providingTech;
				AddTech(tech);
				//Only increase notification tech
				FinishedSites[feature.FactionID].Add(feature);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger("totalStructuresBuilt");
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresBuilt", FinishedSites[feature.FactionID].Count);

				Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger($"totalBuilt_{sid}");

				OnBuildingPlaced?.Invoke(sid);
				OnBuildingBuilt?.Invoke(sid);
			}
			else
			{
				feature.constructionInfo = UI3DFactory.Instance.GetUI3DConstruction(feature, feature.ConstructionState == EConstructionState.Demolition, 2);
				UnfinishedSites[feature.FactionID].Add(feature);
				// totalStructuresPlaced is handled in GridTool
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresPlaced", UnfinishedSites[feature.FactionID].Count);

				OnBuildingPlaced?.Invoke(feature.Entity.StaticData.ID);
			}

		}

		internal override void DeregisterFeature(ConstructionFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			if (faction < 0) faction = feature.FactionID;
			if (UnfinishedSites[faction].Contains(feature))
			{
				UnfinishedSites[faction].Remove(feature);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresPlaced", UnfinishedSites[faction].Count);
			}
			if (FinishedSites[faction].Contains(feature))
			{
				FinishedSites[faction].Remove(feature);
				//We do not remove the tech value from the notification system
				RemoveTech(feature.Entity.GetFeature<StructureFeature>().StaticData.providingTech);

				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresBuilt", FinishedSites[faction].Count);
			}

			if (feature.constructionInfo != null)
			{
				UI3DFactory.Instance.Recycle(feature.constructionInfo);
				feature.constructionInfo = null;
			}
		}

		internal override void UpdateFaction(ConstructionFeature feature)
		{
			base.UpdateFaction(feature);
			Debug.LogError("Unimplemented feature: Change faction of construction features");
		}

		public override void UpdateGameLoop()
		{
			ConstructionFeature building;
			var data = GameConfig.Instance.GeneralEconomyValues;

			if (false)  //Decay over time off until necessary, currently it seems that this can only punish the player in the long run
			{           //Damage through fire, raids and natural disasters are enough
				wearTimer -= Main.deltaTimeSafe;
				if (wearTimer <= 0)
				{
					wearTimer += 1;

					//Simulate wear of buildings
					var decay = data.ConstructionDecay * Main.deltaTimeSafe;
					for (int f = 0; f < FinishedSites.Count; f++)
					{
						for (int i = 0; i < FinishedSites[f].Count; i++)
						{
							building = FinishedSites[f][i];
							if (building.ConstructionState == EConstructionState.Demolition)
								continue;
							//Reduce health!
							entitySystem.AddDamage(building.Entity, decay);
						}
					}
				}
			}

			var constructionValue = data.ConstructionSpeed * Main.deltaTimeSafe;
			var deconstructionValue = data.DemolitionSpeed * Main.deltaTimeSafe;
			//Contruction and Deconstruction
			for (int f = 0; f < UnfinishedSites.Count; f++)
			{
				for (int i = 0; i < UnfinishedSites[f].Count; i++)
				{
					building = UnfinishedSites[f][i];

					//Construction
					if (building.ConstructionState == EConstructionState.Construction)
					{
						//Add points to construction for every constructor
						foreach (var constructor in building.Constructors)
						{
							if (constructor.CurrentTask == null)
								continue;
							if (constructor.CurrentTask.CurrentAction == null)
								continue;
							if (constructor.CurrentTask.CurrentAction is ConstructionAction)
							{
								if ((constructor.CurrentTask.CurrentAction as ConstructionAction).inConstruction)
									AddConstructionPoints(building, constructionValue);
							}
						}

						if (building.CurrentConstructionPoints >= building.StaticData.MaxConstructionPoints)
						{
							building.ConstructionState = EConstructionState.Ready;
							ChangingSites[f].Push(building);
							building.Entity.GetFeature<StructureFeature>().View.
								FinishConstructionSite();
							building.DeregisterConstructionJob();
							if (building.constructionInfo != null)
							{
								UI3DFactory.Instance.Recycle(building.constructionInfo);
								building.constructionInfo = null;
							}
						}
						else
							building.Entity.GetFeature<StructureFeature>().View.
								UpdateConstructionView();
					}
					//Deconstruction
					else if (building.ConstructionState == EConstructionState.Demolition)
					{
						//Reduce points to construction for every constructor
						foreach (var constructor in building.Constructors)
						{
							if (constructor.CurrentTask == null)
								continue;
							if (constructor.CurrentTask.CurrentAction == null)
								continue;
							if (constructor.CurrentTask.CurrentAction is DemolitionAction)
							{
								if ((constructor.CurrentTask.CurrentAction as DemolitionAction).inDemolition)
									RemoveConstructionPoints(building, deconstructionValue);
							}
						}
						if (building.CurrentConstructionPoints <= 0)
						{
							DemolishConstructionSite(building);
						}
						else
							building.Entity.GetFeature<StructureFeature>().View.
								UpdateConstructionView();
					}
				}

				while (ChangingSites[f].Count > 0)
				{
					building = ChangingSites[f].Pop();

					switch (building.ConstructionState)
					{
						case EConstructionState.Construction:
						case EConstructionState.Demolition:
							DemolishConstructionSite(building);
							if (!UnfinishedSites[f].Contains(building)) UnfinishedSites[f].Add(building);
							if (FinishedSites[f].Contains(building)) FinishedSites[f].Remove(building);

							//Destroy
							break;
						case EConstructionState.Ready:
							if (UnfinishedSites[f].Contains(building)) UnfinishedSites[f].Remove(building);
							if (!FinishedSites[f].Contains(building))
							{
								FinishedSites[f].Add(building);
								var sid = building.Entity.StaticData.ID;
								OnBuildingBuilt?.Invoke(building.Entity.StaticData.ID);
								Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger($"totalBuilt_{sid}");
							}

							building.ConvertConstructionIntoBuilding();
							gridMap.UpdateGridObject(building.Entity, true);    //false when we have deffered update
							var tech = building.Entity.GetFeature<StructureFeature>().StaticData.providingTech;
							AddTech(tech);
							break;
					}
				}
			}

			//UpdateStatistics();
		}

		//Irreversible process - use with caution!
		public static void InitiateDemolition(ConstructionFeature site)
		{
			if (site.ConstructionState != EConstructionState.Demolition)
			{
				site.ConstructionState = EConstructionState.Demolition;
				site.DemolitionStartingProgress = site.ConstructionProgress;
				gridMap.UpdateGridObject(site.Entity, true);    //false when we have deffered update
			}
			var features = site.Entity.Features.Values.ToArray();
			for (int i = site.Entity.FeatureCount - 1; i >= 0; i--)
			{
				var feature = features[i];
				if (feature is InventoryFeature)
					continue;
				if (feature is ConstructionFeature)
					continue;
				if (feature is GridObjectFeature)
					continue;
				if (feature is StructureFeature)
					continue;
				if (feature.IsRunning)
					feature.Stop();
			}
			var mySystem = Main.Instance.GameManager.SystemsManager.ConstructionSystem;
			if (mySystem.FinishedSites[site.FactionID].Contains(site))
			{
				mySystem.FinishedSites[site.FactionID].Remove(site);
				Main.Instance.GameManager.SystemsManager.ConstructionSystem.RemoveTech(site.Entity.GetFeature<StructureFeature>().StaticData.providingTech);
				mySystem.OnTechChanged?.Invoke();
				if (!mySystem.UnfinishedSites[site.FactionID].Contains(site))
				{
					//TODO:Faulty - still?
					mySystem.UnfinishedSites[site.FactionID].Add(site);
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresPlaced", mySystem.UnfinishedSites[site.FactionID].Count);
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentStructuresBuilt", mySystem.FinishedSites[site.FactionID].Count);
				}
			}
		}

		public static void MarkForDemolition(ConstructionFeature site, bool mark)
		{
			if (site.MarkedForDemolition == mark)
				return;
			if (!mark)
			{
				if (site.ConstructionState == EConstructionState.Demolition)
					return;

				//Reset normal state
				site.MarkedForDemolition = false;
				//Is construction
				if (site.ConstructionState == EConstructionState.Construction)
				{
					if (site.constructionInfo == null)
						site.constructionInfo = UI3DFactory.Instance.GetUI3DConstruction(site, false, 2);
					else site.constructionInfo.SetMode(false);
				}
				else
				{
					site.DeregisterDeconstructionJob();
					UI3DFactory.Instance.Recycle(site.constructionInfo);
					site.constructionInfo = null;
				}
			}
			else
			{
				site.MarkedForDemolition = true;
				site.RegisterDeconstructionJob();
				if (site.constructionInfo == null)
					site.constructionInfo = UI3DFactory.Instance.GetUI3DConstruction(site, true, 2);
				else site.constructionInfo.SetMode(true);
			}
		}

		//Site is completely dmolished
		public static void DemolishConstructionSite(ConstructionFeature site)
		{
			site.CurrentConstructionPoints = 0;
			if (site.constructionInfo != null)
			{
				UI3DFactory.Instance.Recycle(site.constructionInfo);
				site.constructionInfo = null;
			}
			//UpdateGridMap
			//gridMap.UpdateGridObject(site.Entity, true, true);

			InventorySystem.DropInventoryToTheFloorAsync(site.Entity.Inventory);
			int itemCount = (int)Mathf.Floor(site.StaticData.DemolitionReturn.Sum(x => x.Amount) * site.DemolitionStartingProgress);
			if (itemCount > 0)
			{
				//Clone the return array
				var drops = new List<ResourceStack>();
				foreach (var stack in site.StaticData.DemolitionReturn)
				{
					drops.Add(new ResourceStack(stack.ResourceID, stack.Amount));
				}

				for (int i = 0; i < itemCount; i++)
				{
					if (drops.Count <= 0) break;

					var stack = drops[UnityEngine.Random.Range(0, drops.Count)];
					stack.Amount -= 1;
					if (stack.Amount <= 0)
						drops.Remove(stack);
					ResourcePileSystem.PlaceStoragePile(site.Entity, stack.ResourceID, 1);
				}
			}
			site.Entity.Destroy();

			var sid = site.Entity.StaticData.ID;
			OnBuildingDemolished?.Invoke(sid);
			Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger($"totalDemolished_{sid}");
		}

		//Site is ready and finished
		public static void FinishConstructionSite(ConstructionFeature site)
		{
			site.CurrentConstructionPoints = site.StaticData.MaxConstructionPoints;
		}
		public static void AddConstructionPoints(ConstructionFeature site, float points)
		{
			//Prevent construction points to be greater than resources progress
			var maxPoints = Mathf.Min(site.StaticData.MaxConstructionPoints, (site.ResourceProgress + EPSILON) * site.StaticData.MaxConstructionPoints);
			site.CurrentConstructionPoints = CivMath.Clamp(site.CurrentConstructionPoints + points, 0, maxPoints);
		}
		public static void RemoveConstructionPoints(ConstructionFeature site, float points)
		{
			site.CurrentConstructionPoints = CivMath.Clamp(site.CurrentConstructionPoints - points, 0, site.StaticData.MaxConstructionPoints);
		}

		public override void UpdateStatistics()
		{
		}
	}
}