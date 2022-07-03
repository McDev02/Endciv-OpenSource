using UnityEngine;

namespace Endciv.Testing
{
	public class TestCheatInputs : MonoBehaviour
	{
		[SerializeField] GameManager MyGameManager;
		GameInputManager InputManager;
		UserToolSystem PlacementSystem;
		FactoryManager Factories;
		bool IsRunning;

		[SerializeField] int SpawnUnits = 1;
		[SerializeField] float SpawnUnitsRadius = 0;

		enum ECheatUnit { human, dog, cow, chicken, trader_car, COUNT }
		ECheatUnit lastUnit;
#if DEV_MODE || UNITY_EDITOR
		void Awake()
		{
			IsRunning = false;
			MyGameManager.OnGameRun += Run;

		}

		void Run()
		{
			IsRunning = true;
			InputManager = MyGameManager.gameInputManager;
			Factories = MyGameManager.Factories;
			PlacementSystem = MyGameManager.UserToolSystem;
		}

		// Update is called once per frame
		void Update()
		{
			if (!IsRunning || !InputManager.IsGameInputAllowed) return;

			//Place and toggle through Units
			if (Input.GetKeyDown(KeyCode.U))
			{
				int id = (int)lastUnit;
				if (MyGameManager.UserToolSystem.CurrentState == UserToolSystem.EToolState.PlaceUnit)
					id++;
				if (id >= (int)ECheatUnit.COUNT)
					id = 0;
				lastUnit = (ECheatUnit)id;
				MyGameManager.UserToolSystem.PlaceUnit(lastUnit.ToString(), SpawnUnits, SpawnUnitsRadius);
			}
			if (Input.GetKeyDown(KeyCode.Delete))
			{
				var entity = MyGameManager.UserToolSystem.SelectionTool.SelectedEntity;
				if (entity != null)
				{
					if (entity.GetFeature<EntityFeature>().IsAlive)
						MyGameManager.SystemsManager.EntitySystem.KillEntity(entity);
					else
						MyGameManager.SystemsManager.EntitySystem.DestroyEntity(entity);
				}
			}
		}
#endif
	}
}