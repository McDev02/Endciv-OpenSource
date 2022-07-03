using System.Collections.Generic;
using UnityEngine;
using System;

namespace Endciv
{
	[Serializable]
	public class GraveyardFeature : Feature<GraveyardFeatureSaveData>
	{
		//Static Data
		public GraveyardStaticData StaticData { get; private set; }

		private Vector2i[] graveSpots;
		private GraveModelView[] occupiedGraves;
		private HashSet<int> reservedPlotIDs;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<GraveyardStaticData>();
		}

		private SystemsManager manager;
		private ModelFactory factory;

		//Methods
		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			factory = Main.Instance.GameManager.Factories.ModelFactory;
			manager.GraveyardSystem.RegisterFeature(this);
			base.Run(manager);
			CalculateGraveSpots();
		}

		private void CalculateGraveSpots()
		{
			var gridData = Entity.GetFeature<GridObjectFeature>().GridObjectData;
			int graveCount = GraveyardSystem.GetGraveyardSpots(gridData);
			graveSpots = new Vector2i[graveCount];
			occupiedGraves = new GraveModelView[graveCount];
			int graveIndex = 0;
			for (int x = 0; x < gridData.Rect.Width + 1; x++)
			{
				for (int y = 0; y < gridData.Rect.Length - 1; y += 3)
				{
					var gravePos = gridData.Rect.BottomLeft + new Vector2i(x, y);
					graveSpots[graveIndex] = gravePos;
					graveIndex++;
				}
			}
			reservedPlotIDs = new HashSet<int>();
		}

		public override void Stop()
		{
			manager.GraveyardSystem.DeregisterFeature(this);
			base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.GraveyardSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.GraveyardSystem.RegisterFeature(this);
		}

		public bool HasSpace()
		{
			for (int i = 0; i < occupiedGraves.Length; i++)
			{
				if (occupiedGraves[i] == null && !reservedPlotIDs.Contains(i))
					return true;
			}
			return false;
		}

		public float AddConstructionPoints(int graveID, float increment)
		{
			if (occupiedGraves[graveID] == null)
				return 1f;
			occupiedGraves[graveID].ConstructionProgress += increment;
			return occupiedGraves[graveID].ConstructionProgress;
		}

		public int ReserveGravePlot()
		{
			int id = GetEmptyGravePlot();
			if (id > -1)
			{
				reservedPlotIDs.Add(id);
			}
			return id;
		}

		public void UnreserveGravePlot(int plotID)
		{
			if (plotID == -1)
				return;
			if (reservedPlotIDs.Contains(plotID))
			{
				reservedPlotIDs.Remove(plotID);
			}
		}

		public int GetEmptyGravePlot()
		{
			if (graveSpots.Length <= 0)
				return -1;
			List<int> emptyPlotIDs = new List<int>();
			for (int i = 0; i < occupiedGraves.Length; i++)
			{
				if (occupiedGraves[i] != null)
					continue;
				emptyPlotIDs.Add(i);
			}
			if (emptyPlotIDs.Count <= 0)
				return -1;
			return emptyPlotIDs[UnityEngine.Random.Range(0, emptyPlotIDs.Count)];
		}

		public int PlaceGrave(GraveModelView grave, int variationID, int plotID)
		{
			if (reservedPlotIDs.Contains(plotID))
				UnreserveGravePlot(plotID);
			var pos2D = graveSpots[plotID];
			var pos3D = Main.Instance.GameManager.GridMap.View.GetTileWorldPosition(pos2D).To3D(Entity.GetFeature<EntityFeature>().View.transform.position.y);
			grave.transform.parent = Entity.GetFeature<StructureFeature>().View.transform;
			grave.transform.position = pos3D;
			occupiedGraves[plotID] = grave;
			grave.Setup(plotID, variationID);
			return plotID;
		}

		public Vector2i GetPlotCoordinates(int plotID)
		{
			return graveSpots[plotID];
		}

		private void TestGravePlacement()
		{
			for (int i = 0; i < graveSpots.Length; i++)
			{
				int var;
				var viewModel = factory.GetRandomModelObject<GraveModelView>("grave", out var);
				viewModel.name = "grave";
				PlaceGrave(viewModel, var, i);
			}
		}

		public override ISaveable CollectData()
		{
			var data = new GraveyardFeatureSaveData();
			if (occupiedGraves != null && occupiedGraves.Length > 0)
			{
				data.occupiedGraves = new List<GraveSaveData>(occupiedGraves.Length);
				for (int i = 0; i < occupiedGraves.Length; i++)
				{
					if (occupiedGraves[i] == null)
						continue;
					data.occupiedGraves.Add((GraveSaveData)occupiedGraves[i].CollectData());
				}
			}
			else
				data.occupiedGraves = new List<GraveSaveData>();
			data.reservedPlotIDs = new int[reservedPlotIDs.Count];
			int count = 0;
			foreach (var id in reservedPlotIDs)
			{
				data.reservedPlotIDs[count] = id;
			}
			return data;
		}

		public override void ApplyData(GraveyardFeatureSaveData data)
		{
			reservedPlotIDs = new HashSet<int>();
			foreach (var id in data.reservedPlotIDs)
			{
				reservedPlotIDs.Add(id);
			}
			foreach (var grave in data.occupiedGraves)
			{
				var model = manager.gameManager.Factories.ModelFactory.GetModelObject<GraveModelView>("grave", grave.variationID);
				PlaceGrave(model, grave.variationID, grave.positionIndex);
				model.ApplySaveData(grave);
			}

		}

	}
}