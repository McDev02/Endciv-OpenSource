using System;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Responsble to spawn outsiders that visit the town
	/// </summary>
	public class AnimalSpawnSystem : BaseGameSystem, ISaveable, ILoadable<AnimalSpawnSystemSaveData>
	{
		private FactoryManager factories;
		private GridMap gridMap;
		private GameManager gameManager;

		private List<AnimalAIAgentFeature> currentDogs;
		private float newDogCounter;

		public AnimalSpawnSystem(GameManager gameManager, FactoryManager factories) : base()
		{
			this.factories = factories;
			this.gameManager = gameManager;
			gridMap = gameManager.GridMap;

			currentDogs = new List<AnimalAIAgentFeature>();
		}


		public override void UpdateStatistics()
		{
		}

		public override void UpdateGameLoop()
		{
			UpdateWildDogsLogic();
		}

		void UpdateWildDogsLogic()
		{
			var stats = GameStatistics.MainTownStatistics;

			if (newDogCounter <= 0)
			{
				SpawnDog();
				newDogCounter = GameConfig.Instance.GlobalAIData.NewDogTime;
			}
			else
			{
				float factor = 1f / (currentDogs.Count + 0.2f);
				factor = Mathf.Sqrt(Mathf.Max(0, factor));
				newDogCounter -= factor * GameConfig.Instance.GlobalAIData.DogSpawnFactor;
			}
		}

		public void SpawnDog()
		{
			Vector2i spawn;

			if (gridMap.FindRandomEmptyEdgePoint(out spawn))
			{
				var factoryParams = new FactoryParams();
				factoryParams.SetParams
					(
						new GridAgentFeatureParams()
						{
							Position = gridMap.View.GetTileWorldPosition(spawn).To3D()
						},
						new EntityFeatureParams()
						{
							FactionID = SystemsManager.NeutralNpcFaction
						}
					);
				var entity = factories.SimpleEntityFactory.CreateInstance("dog", null, factoryParams);
				var doggo = entity.GetFeature<AnimalAIAgentFeature>();
				if (doggo != null)
					currentDogs.Add(doggo);
				else
					Debug.LogError("Dog entity was missing AnimalAIAgentFeature");
			}
		}

		public ISaveable CollectData()
		{
			var data = new AnimalSpawnSystemSaveData();
			data.newDogCounter = newDogCounter;
			if (currentDogs != null && currentDogs.Count > 0)
			{
				data.currentDogsUIDs = new List<string>(currentDogs.Count);
				for (int i = 0; i < currentDogs.Count; i++)
				{
					data.currentDogsUIDs.Add(currentDogs[i].Entity.UID.ToString());
				}
			}

			return data;
		}

		public void ApplySaveData(AnimalSpawnSystemSaveData data)
		{
			if (data == null)
				return;
			newDogCounter = data.newDogCounter;

			if (data.currentDogsUIDs != null)
			{
				foreach (var workerID in data.currentDogsUIDs)
				{
					Guid id = Guid.Parse(workerID);
					if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
					{
						currentDogs.Add(Main.Instance.GameManager.SystemsManager.Entities[id].GetFeature<AnimalAIAgentFeature>());
					}
				}
			}
		}
	}
}