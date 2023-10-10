
using System.Collections.Generic;

namespace Endciv
{
	public enum EMiningType { Rainwater, Groundwater }

	/// <summary>
	/// Manages Mining
	/// </summary>
	public class MiningSystem : EntityFeatureSystem<MiningFeature>
	{
		int[,] wellCountPerTile;

		TimeManager timeManager;
		WeatherSystem weatherSystem;
		static GridMap gridMap;

		GeneralSystemsConfig config;
		float tickFactor;

		public struct WellData
		{
			public float waterTiles;
			public float pollution;
			public float efficientcy;
		}

		public MiningSystem(int factions, GridMap gridMap, GeneralSystemsConfig cfg, TimeManager timeManager, WeatherSystem weatherSystem) : base(factions)
		{
			this.weatherSystem = weatherSystem;
			this.timeManager = timeManager;
			MiningSystem.gridMap = gridMap;

			config = cfg;

			UpdateStatistics();
			tickFactor = timeManager.dayTickFactor;
			UpdateWellCountPerTile();
		}

		internal override void RegisterFeature(MiningFeature feature)
		{
			base.RegisterFeature(feature);
			//recalculate all wells or only those within range of this new one

			UpdateWellCountPerTile();
			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				feature.wellData = CalculateGain(FeaturesCombined[i]);
			}
		}

		internal override void DeregisterFeature(MiningFeature feature, int faction = -1)
		{
			//recalculate all wells or only those within range of this new one
			base.DeregisterFeature(feature, faction);

			UpdateWellCountPerTile();
			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				feature.wellData = CalculateGain(FeaturesCombined[i]);
			}
		}

		public override void UpdateGameLoop()
		{
			float groundwaterFactor = config.groundwaterCollectorFactor * tickFactor;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					var mine = FeaturesByFaction[f][i];
					//Todo: may cache in feature, inventory is always required for mining
					var inv = mine.Entity.GetFeature<InventoryFeature>();

					if (mine.StaticData.miningType == EMiningType.Groundwater)
					{
						mine.resourceCollectionRate = mine.wellData.waterTiles * mine.StaticData.value * groundwaterFactor;

						if (mine.resourceCollectionRate > 0)
						{
							mine.collectedResource += mine.resourceCollectionRate;

							var amount = (int)Mathf.Abs(mine.collectedResource);
							if (amount > 0)
							{
								//Always reduce water collected, even if it can not be added to the inventory
								if (mine.collectedResource < 0)
									mine.collectedResource += amount;
								else
									mine.collectedResource -= amount;

								//We could add water
								amount = Mathf.Min(amount, InventorySystem.GetAddableAmount(inv, FactoryConstants.WaterID));

								if (amount > 0)
								{
									//Try to add water
									var item = Main.Instance.GameManager.Factories.SimpleEntityFactory.
										CreateInstance(FactoryConstants.WaterID).GetFeature<ItemFeature>();
									List<ItemFeature> items = new List<ItemFeature>();
									if (item.StaticData.IsStackable)
									{
										item.Quantity = amount;
										items.Add(item);
									}
									else
									{
										item.Quantity = 1;
										for (int count = 1; count < amount; count++)
										{
											var itm = Main.Instance.GameManager.Factories.SimpleEntityFactory.
												CreateInstance(FactoryConstants.WaterID).GetFeature<ItemFeature>();
											itm.Quantity = 1;
											items.Add(itm);
										}
									}
									foreach (var itm in items)
									{
										InventorySystem.AddItem(inv, itm, false);
									}

								}
								//Reduce fill rate to not update each tick, also adds a bit of loss
								else
									mine.collectedResource -= 1f / 2f;
							}
							//Limit: mine.collectedResource = Mathf.Clamp(mine.collectedResource,0, mine.staticData.wat)
						}
					}
					if (mine.StaticData.miningType == EMiningType.Rainwater)
					{
						mine.resourceCollectionRate = config.rainwaterCollectorFactor * weatherSystem.RainfillPerTile;
						if (mine.resourceCollectionRate > 0)
						{
							mine.collectedResource += mine.resourceCollectionRate;

							var amount = (int)Mathf.Abs(mine.collectedResource);
							if (amount > 0)
							{
								//Always reduce water collected, even if it can not be added to the inventory
								if (mine.collectedResource < 0)
									mine.collectedResource += amount;
								else
									mine.collectedResource -= amount;

								//We could add water
								amount = Mathf.Min(amount, InventorySystem.GetAddableAmount(inv, FactoryConstants.WaterID));

								if (amount > 0)
								{
									//Try to add water
									var waterUnit = Main.Instance.GameManager.Factories.SimpleEntityFactory.
										CreateInstance(FactoryConstants.WaterID).GetFeature<ItemFeature>();
									waterUnit.Quantity = amount;
									InventorySystem.AddItem(inv, waterUnit, false);
								}
								//Reduce fill rate to not update each tick, also adds a bit of loss
								else
									mine.collectedResource -= 1f / 2f;
							}
							//Limit: mine.collectedResource = Mathf.Clamp(mine.collectedResource,0, mine.staticData.wat)
						}
					}
				}
			}
			//UpdateStatistics();
		}

		public override void UpdateStatistics()
		{
		}

		public WellData CalculateGain(MiningFeature mining)
		{
			return CalculateGain(mining.StaticData, mining.Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect, true);
		}

		public WellData CalculateGain(MiningStaticData mining, RectBounds rect)
		{
			return CalculateGain(mining, rect, false);
		}
		private WellData CalculateGain(MiningStaticData mining, RectBounds rect, bool isRegistered = true)
		{
			//Calculate gain
			int r = (int)mining.radius;
			WellData data = new WellData();

			var centeri = rect.Centeri;
			int fullTiles = 0;
			float tilesWeight;
			for (int x = -r; x < r; x++)
			{
				for (int y = -r; y < r; y++)
				{
					var diff = new Vector2i(x, y);
					var pos = centeri + diff;
					if (diff.Magnitude > r)
						continue;
					fullTiles++;
					if (!gridMap.Grid.IsInRange(pos))
						continue;

					tilesWeight = Mathf.Clamp01(gridMap.Data.passability[pos.X, pos.Y]);
					var poll = gridMap.Data.pollution[pos.X, pos.Y];

				float wellsOfThisTile = wellCountPerTile[pos.X, pos.Y] + (isRegistered ? 0 : 1);
					//Decrease value of shared tiles to enhance the negative effect of overlapping tiles
					wellsOfThisTile = (wellsOfThisTile - 1) * 1.25f + 1;
					if (wellsOfThisTile > 1)
						tilesWeight *= 1f / wellsOfThisTile;

					/*	Displays all tiles for debugging
					if (rectViewObjects.Count <= counter)
						rectViewObjects.Add(UnityEngine.Object.Instantiate(userToolsView.RectIndicatorPrefab));
					var rectObj = rectViewObjects[counter++];
					rectObj.transform.position = gridMapView.GetPointWorldPosition(pos).To3D();
					rectObj.transform.localScale = new Vector3(1, 0.5f, 1);
					rectObj.material.SetColor(userToolsView.ColorName, Color.Lerp(userToolsView.WaterColor, userToolsView.InvalidColor, poll));

					rectObj.gameObject.SetActive(true);*/

					data.waterTiles += tilesWeight;
					data.pollution += tilesWeight * poll;
				}
			}
			data.pollution /= data.waterTiles;
			data.efficientcy = data.waterTiles / fullTiles;

			return data;
		}

		private void UpdateWellCountPerTile()
		{
			wellCountPerTile = new int[gridMap.Width, gridMap.Length];

			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				var well = FeaturesCombined[i];
				if (well.StaticData.miningType != EMiningType.Groundwater)
					continue;

				var rect = well.Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect;
				var centeri = rect.Centeri;

				int r = (int)well.StaticData.radius;
				for (int x = -r; x < r; x++)
				{
					for (int y = -r; y < r; y++)
					{
						var diff = new Vector2i(x, y);
						var pos = centeri + diff;
						wellCountPerTile[pos.X, pos.Y]++;
					}
				}
			}
		}
	}
}