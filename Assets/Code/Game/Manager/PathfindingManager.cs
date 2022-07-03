using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

namespace Endciv
{
	public class PathfindingManager : MonoBehaviour
	{
		public static PathfindingManager Instance;

		/// <summary>
		/// The default number of maximum enqueued search requests jobs.
		/// </summary>
		public const int MAX_ENQUEUED_JOBS = 1024;
		public const int JOB_BATCH_SIZE = 4;

		public List<Gridfield> gridfields;
#if USE_FLOWFIELDS
		[SerializeField] FlowfieldView FlowfieldView;
#endif
		List<PathfinderJob> PathfinderJobs;
		volatile Stack<PathfinderJob> NewPathfinderJobs;
		volatile Stack<PathfinderJob> CancelPathfinderJobs;
		bool IsReady;

		public PathfinderJob recentJob;

		public int CurrentJobs;
		public int NewJobs;
		public volatile string Message;

		bool HasUpdatedMap;
		GridMap gridMap;

		/// <summary>
		/// The main Thread object.
		/// </summary>
		private Thread Thread;
		/// <summary>
		/// A flag to stop the main thread.
		/// </summary>
		private bool ThreadRun = false;

		public void Setup(GameManager gameManager)
		{
			Instance = this;
			NewPathfinderJobs = new Stack<PathfinderJob>(32);
			CancelPathfinderJobs = new Stack<PathfinderJob>(16);
			PathfinderJobs = new List<PathfinderJob>((int)(MAX_ENQUEUED_JOBS / 8f));
			//Todo add batches and job system

			gridMap = gameManager.GridMap;
			gridfields = new List<Gridfield>(JOB_BATCH_SIZE);
#if USE_FLOWFIELDS
			FlowfieldView.Setup(Flowfield);
#endif
			IsReady = true;
		}

		public bool GetPathfindingJob(Vector2i Goal, ref PathfinderJob job, float openAreaMinValue = 0)
		{
			return GetPathfindingJob(new Location(Goal), ref job, openAreaMinValue);
		}
		public bool GetPathfindingJob(Vector2i Goal, Vector2i from, EPathfindingMode pathfindingMode, ref PathfinderJob job, float openAreaMinValue = 0)
		{
			return GetPathfindingJob(new Location(Goal), new Location(from), pathfindingMode, ref job, openAreaMinValue);
		}

		public bool GetPathfindingJob(Location destination, ref PathfinderJob job, float openAreaMinValue = 0)
		{
			return GetPathfindingJob(destination, null, EPathfindingMode.SearchAll, ref job, openAreaMinValue);
		}
		public bool GetPathfindingJob(Location destination, Location origin, EPathfindingMode pathfindingMode, ref PathfinderJob job, float openAreaMinValue = 0)
		{
			if (job == null)
			{
#if USE_FLOWFIELDS
                job = new PathfinderJob(Flowfield.GetNewFlowmap());
#else
				job = new PathfinderJob();
#endif
			}
			job.IsReady = false;
			lock (NewPathfinderJobs)
			{
				if (PathfinderJobs.Count + NewPathfinderJobs.Count > MAX_ENQUEUED_JOBS)
				{
					Debug.Log("Max queued jobs reached.");
					return false;
				}
			}
			validatePath(destination, origin);
			
			job.Reset(destination, origin, pathfindingMode, GameConfig.Instance.pathfindingTargetReachedBias, openAreaMinValue);
			if (job.Destination.Type == Location.EDestinationType.Waypoint)
				Debug.LogError("Job type is Waypoint, this can not happen!");

			lock (NewPathfinderJobs)
			{
				NewPathfinderJobs.Push(job);
				if (job == null)
					Debug.LogError("Job is null!");
			}
			return true;
		}

		void validatePath(Location destination, Location origin)
		{
			var indicies = origin.Indecies;
			for (int i = 0; i < indicies.Length; i++)
			{
				if (!gridMap.IsPassable(indicies[i]))
					Debug.LogWarning($"Origin index {indicies[i].ToString()} is not passable");
			}
			indicies = destination.Indecies;
			if (destination.Type == Location.EDestinationType.Waypoint || destination.Type == Location.EDestinationType.Position)
			{
				for (int i = 0; i < indicies.Length; i++)
				{
					if (!gridMap.IsPassable(indicies[i]))
						Debug.LogError($"Destination index {indicies[i].ToString()} is not passable");
				}
			}
		}

		internal void CancelPathfindingJob(PathfinderJob job)
		{
			if (!CancelPathfinderJobs.Contains(job))
				CancelPathfinderJobs.Push(job);
		}

		public void Run()
		{
			StartThread();
		}

		private void Update()
		{
			if (!IsReady) return;

			if (!HasUpdatedMap) return;
			HasUpdatedMap = false;

			if (!string.IsNullOrEmpty(Message)) Debug.LogError(Message);
			Message = null;
	
#if USE_FLOWFIELDS
			FlowfieldView.UpdateMap();
#endif
		}

		/// <summary>
		/// Starts the pathfinder job dispatcher thread.
		/// Todo: Maybe change to actual C# job system.
		/// </summary>
		void StartThread()
		{
			Debug.Log("Start Pathfinder Thread");
			if (Thread != null && Thread.IsAlive)
				return;

			ThreadRun = true;

			Thread = new Thread(RunThread);
			Thread.IsBackground = true;
			Thread.Start();
		}

		/// <summary>
		/// Stops the pathfinder job dispatcher thread.
		/// </summary>
		public void StopThread()
		{
			Debug.Log("Stop Pathfinder Thread");
			if (Thread == null)
				return;

			ThreadRun = false;

			if (!Thread.Join(10000))
			{
				Thread.Abort();
			}

			Thread = null;
		}

		private async void RunThread()
		{
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			while (ThreadRun)
			{
				int batch = 0;
				System.Threading.Tasks.Task[] tasks = null;
				//lock (PathfinderJobs)
				//{
				//Cancel jobs
				if (false)  //This will not work yet, we have to rework, if we even need it
				{
					lock (CancelPathfinderJobs)
					{
						while (CancelPathfinderJobs.Count > 0)
						{
							var job = CancelPathfinderJobs.Pop();
							if (PathfinderJobs.Contains(job)) PathfinderJobs.Remove(job);
						}
					}
				}

				//Transfer new jobs
				lock (NewPathfinderJobs)
				{
					while (NewPathfinderJobs.Count > 0)
					{
						var newjob = NewPathfinderJobs.Pop();
						if (newjob == null) Debug.LogError("New job is already null!");
						else
							PathfinderJobs.Add(newjob);
					}
					NewJobs = NewPathfinderJobs.Count;
					CurrentJobs = PathfinderJobs.Count;
				}

				batch = Math.Min(JOB_BATCH_SIZE, PathfinderJobs.Count);
				tasks = new System.Threading.Tasks.Task[batch];
				for (int i = 0; i < batch; i++)
				{
					var job = PathfinderJobs[i];

					if (gridfields.Count <= i)
						gridfields.Add(new Gridfield(gridMap.Data, gridMap.Grid));

					var myGridfield = gridfields[i];

					tasks[i] = System.Threading.Tasks.Task.Run(() =>
					{
						recentJob = job;
						if (job == null)
						{
							Message = "Job is null!";
							Thread.Sleep(1);
						}
						else
						{
							if (job.IsReady)
							{
								Message = "Job is already marked as Ready!";
								job.IsReady = false;
								Thread.Sleep(1);
							}
#if USE_FLOWFIELDS
                            Gridfield.CalculateFlowmap(job);
#else
							try
							{
								myGridfield.CalculatePath(job);
							}
							catch (Exception e)
							{
								Message = "Task error: " + e.ToString();
							}
#endif

							job.IsReady = true;
						}
					});
				}
				//}
				if (tasks != null && tasks.Length > 0)
				{
					await System.Threading.Tasks.Task.WhenAll(tasks);
					for (int i = 0; i < batch; i++)
					{
						var job = PathfinderJobs[i];
						if (!job.IsReady)
							Message = "Job is not ready at the end!";
					}
					PathfinderJobs.RemoveRange(0, batch);
					CurrentJobs = PathfinderJobs.Count;
				}

				HasUpdatedMap = true;
				// prevent system 100% cpu usage
				Thread.Sleep(1);
			}

			Debug.Log("Pathfinder Thread has Reached End");
		}
	}
}