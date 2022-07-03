using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace Endciv
{	
	[Serializable]
	public class FarmlandFeature : Feature<FarmlandFeatureSaveData>
	{
		//Static Data
		public FarmlandStaticData StaticData { get; private set; }

		//Properties		
		public CropFeature[,] cropModels;
		public AIAgentFeatureBase[,] assignedFarmers;
		public AIAgentFeatureBase assignedWaterTransporter;
		public List<List<CropFeature>> CropGroups;
		RectBounds rect;

        public AgricultureSystem System { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<FarmlandStaticData>();
			if (!Entity.Inventory.Chambers.Contains("Output"))
				outputChamberID = InventorySystem.AddChamber(Entity.Inventory, "Output");
			rect = entity.GetFeature<GridObjectFeature>().GridObjectData.Rect;
			cropModels = new CropFeature[rect.Width, rect.Length];
			assignedFarmers = new AIAgentFeatureBase[rect.Width, rect.Length];
			CropGroups = new List<List<CropFeature>>(4);
		}

		private SystemsManager manager;
		public int outputChamberID;

		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
            System = manager.AgricultureSystem;
			manager.AgricultureSystem.RegisterFeature(this);
			base.Run(manager);
		}

		public override void Stop()
		{
			manager.AgricultureSystem.DeregisterFeature(this);
			base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.AgricultureSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.AgricultureSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new FarmlandFeatureSaveData();
			data.assignedFarmerIDs = new string[rect.Width, rect.Length];
			data.cropModels = new EntitySaveData[rect.Width, rect.Length];
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (assignedFarmers[i, j] == null)
						data.assignedFarmerIDs[i, j] = string.Empty;
					else
						data.assignedFarmerIDs[i, j] = assignedFarmers[i, j].Entity.UID.ToString();

					if (cropModels[i, j] != null)
					{
						data.cropModels[i, j] = cropModels[i, j].Entity.CollectData() as EntitySaveData;
					}
				}
			}
			if (CropGroups != null && CropGroups.Count > 0)
			{
				data.CropGroupIDs = new List<List<SerVector2i>>();
				foreach (var list in CropGroups)
				{
					if (list == null || list.Count <= 0)
					{
						data.CropGroupIDs.Add(null);
					}
					else
					{
						data.CropGroupIDs.Add(new List<SerVector2i>());
						var idList = data.CropGroupIDs.Last();
						foreach (var crop in list)
						{
							var index = GetCropsIndex(crop);
							if (index.X != -1 && index.Y != -1)
							{
								idList.Add(new SerVector2i(index));
							}
							else
							{
								idList.Add(new SerVector2i(-1, -1));
							}
						}
					}
				}
			}
			if (assignedWaterTransporter == null)
			{
				data.assignedWaterTransporterID = string.Empty;
			}
			else
			{
				data.assignedWaterTransporterID = assignedWaterTransporter.Entity.UID.ToString();
			}
			data.outputChamberID = outputChamberID;
			return data;
		}

		public override void ApplyData(FarmlandFeatureSaveData data)
		{
			assignedFarmers = new AIAgentFeatureBase[data.assignedFarmerIDs.GetLength(0), data.assignedFarmerIDs.GetLength(1)];
			cropModels = new CropFeature[data.cropModels.GetLength(0), data.cropModels.GetLength(1)];
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (!string.IsNullOrEmpty(data.assignedFarmerIDs[i, j]))
					{
                        var id = Guid.Parse(data.assignedFarmerIDs[i, j]);
						if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
						{
							assignedFarmers[i, j] = Main.Instance.GameManager.SystemsManager.Entities[id].GetFeature<AIAgentFeatureBase>();
						}
					}

					if (data.cropModels[i, j] != null)
					{
                        var instance = AgricultureSystem.PlantCrops(this, data.cropModels[i, j].id, data.cropModels[i, j].GetSaveData<CropFeatureSaveData>().variationID, new SerVector2i(i, j));
                        instance.Entity.ApplySaveData(data.cropModels[i, j]);
                    }
                }
			}
			CropGroups = new List<List<CropFeature>>();
			if (data.CropGroupIDs != null && data.CropGroupIDs.Count > 0)
			{
				for (int i = 0; i < data.CropGroupIDs.Count; i++)
				{
					CropGroups.Add(new List<CropFeature>());
					if (data.CropGroupIDs[i] == null || data.CropGroupIDs[i].Count <= 0)
						continue;
					for (int j = 0; j < data.CropGroupIDs[i].Count; j++)
					{
						if (data.CropGroupIDs[i][j] == null)
						{
							CropGroups.Add(null);
						}
						else
						{
							var index = data.CropGroupIDs[i][j];
							CropGroups[i].Add(cropModels[index.X, index.Y]);
						}
					}
				}

			}

			if (!string.IsNullOrEmpty(data.assignedWaterTransporterID))
			{
                var id = Guid.Parse(data.assignedWaterTransporterID);
                if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
                {
					assignedWaterTransporter = Main.Instance.GameManager.SystemsManager.Entities[id].GetFeature<AIAgentFeatureBase>();
				}
			}
		}

		public int SpaceLeft()
		{
			int space = 0;
			for (int x = 0; x < rect.Width; x++)
			{
				for (int y = 0; y < rect.Length; y++)
				{
					if (cropModels[x, y] == null)
						space++;
				}
			}
			return space;
		}

		public int MaxSpace { get { return rect.Width * rect.Length; } }

		public bool HasFarmer(AIAgentFeatureBase farmer)
		{
			for (int i = 0; i < assignedFarmers.GetLength(0); i++)
			{
				for (int j = 0; j < assignedFarmers.GetLength(1); j++)
				{
					if (assignedFarmers[i, j] == farmer)
						return true;
				}
			}
			return false;
		}

		public void UnassignFarmer(AIAgentFeatureBase farmer, Vector2i index)
		{
			if (assignedFarmers[index.X, index.Y] == farmer)
			{
				assignedFarmers[index.X, index.Y] = null;
			}
		}

		public void UnassignFarmer(AIAgentFeatureBase farmer)
		{
			for (int i = 0; i < assignedFarmers.GetLength(0); i++)
			{
				for (int j = 0; j < assignedFarmers.GetLength(1); j++)
				{
					if (assignedFarmers[i, j] == farmer)
					{
						assignedFarmers[i, j] = null;
						break;
					}

				}
			}
		}

		public void RemoveCrops(CropFeature crops)
		{
			for (int i = CropGroups.Count - 1; i >= 0; i--)
			{
				if (CropGroups[i].Contains(crops))
				{
					CropGroups[i].Remove(crops);
				}
				if (CropGroups[i].Count <= 0)
					CropGroups.RemoveAt(i);
			}

			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (cropModels[i, j] == crops)
					{
						cropModels[i, j] = null;
						break;
					}
				}
			}
            crops.Destroy();			
		}

		public bool HasUnwateredPlant(float targetWaterLevel, out Vector2i plantIndex)
		{
			plantIndex = new Vector2i(-1, -1);
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (cropModels[i, j] == null)
					{
						continue;
					}
					if (cropModels[i, j].cropState != ECropState.Growing)
					{
						continue;
					}
					if (cropModels[i, j].humidity.Progress > targetWaterLevel)
					{
						continue;
					}
					if (assignedFarmers[i, j] != null)
					{
						continue;
					}
					plantIndex = new Vector2i(i, j);
					return true;
				}
			}
			return false;
		}

		public bool HasUnplantedPlant(out Vector2i plantIndex)
		{
			plantIndex = new Vector2i(-1, -1);
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (cropModels[i, j] == null)
					{
						continue;
					}
					if (cropModels[i, j].cropState != ECropState.Unplanted)
					{
						continue;
					}
					if (assignedFarmers[i, j] != null)
					{
						continue;
					}
					plantIndex = new Vector2i(i, j);
					return true;
				}
			}
			return false;
		}

		public bool HasGrownPlant(out Vector2i plantIndex)
		{
			plantIndex = new Vector2i(-1, -1);
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (cropModels[i, j] == null)
					{
						continue;
					}
					if (cropModels[i, j].cropState != ECropState.Mature)
					{
						continue;
					}
					if (assignedFarmers[i, j] != null)
					{
						continue;
					}
					plantIndex = new Vector2i(i, j);
					return true;
				}
			}
			return false;
		}

		public Location GetCropsLocation(Vector2i plantIndex)
		{
			return new Location(new Vector2[] { cropModels[plantIndex.X, plantIndex.Y].View.transform.position.To2D() });
		}

		public Vector2i GetCropsIndex(CropFeature crop)
		{
			Vector2i index = new Vector2i(-1, -1);
			for (int i = 0; i < cropModels.GetLength(0); i++)
			{
				for (int j = 0; j < cropModels.GetLength(1); j++)
				{
					if (cropModels[i, j] == crop)
					{
						index = new Vector2i(i, j);
						break;
					}
				}
			}
			return index;
		}
	}
}