using System;
using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
    public enum EMilestoneStatus
    {
        Innactive,
        InProgress,
        Completed
    }    

    public class Milestone : ISaveable, ILoadable<MilestoneSaveData>
    {
        public MilestoneStaticData StaticData;

        public Notification[] objectives;
        private EMilestoneStatus status;
        private NotificationSystem system;

        public EMilestoneStatus Status
        {
            get
            {
                if(status == EMilestoneStatus.InProgress)
                {
                    bool completed = true;
                    foreach(var objective in objectives)
                    {
                        if (objective.status != ENotificationStatus.Complete)
                        {
                            completed = false;
                            break;
                        }                                                    
                    }
                    if (completed)
                        status = EMilestoneStatus.Completed;
                }
                return status;
            }
            set
            {
                status = value;
            }
        }

        public void Setup(MilestoneStaticData staticData, NotificationFactory factory, NotificationSystem system)
        {
            StaticData = staticData;
            objectives = new Notification[staticData.Objectives.Length];
            this.system = system;
            for(int i = 0; i < objectives.Length; i++)
            {
				//Don't add a null check here, we want clean static data.
                objectives[i] = factory.CreateObjective(staticData.Objectives[i].ID, system);                
            }
            system.OnNotificationComplete -= OnObjectiveComplete;
            system.OnNotificationComplete += OnObjectiveComplete;
        }

        private void OnObjectiveComplete(Notification notification)
        {
            if (!objectives.Contains(notification))
                return;
            foreach(var objective in objectives)
            {
                if (objective.status == ENotificationStatus.Complete)
                    continue;
                if (!objective.StaticData.displayObjectiveWindowAutomatically)
                    continue;
                Main.Instance.GameManager.GameGUIController.ShowObjectiveWindow(objective.GetPages(), objective.StaticData.Title);
                break;
            }
        }

        public void Run()
        {
            if(status == EMilestoneStatus.Innactive)
                status = EMilestoneStatus.InProgress;
            foreach(var objective in objectives)
            {
                objective.Run();
                if(objective.status == ENotificationStatus.Untriggered)
                    objective.status = ENotificationStatus.Triggered;
            }
            if(objectives.Length > 0)
            {
                Main.Instance.GameManager.GameGUIController.ShowObjectiveWindow(objectives[0].GetPages(), objectives[0].StaticData.Title);
            }
        }
        

        public virtual void OnMilestoneComplete()
        {
            system.OnNotificationComplete -= OnObjectiveComplete;
        }

        public ISaveable CollectData()
        {
            var data = new MilestoneSaveData();
            data.status = (int)status;
            data.objectiveData = new Dictionary<string, NotificationSaveData>();
            foreach(var objective in objectives)
            {
                data.objectiveData.Add(objective.StaticData.ID, (NotificationSaveData)objective.CollectData());
            }
            return data;
        }

        public void ApplySaveData(MilestoneSaveData data)
        {
            if (data == null)
                return;
            status = (EMilestoneStatus)data.status;
            foreach(var obj in data.objectiveData)
            {
                var objective = objectives.FirstOrDefault(x => x.StaticData.ID == obj.Key);
                if (objective == null)
                    continue;
                objective.ApplySaveData(obj.Value);
            }
        }

    }

}
