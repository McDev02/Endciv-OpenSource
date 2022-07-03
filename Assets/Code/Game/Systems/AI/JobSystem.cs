using System.Collections.Generic;
namespace Endciv
{
	public enum EOccupation { None, Labour, Construction, Scavenger, Supply, Production, Farmer, Herder, COUNT }

	public class JobSystem : BaseGameSystem
	{
		public List<IAIJob> Jobs { get; private set; }
		/// <summary>
		/// Jobs listed by occupation
		/// </summary>
		/// <summary>
		/// a list of jobs that have tasks listed by occupation
		/// </summary>
		List<IAIJob>[] TasksByOccupation;

		//Maybe add lists for jobs that still require workers


		public JobSystem() : base()
		{
			Jobs = new List<IAIJob>(16);
			TasksByOccupation = new List<IAIJob>[(int)EOccupation.COUNT];
			for (int i = 0; i < TasksByOccupation.Length; i++)
			{
				TasksByOccupation[i] = new List<IAIJob>(8);
			}
		}

		internal void RegisterJob(IAIJob job)
		{
			if (!Jobs.Contains(job))
			{
				Jobs.Add(job);
				var occupations = job.Occupations;
				if (occupations == null || occupations.Length <= 0)
					return;
				foreach (var occupation in occupations)
				{
					TasksByOccupation[(int)occupation].Add(job);
				}
			}
		}
		public override void UpdateStatistics()
		{
		}

		internal void DeregisterJob(IAIJob job)
		{
			if (Jobs.Contains(job))
			{
				Jobs.Remove(job);
				var occupations = job.Occupations;
				if (occupations == null || occupations.Length <= 0)
					return;
				foreach (var occupation in occupations)
				{
					TasksByOccupation[(int)occupation].Remove(job);
				}
			}
		}

		internal void RegisterNeed(IAIJob job, EOccupation occupation)
		{
			//Maybe add sorting by priority?
			var jobs = TasksByOccupation[(int)occupation];
			if (!jobs.Contains(job))
				jobs.Add(job);
		}
		internal void DeregisterNeed(IAIJob job, EOccupation occupation)
		{
			var jobs = TasksByOccupation[(int)occupation];
			if (jobs.Contains(job))
				jobs.Remove(job);
		}


		public override void UpdateGameLoop()
		{
		}


		public AITask GetTaskByOccupation(EOccupation occupation, AIAgentFeatureBase agent)
		{
			var jobs = TasksByOccupation[(int)occupation];
			if (jobs == null || jobs.Count <= 0)
				return null;

			//Here we need a List of Tasks sorted by priority.

			SortedList<float, AITask> tasks = new SortedList<float, AITask>(new DuplicateKeyComparerDescending<float>());
			for (int i = 0; i < jobs.Count; i++)
			{
				var job = jobs[i];
				if (job.WorkerCount >= job.MaxWorkers)
					continue;
                AITask task = job.AskForTask(occupation, agent);
				if (task == null)
					continue;

				//Find task
				float unitPriority = 1; //This will be used for distance related priority, far away tasks are less important.
				tasks.Add(task.priority * unitPriority, task);
			}

			//Now the highest ranking task (first or last entry?) is what we want. Usually this should always be returned, we do this for loop for bug prevention.
			//This could be the case if GetTask returns something different than AskFor Task.
			foreach (var pair in tasks)
			{
				var worktask = pair.Value;
				var job = worktask.job;
				if (worktask != null)
				{
					worktask.Initialize();                    
					if (job != null && job.IsWorkplace)
					{
                        job.OnTaskStart();
                        job.RegisterWorker((CitizenAIAgentFeature)agent, worktask.workerType);
					}

					return worktask;
				}
			}
			return null;//No task found
		}		
	}
}