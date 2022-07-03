using System;
using System.Collections.Generic;

namespace Endciv
{
	public class ExpeditionFeature : Feature<ExpeditionFeatureSaveData>, 
		IViewController<ExpeditionView>
	{
		public ExpeditionView view;

		public Dictionary<CitizenAIAgentFeature, AIGroupSystem.EAssigneeState> assignees = new Dictionary<CitizenAIAgentFeature, AIGroupSystem.EAssigneeState>();

		/// <summary>
		/// Time when the expedition started in ticks
		/// </summary>
		public int tickWhenExpeditionStarted;
		public EState state;
		public int timer;
		public Location gatherLocation;
		public Location expeditionLocation;

		public Location CurrentLocation
		{
			get
			{
				return state == EState.Gathering ? gatherLocation : expeditionLocation;
			}
		}

		public enum EState { None, Gathering, Started, Active, Finished }

		public void SetState(CitizenAIAgentFeature citizen, AIGroupSystem.EAssigneeState state)
		{
			if (!assignees.ContainsKey(citizen))
				assignees.Add(citizen, state);
			else assignees[citizen] = state;
		}

		private InfobarSystem infoSystem;

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			infoSystem = manager.InfobarSystem;
		}

		public override void Stop()
		{
			base.Stop();
			infoSystem.UnregisterEntity(EInfobarCategory.Exploration, Entity, true);
		}

		#region IViewController

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			this.view = (ExpeditionView)view;
		}

		public void ShowView()
		{
			view.ShowHide(true);
			infoSystem.RegisterEntity(EInfobarCategory.Exploration, Entity, true);
		}

		public void HideView()
		{
			view.ShowHide(false);
			infoSystem.UnregisterEntity(EInfobarCategory.Exploration, Entity, true);
		}

		public void UpdateView()
		{
			view.UpdateView();
		}

		public void SelectView()
		{
			view.OnViewSelected();
		}

		public void DeselectView()
		{
			view.OnViewDeselected();
		}
		#endregion

		public override void ApplyData(ExpeditionFeatureSaveData data)
		{
			tickWhenExpeditionStarted = data.tickWhenExpeditionStarted;
			state = (EState)data.state;
			timer = data.timer;
			if (data.expeditionLocation != null)
				expeditionLocation = data.expeditionLocation.ToLocation();
			if (data.gatherLocation != null)
				gatherLocation = data.gatherLocation.ToLocation();
			assignees = new Dictionary<CitizenAIAgentFeature, AIGroupSystem.EAssigneeState>();
			if(data.assignees != null)
			{
				foreach(var pair in data.assignees)
				{
					var guid = Guid.Parse(pair.Key);
					var entity = Main.Instance.GameManager.SystemsManager.Entities[guid];
					assignees.Add(entity.GetFeature<CitizenAIAgentFeature>(), (AIGroupSystem.EAssigneeState)pair.Value);
				}
			}
			if (data.isVisible)
				ShowView();
			else
				HideView();			
			view.SetTooltip(data.tooltip);
		}

		public override ISaveable CollectData()
		{
			var data = new ExpeditionFeatureSaveData();
			data.tickWhenExpeditionStarted = tickWhenExpeditionStarted;
			data.state = (int)state;
			data.timer = timer;
			if(expeditionLocation != null)
				data.expeditionLocation = (LocationSaveData)expeditionLocation.CollectData();
			if(gatherLocation != null)
				data.gatherLocation = (LocationSaveData)gatherLocation.CollectData();
			data.assignees = new Dictionary<string, int>(assignees.Count);
			foreach(var pair in assignees)
			{
				data.assignees.Add(pair.Key.Entity.UID.ToString(), (int)pair.Value);
			}
			data.isVisible = (view != null && view.IsVissible);
			data.tooltip = view.GetTooltip();
			return data;
		}		
	}
}
