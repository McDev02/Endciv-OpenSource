using UnityEngine;
using System.Collections;
using System;

namespace Endciv
{
	public class UiUserInput : MonoBehaviour
	{
		GameGUIController gameGUIController;
		GameManager gameManager;
		GameInputManager gameInputManager;

		bool isRunning;

		public void OnGatherTool()
		{
			gameManager.UserToolSystem.OrderGathering();
		}
		public void OnDemolitionTool()
		{
			gameManager.UserToolSystem.OrderDemolition();
		}
		public void OnReservationTool()
		{
			gameManager.UserToolSystem.OrderReservation();
		}

		public void OnCycleThroughBuildings()
		{
			gameManager.gameHelper.OnCycleThroughBuildings();
		}
		public void OnCycleThroughUnits()
		{
			gameManager.gameHelper.OnCycleThroughUnits();
		}
	

		internal void Setup(GameGUIController gameGUIController, GameManager gameManager)
		{
			this.gameGUIController = gameGUIController;
			this.gameManager = gameManager;
			gameInputManager = gameManager.gameInputManager;
			isRunning = true;
		}

		private void Update()
		{
			if (!isRunning) return;
			if (gameInputManager.GetActionDown("CycleThroughCitizens"))
				gameManager.gameHelper.OnCycleThroughUnits();
			if (gameInputManager.GetActionDown("CycleThroughStructures"))
				gameManager.gameHelper.OnCycleThroughBuildings();
		}
	}
}