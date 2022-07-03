using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Endciv
{
	/// <summary>
	/// Will take care of most game logic systems and handles the main game logic loop
	/// </summary>
	public class SystemsManager
	{
		public const int NoFaction = -1;
		public const int MainPlayerFaction = 0;
		public const int NeutralNpcFaction = 1;
		public int Factions { get { return 2; } }

		public GameManager gameManager { get; private set; }
		public TimeManager timeManager { get; private set; }

		//General Systems
		public WeatherSystem WeatherSystem { get; private set; }
		public InventorySystem ResourceSystem { get; private set; }
		public NpcSpawnSystem NpcSpawnSystem { get; private set; }
		public AnimalSpawnSystem AnimalSpawnSystem { get; private set; }
		public NotificationSystem NotificationSystem { get; private set; }
		public InfobarSystem InfobarSystem { get; private set; }

		//Entities
		public EntitySystem EntitySystem { get; private set; }
		//Unit Systems
		public UnitSystem UnitSystem { get; private set; }
		public StructureSystem StructureSystem { get; private set; }
		public PartitionSystem PartitionSystem { get; private set; }
		public GridAgentSystem GridAgentSystem { get; private set; }
		public AIAgentSystem AIAgentSystem { get; private set; }
		public ResourcePileSystem ResourcePileSystem { get; private set; }
		public AIGroupSystem AIGroupSystem { get; private set; }
		//Town Systems
		public PowerSourceSystem PowerSourceSystem { get; private set; }
		public ElectricitySystem ElectricitySystem { get; private set; }
		public TemperatureSystem TemperatureSystem { get; private set; }
		public StorageSystem StorageSystem { get; private set; }
		public HousingSystem HousingSystem { get; private set; }
		public ConstructionSystem ConstructionSystem { get; private set; }
		public ProductionSystem ProductionSystem { get; private set; }
		public AgricultureSystem AgricultureSystem { get; private set; }
		public PastureSystem PastureSystem { get; private set; }
		public JobSystem JobSystem { get; private set; }
		public MiningSystem MiningSystem { get; private set; }
		public GraveyardSystem GraveyardSystem { get; private set; }
		public WasteSystem WasteSystem { get; private set; }
		public UtilitySystem UtilitySystem { get; private set; }

		List<BaseGameSystem>[] systemQueue;
		int systemQueueIterations;


		public bool IsRunning { get; private set; }
		/// <summary>
		/// Tells other classes if the game is within the simulation over multiple frames (later).
		/// We can then queue certain actions and UI commands to execute them in the correct time after all calcualtions are done.
		/// </summary>
		public bool IsInSimulation { get; private set; }

		Stopwatch watch;

		public Dictionary<Guid, BaseEntity> Entities;

		public SystemsManager(GameManager gameManager, TimeManager timeManager)
		{
			this.gameManager = gameManager;
			this.timeManager = timeManager;

			watch = new Stopwatch("SystemsManager");

			EntitySystem = new EntitySystem(Factions, this);

		}

		public void Setup(GameConfig gameConfig, WorldData worldData)
		{
			//Create and run Systems
			WeatherSystem = new WeatherSystem(timeManager, worldData);
			Entities = new Dictionary<Guid, BaseEntity>(128);

			UnitSystem = new UnitSystem(Factions, EntitySystem, timeManager, gameConfig.UnitSystemData);
			StructureSystem = new StructureSystem(gameManager.GridMap, Factions);
			ProductionSystem = new ProductionSystem(Factions, gameManager.Factories.SimpleEntityFactory);
			MiningSystem = new MiningSystem(Factions, gameManager.GridMap, gameConfig.GeneralSystemsData, timeManager, WeatherSystem);
			ResourceSystem = new InventorySystem(Factions, Main.Instance.GameManager.Factories);
			NpcSpawnSystem = new NpcSpawnSystem(gameManager, gameManager.Factories);
			AnimalSpawnSystem = new AnimalSpawnSystem(gameManager, gameManager.Factories);
			ConstructionSystem = new ConstructionSystem(Factions, gameManager.SystemsManager.EntitySystem, gameManager.GridMap);
			UtilitySystem = new UtilitySystem(Factions);
#if USE_GRIDTILE
			PartitionSystem = new PartitionSystem(gameManager.GridMap, 16);
#else
			GridObjectSystem = new GridObjectSystem(gameManager.GridMap, 8);
#endif
			NotificationSystem = new NotificationSystem();
			NotificationSystem.Setup(gameManager.Factories.NotificationFactory, gameManager.gameMapSettings.Scenarios);
			InfobarSystem = new InfobarSystem();
			GridAgentSystem = new GridAgentSystem(Factions, gameManager.GridMap, PartitionSystem);

			PowerSourceSystem = new PowerSourceSystem(WeatherSystem);
			ElectricitySystem = new ElectricitySystem(Factions);
			TemperatureSystem = new TemperatureSystem(WeatherSystem, Factions);

			//Town
			JobSystem = new JobSystem();
			AIAgentSystem = new AIAgentSystem(Factions, gameManager, this);
			AIGroupSystem = new AIGroupSystem(Factions, gameManager, AIAgentSystem.CitizenAISystem);
			HousingSystem = new HousingSystem(Factions, EntitySystem, timeManager);
			StorageSystem = new StorageSystem(Factions, gameManager.Factories.SimpleEntityFactory);

			//These systems have AIAgentSystem dependencies, they must instantiate after it
			AgricultureSystem = new AgricultureSystem(Factions, gameManager.GridMap, timeManager, WeatherSystem, gameManager.Factories.SimpleEntityFactory, gameConfig.AgricultureSystemData);
			PastureSystem = new PastureSystem(Factions, timeManager);
			GraveyardSystem = new GraveyardSystem(Factions, gameManager.Factories.ModelFactory);
			ResourcePileSystem = new ResourcePileSystem(Factions, gameManager);
			WasteSystem = new WasteSystem(Factions, gameManager.GridMap);

			//setup system queue
			systemQueue = new List<BaseGameSystem>[1];
			var queue = new List<BaseGameSystem>();
			queue.Add(NpcSpawnSystem);
			queue.Add(AnimalSpawnSystem);
			queue.Add(WeatherSystem);
			queue.Add(ResourceSystem);

			queue.Add(MiningSystem);
			queue.Add(UtilitySystem);
			queue.Add(PowerSourceSystem);
			queue.Add(ElectricitySystem);
			queue.Add(TemperatureSystem);
			queue.Add(HousingSystem);
			//queue.Add(ConstructionSystem);
			queue.Add(StorageSystem);
			queue.Add(ResourcePileSystem);

			queue.Add(AIGroupSystem);
			queue.Add(JobSystem);

			queue.Add(UnitSystem);
			queue.Add(ProductionSystem);
			queue.Add(AgricultureSystem);
			queue.Add(PastureSystem);

			queue.Add(AIAgentSystem);
			systemQueue[0] = queue;
			systemQueueIterations = systemQueue.Length;
		}

		public void Run()
		{
			//Run Logic main loop
			if (Main.Instance.NoGameUpdate) return;

			gameManager.StartCoroutine(SystemLoop());
		}

		public void Stop()
		{
			IsRunning = false;
		}

		IEnumerator SystemLoop()
		{
			float timer = 0;
			int loopTimer = 0;
			IsRunning = true;
			while (IsRunning)
			{
				//Only run if game is not paused.
				if (!timeManager.IsGamePaused)
				{
					//Fixed Time
					if (timer >= 1)
					{
#if _LogTime
						watch.Reset();
						watch.Start();
#endif
						loopTimer = (loopTimer + 1) % systemQueueIterations;
						timeManager.NextGameTick();
						timer -= 1;

						IsInSimulation = true;
#if _LogTime
						watch.LogRound("Start System Update Loop");
#endif
						var q = systemQueue[loopTimer];
						for (int i = 0; i < q.Count; i++)
						{
							q[i].UpdateGameLoop();
#if _LogTime
							watch.LogRound($"{q[i].SystemName}");
#endif
						}
						IsInSimulation = false;
#if _LogTime
						watch.LogTotal("Systems Tick Update Loop -----------------------------------------");
#endif
					}

					//Run each Frame if not paused
					ConstructionSystem.UpdateGameLoop();
					timeManager.SetTickProgress(timer);
					AIAgentSystem.UpdateEachFrame();

					EntitySystem.UpdateGameLoop();
					GridAgentSystem.UpdateGameLoop();

					timer += Main.deltaTime;
				}

				//Run each frame
				NotificationSystem.UpdateGameLoop();
				yield return null;
			}
			IsRunning = false;
		}

		//Not good, makeshift for initialization timing
		internal void InitializeAfterMapCreation()
		{
			StorageSystem.Run();
			ResourcePileSystem.Run();
			AgricultureSystem.Run();
			PastureSystem.Run();
			GraveyardSystem.Run();
			WasteSystem.Run();
			AIGroupSystem.Run();
		}

		internal void RegisterEntity(BaseEntity entity)
		{
			EntitySystem.RegisterEntity(entity);
			Entities.Add(entity.UID, entity);
		}

		internal void DeregisterEntity(BaseEntity entity)
		{
			if (!Entities.ContainsKey(entity.UID))
				Debug.Log("Entity was not registered!");
			else
			{
				EntitySystem.DeregisterEntity(entity);
				PartitionSystem.UnRegisterStructure(entity);
				if (entity.IsRunning)
					entity.Stop();
				Entities.Remove(entity.UID);
			}
		}
	}
}