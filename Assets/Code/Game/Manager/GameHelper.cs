using UnityEngine;
using System.Collections;
using Endciv;

public class GameHelper
{
	private GameManager gameManager;

	public GameHelper(GameManager gameManager)
	{
		this.gameManager = gameManager;
	}

	BaseEntity lastCycledCitizen;
	StructureFeature lastCycledStructure;

	public void OnCycleThroughBuildings()
	{
		bool findFirstEntity = lastCycledStructure != null;
		var structures = gameManager.SystemsManager.StructureSystem.FeaturesByFaction[SystemsManager.MainPlayerFaction];
		for (int i = 0; i < structures.Count; i++)
		{
			var structure = structures[i];
			if (structure == null)
				continue;

			if (findFirstEntity)
			{
				if (structure == lastCycledStructure)
					findFirstEntity = false;
				if (i >= structures.Count - 1)
				{
					findFirstEntity = false;
					i = -1;
				}
			}
			else
			{
				var gridObject = structure.Entity.GetFeature<GridObjectFeature>();
				if (gridObject != null)
				{
					lastCycledStructure = structure;
					gameManager.CameraController.SetPosition(gridObject.GetPosition());
					break;
				}
			}
		}
	}
	public void OnCycleThroughUnits()
	{
		bool findFirstEntity = lastCycledCitizen != null;

		var citizens = gameManager.SystemsManager.UnitSystem.Citizens[SystemsManager.MainPlayerFaction];
		for (int i = 0; i < citizens.Count; i++)
		{
			var citizen = citizens[i];
			if (citizen == null)
				continue;

			if (findFirstEntity)
			{
				if (citizen == lastCycledCitizen)
					findFirstEntity = false;
				if (i >= citizens.Count - 1)
				{
					findFirstEntity = false;
					i = -1;
				}
			}
			else
			{
				var entity = citizen.GetFeature<EntityFeature>();
				if (entity != null)
				{
					lastCycledCitizen = citizen;
					gameManager.CameraController.SetPosition(entity.View.transform.position.To2D());
					break;
				}
			}
		}
	}
}