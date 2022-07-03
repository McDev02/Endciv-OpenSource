
using System;
using UnityEngine;

namespace Endciv
{
	public class UserToolSystem
	{
		public enum EToolState
		{
			None,
			Selection,
			PlaceGridObject,
			PlaceUnit,
			Order,
			Reservation
		}
		public EToolState CurrentState { get; private set; }
		private EToolState lastState;

		public UserTool_Unit UnitTool { get; private set; }
		public UserTool_GridObject GridObjectTool { get; private set; }
		public UserTool_Order OrderTool { get; private set; }
		public UserTool_Reservation ReservationTool { get; private set; }
		public UserTool_Selection SelectionTool { get; private set; }
		public UserTool CurrentTool { get; private set; }
		private GameInputManager gameInputManager;
		private UserToolsView userToolsView;

		public Action OnToolChanged;

		private GameManager GameManager;
		private FactoryManager Factories;

		//Let this handle the InputManager,
		Vector3 oldMouse;

		public UserToolSystem(GameManager gameManager, UserToolsView userToolsView, FactoryManager factories, GameInputManager gameInputManager)
		{
			this.userToolsView = userToolsView;
			GameManager = gameManager;
			Factories = factories;
			this.gameInputManager = gameInputManager;

			GridObjectTool = new UserTool_GridObject(this, userToolsView, GameManager, Factories.SimpleEntityFactory);
			UnitTool = new UserTool_Unit(this, userToolsView, GameManager, Factories.SimpleEntityFactory);
			OrderTool = new UserTool_Order(this, GameManager.GameGUIController, gameInputManager);
			ReservationTool = new UserTool_Reservation(this, userToolsView, GameManager);
			SelectionTool = new UserTool_Selection(gameInputManager);
		}

		public void Run()
		{
			SwitchState(EToolState.Selection);
		}

		public void SetTolastState()
		{
			SwitchState(lastState);
		}

		public void SwitchState(EToolState state)
		{
			lastState = CurrentState;
			if (CurrentTool != null)
				CurrentTool.Stop();
			CurrentState = state;
			CurrentTool = null;

			GameManager.TerrainManager.terrainView.ShowGrid = false;
			switch (state)
			{
				case EToolState.None:
					CurrentTool = null;
					break;
				case EToolState.Selection:
					CurrentTool = SelectionTool;
					break;
				case EToolState.PlaceGridObject:
					CurrentTool = GridObjectTool;
					GameManager.TerrainManager.terrainView.ShowGrid = true;
					break;
				case EToolState.PlaceUnit:
					CurrentTool = UnitTool;
					break;
				case EToolState.Order:
					CurrentTool = OrderTool;
					break;
				case EToolState.Reservation:
					CurrentTool = ReservationTool;
					GameManager.TerrainManager.terrainView.ShowGrid = true;
					break;
				default:
					CurrentTool = SelectionTool;
					break;
			}

			OnToolChanged?.Invoke();
			gameInputManager.restrictCameraRotation = CurrentState == EToolState.PlaceGridObject;
		}

		//Interface
		public void PlaceStructure(string id)
		{
			SwitchState(EToolState.PlaceGridObject);
			GridObjectTool.UIPlaceStructure(id);
		}
		public void PlaceResourcePile(string id)
		{
			SwitchState(EToolState.PlaceGridObject);
			GridObjectTool.UIPlaceResourcePile(id);
		}
		public void CycleNextView()
		{
			GridObjectTool.CycleNextView();
		}
		public void PlaceUnit(string id, int batch = 1, float radius = 0)
		{
			var gender = UnityEngine.Random.value <= UnitSystem.GenderGenerationThreshold ? ELivingBeingGender.Male : ELivingBeingGender.Female;
			var age = UnityEngine.Random.value <= UnitSystem.AdultGenerationThreshold ? ELivingBeingAge.Adult : ELivingBeingAge.Child;
			PlaceUnit(id, age, gender, batch, radius);
		}
		public void PlaceUnit(string id, ELivingBeingAge age, ELivingBeingGender gender, int batch = 1, float radius = 0)
		{
			SwitchState(EToolState.PlaceUnit);
			if (batch <= 1)
				UnitTool.UIPlaceUnit(id, gender, age);
			else
				UnitTool.UIPlaceUnitBatch(id, batch, gender, age, radius);
		}
		//Orders
		public void Order(UserTool_Order.EOrderType type)
		{
			SwitchState(EToolState.Order);
			OrderTool.Order(type);
		}
		public void OrderGathering()
		{
			SwitchState(EToolState.Order);
			OrderTool.Order(UserTool_Order.EOrderType.Collection);
		}
		public void OrderDemolition()
		{
			SwitchState(EToolState.Order);
			OrderTool.Order(UserTool_Order.EOrderType.Demolition);
		}
		public void OrderReservation()
		{
			SwitchState(EToolState.Reservation);
			ReservationTool.Reservation();
		}

		internal void Process()
		{
			if (Input.GetMouseButtonDown(1))
				oldMouse = Input.mousePosition;
			if (Input.GetMouseButtonUp(1) && (oldMouse - Input.mousePosition).magnitude <= 0.1f)
			{
				SwitchState(EToolState.Selection);
			}

			if (CurrentTool != null)
				CurrentTool.Process();
		}
	}
}