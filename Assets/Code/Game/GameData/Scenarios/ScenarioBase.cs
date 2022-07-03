using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endciv
{	
	public abstract class ScenarioBase : MonoBehaviour, ISaveable, ILoadable<ScenarioSaveData>
	{
		public MilestoneStaticData[] Milestones;

		protected Milestone[] milestones;

		protected int currentMilestoneID;

		protected NotificationSystem system;
		protected GameManager gameManager;
		protected NotificationSystem notificationSystem;

		public void Setup(GameManager gameManager, NotificationSystem notificationSystem)
		{
			this.notificationSystem = notificationSystem;
			this.gameManager = gameManager;
			this.system = gameManager.SystemsManager.NotificationSystem;

			milestones = new Milestone[Milestones.Length];

			var factory = gameManager.Factories.NotificationFactory;
			for (int i = 0; i < milestones.Length; i++)
			{
				milestones[i] = factory.CreateMilestone(Milestones[i].ID, system);
			}
		}

		internal abstract void UpdateGameLoop();
		internal abstract void MilestoneChanged();

		public abstract void Run();

		public Milestone GetCurrentMilestone()
		{
			if (currentMilestoneID >= milestones.Length)
				return null;
			return milestones[currentMilestoneID];
		}

		public Milestone GetΝextMilestone()
		{
			currentMilestoneID++;
			MilestoneChanged();
			return GetCurrentMilestone();
		}

		public virtual ISaveable CollectData()
		{
			var data = new ScenarioSaveData();
			data.currentMilestoneID = currentMilestoneID;
			data.milestoneData = new Dictionary<string, MilestoneSaveData>();
			foreach (var milestone in milestones)
			{
				data.milestoneData.Add(milestone.StaticData.ID, (MilestoneSaveData)milestone.CollectData());
			}
			return data;
		}

		public virtual void ApplySaveData(ScenarioSaveData data)
		{
			if (data == null)
				return;
			foreach (var mile in data.milestoneData)
			{
				var milestone = milestones.FirstOrDefault(x => x.StaticData.ID == mile.Key);
				if (milestone == null)
					continue;
				milestone.ApplySaveData(mile.Value);
			}
			currentMilestoneID = data.currentMilestoneID;
			MilestoneChanged();
		}

		internal void CloseWindows()
		{
		}

		internal void nextPage()
		{
		}
		internal void previousPage()
		{
		}
	}
}