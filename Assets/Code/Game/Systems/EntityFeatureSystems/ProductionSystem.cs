using System.Linq;
using System.Collections.Generic;


namespace Endciv
{	
	public class ProductionSystem : EntityFeatureSystem<ProductionFeature>, ISaveable, ILoadable<ProductionSystemSaveData>
	{
		const int DEBUG_ProductionSpeed = 12;
		const int ProductionBatchSize = 10;
		const int ProductionThreshold = 5;

		SimpleEntityFactory resourceFactory;

		//Output ID / Recipe
		private Dictionary<string, RecipeFeature> GlobalOrders { get; set; }		
		public List<string> AvailableRecipes { get; set; }

		public RecipeFeature[] Orders
		{
			get
			{
				return GlobalOrders.Values.ToArray();
			}
		}

		public ProductionSystem(int factions, SimpleEntityFactory resourceFactory) : base(factions)
		{
			this.resourceFactory = resourceFactory;
			GlobalOrders = new Dictionary<string, RecipeFeature>();
			var recipeIDs = resourceFactory.GetStaticDataIDList<RecipeFeatureStaticData>();
			foreach(var id in recipeIDs)
			{
				var entity = resourceFactory.CreateInstance(id);
				var recipe = entity.GetFeature<RecipeFeature>();
				GlobalOrders.Add(recipe.StaticData.OutputResources.ResourceID, recipe);				
			}
			AvailableRecipes = new List<string>();
		}

		public override void UpdateStatistics()
		{
		}

		internal override void RegisterFeature(ProductionFeature feature)
		{
			base.RegisterFeature(feature);
			AvailableRecipes.Clear();
			for (int i = 0; i < FeaturesByFaction[SystemsManager.MainPlayerFaction].Count; i++)
			{
				var recipies = FeaturesByFaction[SystemsManager.MainPlayerFaction][i].StaticData.Recipes;
				for (int r = 0; r < recipies.Length; r++)
				{
					if (!AvailableRecipes.Contains(recipies[r]))
						AvailableRecipes.Add(recipies[r]);
				}
			}
			OnFeatureAdded?.Invoke();
		}

		internal override void DeregisterFeature(ProductionFeature feature,int faction=-1)
		{
			base.DeregisterFeature(feature);
			if (faction < 0) faction = feature.FactionID;
			AvailableRecipes.Clear();
			for (int i = 0; i < FeaturesByFaction[SystemsManager.MainPlayerFaction].Count; i++)
			{
				var recipies = FeaturesByFaction[SystemsManager.MainPlayerFaction][i].StaticData.Recipes;
				for (int r = 0; r < recipies.Length; r++)
				{
					if (!AvailableRecipes.Contains(recipies[r]))
						AvailableRecipes.Add(recipies[r]);
				}
			}
			OnFeatureRemoved?.Invoke();
		}

		public override void UpdateGameLoop()
		{
			ProductionFeature facility;

			//Contruction and DeProduction
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					facility = FeaturesByFaction[f][i];

					//Auto construct
					AddProductionPoints(facility, DEBUG_ProductionSpeed);
				}
			}

			UpdateProductionProgress();
			//UpdateStatistics

		}

		public static void AddProductionPoints(ProductionFeature site, float points)
		{
			//throw new NotImplementedException();
		}
		
		/// <summary>
		/// Method called whenever a production feature completes a local order
		/// </summary>
		public void UpdateOrderAmount(string ID, int amountChanged, RecipeFeature productionLine)
		{
			var order = GlobalOrders[ID];
			//Order does not exist in the global orders.
			//Not supposed to occur.
			if (order == null)
				return;
			order.amountInProgress -= amountChanged;

			//Check if total amount reached
			if (order.BatchesLeft <= 0)
			{
				//We have completed the order.
				//Stop other facilities in the production line from producing this order
				//Remove order from production line
				foreach (var facility in FeaturesByFaction[SystemsManager.MainPlayerFaction])
				{
					CancelOrder(facility, ID);
				}
			}
			else
			{
				//Add more to the production line if it has reached the threshold
				if (productionLine.BatchesLeft <= ProductionThreshold)
				{
					int size = Mathf.Min(order.BatchesLeft, ProductionBatchSize);
					productionLine.targetAmount += size;
				}
			}
		}		
		
		public RecipeFeature GetLocalProductionOrder(ProductionFeature facility)
		{
			//Calculate currently highest incomplete order list
			var orders = GetOrdersByPriority();

			//Check for no unassigned orders
			if (orders == null || orders.Count <= 0)
				return null;

			//Check if facility can craft any of the orders
			foreach (var order in orders)
			{
				//Check if facility contains recipe for this order
				if (!CanProduceOrder(facility, order))
					continue;

				//Assign order to facility for production
				int size = Mathf.Clamp(Mathf.Min(order.targetAmount - GetActiveRecipeInProgress(order), ProductionBatchSize), 0, int.MaxValue);
				order.amountInProgress += size;
				var localRecipe = resourceFactory.CreateInstance(order.Entity.StaticData.ID).GetFeature<RecipeFeature>();
				localRecipe.targetAmount = size;
				return localRecipe;				
			}
			return null;
		}


		/// <summary>
		/// Updates progress in all production lines
		/// </summary>
		void UpdateProductionProgress()
		{
			//Iterate all facilities
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int p = 0; p < FeaturesByFaction[f].Count; p++)
				{
					var facility = FeaturesByFaction[f][p];

					if (facility.WorkerCount <= 0)
						continue;

					//Iterate all production lines
					for (int i = 0; i < facility.ProductionLines.Length; i++)
					{
						var line = facility.ProductionLines[i];
						//Does the line have an active order?
						if (line == null)
							continue;
						//Does the line have an agent ready tor produce?
						if (!line.InProduction)
							continue;
						//Does the line have enough materials for the next batch?
						if (!HasMaterialsForRecipe(facility, line) && line.amountInProgress <= 0)
						{
							//Cancel production, another line probably got our materials
							line.InProduction = false;
							continue;
						}
						var recipe = line.StaticData;
						//Check if we're ready to begin a new batch production
						if (line.amountInProgress <= 0)
						{
							line.amountInProgress = 1;
							line.CurrentProgress = 0f;
							var inventory = facility.Entity.Inventory;
							//Consume recipe materials                   
							foreach (var mat in recipe.InputResources)
							{
								//Could not reduce materials - cancel order
								var withdrawnItems = InventorySystem.WithdrawItems(inventory, mat.ResourceID, mat.Amount);
								if (withdrawnItems != null)
								{
									line.InProduction = false;
									break;
								}
							}
							//Order was canceled, move to the next production line
							if (line.InProduction == false)
								continue;
						}
						//Update line's batch production progress
						line.CurrentProgress += GameConfig.Instance.GeneralEconomyValues.ProductionSpeed;

						//Is batch ready to produce?
						if (line.CurrentProgress >= 1f)
						{
							//Reset members
							line.amountInProgress = 0;
							line.amountCompleted++;
							line.CurrentProgress = 0f;
							//Produce output resources and store them in the Output chamber							
							var data = resourceFactory.EntityStaticData[recipe.OutputResources.ResourceID];
							List<ItemFeature> producedItems = new List<ItemFeature>();
							if (data.GetFeature<ItemFeatureStaticData>().IsStackable)
							{
								var item = resourceFactory.CreateInstance(recipe.OutputResources.ResourceID).GetFeature<ItemFeature>();
								item.Quantity = recipe.OutputResources.Amount;
								producedItems.Add(item);
							}
							else
							{
								for (int count = 0; count < recipe.OutputResources.Amount; count++)
								{
									var item = resourceFactory.CreateInstance(recipe.OutputResources.ResourceID).GetFeature<ItemFeature>();
									item.Quantity = 1;
									producedItems.Add(item);
								}
							}
							foreach (var item in producedItems)
							{
								InventorySystem.AddItem(facility.Entity.Inventory, item, true, facility.outputChamberID);
							}
							UpdateOrderAmount(line.StaticData.OutputResources.ResourceID, 1, line);
						}
						//Check if local order is complete
						if (line.TotalProgress >= 1f)
						{
							//Remove Order from production line
							facility.ProductionLines[i] = null;
							continue;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns an ordered list of Orders based on current priority
		/// excluding orders that are about to complete
		/// </summary>
		private List<RecipeFeature> GetOrdersByPriority()
		{
			//Iterate global orders, assign priorities
			var orders = GlobalOrders.Values.ToList();
			if (orders == null || orders.Count <= 0)
				return null;

			//Orders whose BatchesLeft is greater than 0
			List<RecipeFeature> standingOrders = new List<RecipeFeature>();

			foreach (var order in orders)
			{
				//Order is about to be completed with current facilities producing.
				//Don't count it.
				int activeRecipeInProgress = GetActiveRecipeInProgress(order);
				if (activeRecipeInProgress >= order.amountInProgress)
					continue;

				//Add order to uncompleted orders list
				standingOrders.Add(order);

				//Assigning priority			
				var outputID = order.StaticData.OutputResources.ResourceID;
				float pow = AvailableRecipes.Contains(outputID) ? 2 : 1;
				var batchesLeft = Mathf.Max(0, order.targetAmount - (StorageSystem.Statistics.GetItemCount(order.StaticData.OutputResources.ResourceID) + activeRecipeInProgress));
				order.currentPriority = Mathf.Pow(order.targetAmount <= 0 ? 0 : Mathf.Max(0, batchesLeft / order.targetAmount), pow);
			}

			if (standingOrders.Count <= 0)
				return null;
			return standingOrders.OrderByDescending(x => x.currentPriority).ToList();
		}

		public ISaveable CollectData()
		{
			var data = new ProductionSystemSaveData();
			data.globalOrders = new EntitySaveData[GlobalOrders.Count];
			int i = 0;
			var recipes = GlobalOrders.Values.ToArray();
			foreach (var recipe in recipes)
			{
				var entityData = (EntitySaveData)recipe.Entity.CollectData();
				data.globalOrders[i] = entityData;				
				i++;
			}
			return data;
		}

		public void ApplySaveData(ProductionSystemSaveData data)
		{
			if (data == null)
				return;
			GlobalOrders = new Dictionary<string, RecipeFeature>();
			foreach (var go in data.globalOrders)
			{
				var entity = resourceFactory.CreateInstance(go.id, go.UID);
				entity.ApplySaveData(go);
				var recipe = entity.GetFeature<RecipeFeature>();
				GlobalOrders.Add(recipe.StaticData.OutputResources.ResourceID, recipe);
			}
		}

		public int GetActiveRecipeInProgress(RecipeFeature recipe)
		{
			int sum = 0;
			foreach (var facility in FeaturesByFaction[SystemsManager.MainPlayerFaction])
			{
				foreach (var line in facility.ProductionLines)
				{
					if (line == null)
						continue;
					if (line.Entity.StaticData.ID != recipe.Entity.StaticData.ID)
						continue;
					sum += line.BatchesLeft;
				}
			}
			return sum;
		}

		public bool CanProduceOrder(ProductionFeature feature, RecipeFeature order)
		{
			foreach(var recipe in feature.StaticData.Recipes)
			{
				if (order.Entity.StaticData.ID == recipe)
					return true;
			}
			return false;
		}

		public void AssignRecipe(ProductionFeature feature, RecipeFeature recipe, int index)
		{
			feature.ProductionLines[index] = recipe;
		}

		public void CancelOrder(ProductionFeature feature, string resourceID)
		{
			for (int i = 0; i < feature.ProductionLines.Length; i++)
			{
				if (feature.ProductionLines[i] == null)
					continue;
				if (feature.ProductionLines[i].StaticData.OutputResources.ResourceID == resourceID)
				{
					feature.ProductionLines[i] = null;
				}
			}
		}		

		public int[] GetAvailableProductionLines(ProductionFeature feature, int limit)
		{
			List<int> lines = new List<int>();
			int max = Mathf.Min(limit, feature.ProductionLines.Length);
			for (int i = 0; i < max; i++)
			{
				if (feature.ProductionLines[i] != null)
					continue;
				if (!feature.StaticData.NeedsLabour)
					continue;
				lines.Add(i);
			}
			return lines.ToArray();
		}

		public RecipeFeature GetProductionLineOf(ProductionFeature feature, CitizenAIAgentFeature agent)
		{
			RecipeFeature recipe = null;
			for (int i = 0; i < feature.ProductionLines.Length; i++)
			{
				if (feature.ActiveWorkers[i] == agent)
				{
					recipe = feature.ProductionLines[i];
					break;
				}

			}
			return recipe;
		}

		public bool HasMaterialsForRecipe(ProductionFeature feature, RecipeFeature recipe)
		{			
			bool hasResources = true;
			foreach (var stack in recipe.StaticData.InputResources)
			{
				if (!InventorySystem.HasItems(feature.Entity.Inventory, stack.ResourceID, stack.Amount))
				{
					hasResources = false;
					break;
				}
			}
			return hasResources;
		}
	}
}