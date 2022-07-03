using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Shoud not be monobehaviour later and is data driven
	/// </summary>
	public class LevelGenerator : MonoBehaviour
	{
		GameManager gameManager;
		GridMap gridMap;
		FactoryManager factories;
		UserToolSystem placementSystem;
		int maxX, maxY;

		GameMapSettings gameMapSettings;

		internal void Setup(GameManager gameManager, GridMap grid, FactoryManager factories, UserToolSystem placementSystem)
		{
			this.gameManager = gameManager;
			this.gridMap = grid;
			this.factories = factories;
			this.placementSystem = placementSystem;
		}

		/// <summary>
		/// Generates random resources
		/// </summary>
		internal IEnumerator DistributeResources(GameMapSettings gameMapSettings, LoadingState loadingState)
		{
			var time = DateTime.Now;
			this.gameMapSettings = gameMapSettings;
			maxX = gridMap.Width - gameMapSettings.terrainSettings.safePadding;
			maxY = gridMap.Length - gameMapSettings.terrainSettings.safePadding;
			var resourcePileIDs = factories.SimpleEntityFactory.GetStaticDataIDList<ResourcePileFeatureStaticData>().
				Except(factories.SimpleEntityFactory.GetStaticDataIDList<StoragePileFeatureStaticData>()).ToList();
			if (resourcePileIDs.Count <= 0)
				Debug.LogError("No ResourcePiles found");

			else
			{
#if USE_GRIDTILE
				int amount = (int)(gridMap.Grid.Area * GridMapView.GridTileFactor * Mathf.Clamp01(gameMapSettings.resourceDensity * gameMapSettings.resourceDensity));
#else
				int amount = (int)(gridMap.Grid.Area * Mathf.Clamp01(gameMapSettings.resourceDensity * gameMapSettings.resourceDensity));
#endif
				int pileCount = resourcePileIDs.Count;
				string objID;
				for (int i = 0; i < amount; i++)
				{
					objID = resourcePileIDs[CivRandom.Range(0, pileCount)];
					TryPlaceResourcePile(objID);

					if ((DateTime.Now - time).Milliseconds >= 500)
					{
						time = DateTime.Now;
						var val = (int)(i * 100f / amount);
						loadingState.SetMessage($"World: {val}%");
						yield return null;
					}
				}
				yield return null;
				gridMap.RecalculateGrid();
			}
		}

		internal IEnumerator SpawnStartingCitizen(GameMapSettings gameMapSettings, LoadingState loadingState)
		{
			Vector2i spawn;
			//minimum of 3 adults
			int children = Mathf.RoundToInt(gameMapSettings.startingCitizens * CivRandom.Range(0.2f, 0.4f));
			for (int i = 0; i < gameMapSettings.startingCitizens; i++)
			{
				if (gridMap.FindClosestEmptyTile(new Vector2i((int)(gridMap.Width / 2f), (int)(gridMap.Length / 2f)), 99, out spawn))
				{
					loadingState.SetMessage($"Citizen: {i}/{gameMapSettings.startingCitizens}");

					var age = i < children ? ELivingBeingAge.Child : ELivingBeingAge.Adult;
					var factoryParams = new FactoryParams();
					factoryParams.SetParams
						(
							new GridAgentFeatureParams()
							{
								Position = gridMap.View.GetTileWorldPosition(spawn).To3D()
							}, 
							new EntityFeatureParams()
							{
								FactionID = SystemsManager.MainPlayerFaction
							},
							new UnitFeatureParams()
							{
								Age = age,
								Gender = ELivingBeingGender.Undefined
							}
						);
					factories.SimpleEntityFactory.CreateInstance("human", null, factoryParams);
					yield return null;
				}
			}
		}

		/// <summary>
		/// Generates starting resources
		/// </summary>
		internal IEnumerator GenerateStartingResources(GameMapSettings gameMapSettings, LoadingState loadingState)
		{
			var agricultureSystem = gameManager.SystemsManager.AgricultureSystem;
			var factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			if (gameMapSettings.startingResources.foodPool != null && gameMapSettings.startingResources.foodPool.Count > 0)
			{
				float generatedNutrition = 0;
				int nutrition = (int)gameMapSettings.startingResources.foodMin + (int)Mathf.Round(gameMapSettings.startingCitizens * gameMapSettings.startingResources.foodFactor);
				for (int i = 0; i < nutrition * 2; i++)
				{
					if (generatedNutrition >= nutrition) break;
					Vector2i spawn;
					if (gridMap.FindClosestEmptyTile(new Vector2i((int)(gridMap.Width / 2f), (int)(gridMap.Length / 2f)), 99, out spawn))
					{
						var id = gameMapSettings.startingResources.foodPool[UnityEngine.Random.Range(0, gameMapSettings.startingResources.foodPool.Count)];
						var food = factory.CreateInstance(id).GetFeature<ItemFeature>();
						food.Quantity = 1;						

						generatedNutrition += food.Entity.GetFeature<ConsumableFeature>().StaticData.Nutrition;
						ResourcePileSystem.PlaceStoragePile(spawn, new List<ItemFeature>() { food });

						//Add seeds if it is a crop
						agricultureSystem.ChangeSeeds(food.Entity.StaticData.ID, 1);

						yield return null;
					}
				}
			}
			yield return null;

			if (gameMapSettings.startingResources.weaponsPool != null && gameMapSettings.startingResources.weaponsPool.Count > 0)
			{
				int amount = (int)gameMapSettings.startingResources.weaponsMin + (int)Mathf.Round(gameMapSettings.startingCitizens * gameMapSettings.startingResources.weaponsFactor);
				for (int i = 0; i < amount; i++)
				{
					Vector2i spawn;
					if (gridMap.FindClosestEmptyTile(new Vector2i((int)(gridMap.Width / 2f), (int)(gridMap.Length / 2f)), 99, out spawn))
					{						
						var id = gameMapSettings.startingResources.weaponsPool[UnityEngine.Random.Range(0, gameMapSettings.startingResources.weaponsPool.Count)];
						var weapon = factory.CreateInstance(id).GetFeature<ItemFeature>();
						weapon.Quantity = 1;
						ResourcePileSystem.PlaceStoragePile(spawn, new List<ItemFeature>() { weapon });
						yield return null;
					}
				}
			}
			yield return null;

			if (gameMapSettings.startingResources.materialPool != null && gameMapSettings.startingResources.materialPool.Count > 0)
			{
				int amount = (int)gameMapSettings.startingResources.materialMin + (int)Mathf.Round(gameMapSettings.startingCitizens * gameMapSettings.startingResources.materialFactor);
				for (int i = 0; i < amount; i++)
				{
					Vector2i spawn;
					if (gridMap.FindClosestEmptyTile(new Vector2i((int)(gridMap.Width / 2f), (int)(gridMap.Length / 2f)), 99, out spawn))
					{
						var materialID = gameMapSettings.startingResources.materialPool[UnityEngine.Random.Range(0, gameMapSettings.startingResources.materialPool.Count)];
						ResourcePileSystem.PlaceStoragePile(spawn, materialID, 1);
						yield return null;
					}
				}
			}

			if (gameMapSettings.startingResources.waterMin > 0)
			{
				int amount = (int)gameMapSettings.startingResources.waterMin + (int)Mathf.Round(gameMapSettings.startingCitizens * gameMapSettings.startingResources.waterFactor);
				if (amount > 0)
				{
					Vector2i spawn;
					if (gridMap.FindClosestEmptyTile(new Vector2i((int)(gridMap.Width / 2f), (int)(gridMap.Length / 2f)), 99, out spawn))
					{
						ResourcePileSystem.PlaceStoragePile(spawn, FactoryConstants.WaterID, amount);
					}
				}
			}
		}

		internal IEnumerator GeneratePlayerCity()
		{
			int padding = 32;
			int width = 8;
			int length = 6;
			int street = 3;

			int posx = padding;
			int posy = padding;
			int count = 0;
			int x = 0;
			int y = 0;
			EDirection rotation;

			while (posy < gridMap.Length - padding - length)
			{
				rotation = posy % 2 == 0 ? EDirection.South : EDirection.North;
				posx = padding;
				while (posx < gridMap.Width - padding - width)
				{
					x++;
					count++;
					posx += width;
					if (x % 2 == 0) posx += street;
					TryPlaceStructure("shackhome", new Vector2i(posx, posy), rotation, SystemsManager.MainPlayerFaction,true);

					if (count % 50 == 0) yield return null;
				}
				x = 0;
				y++;
				posy += length;
				if (y % 2 == 0) posy += street;
			}
		}

		void TryPlaceStructure(string id, Vector2i pos, EDirection rotation, int faction, bool updateImmediately)
		{
			BaseEntity resourcePile;
			for (int i = 0; i < 100; i++)
			{
				if (placementSystem.GridObjectTool.CreateStructure(id, faction, pos, rotation, false, out resourcePile, null, updateImmediately))
					break;
			}
		}

		void TryPlaceResourcePile(string id)
		{
			Vector2i pos;
			BaseEntity resourcePile;
			for (int i = 0; i < 100; i++)
			{
				pos.X = CivRandom.Range(gameMapSettings.terrainSettings.safePadding, maxX);
				pos.Y = CivRandom.Range(gameMapSettings.terrainSettings.safePadding, maxY);
				EDirection dir = (EDirection)CivRandom.Range(0, 4);
				if (placementSystem.GridObjectTool.CreateResourcePile(id, pos, dir, out resourcePile, null, false))
					break;
			}
		}

	}
}