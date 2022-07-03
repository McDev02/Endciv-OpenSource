using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace McLOD
{
	public class McLOD : MonoBehaviour
	{
		[SerializeField] int MaxBatches = 4;
		public static McLOD Instance;
		public bool UseCulling = true;
		public bool LogTime = true;
		public bool RunAsynch;
		public int MaxAsynchBatchCount = 1000;

		Coroutine AsynchUpdateRoutine;

		public Transform Camera;
		//Dictionary<int, List<IMcLODEntity>> EntityGroups;
		List<IMcLODEntity>[] Entities;
		SortedList<float, IMcLODEntity> EntitiesByDistance;

		const bool BatchOrDistance = true;

		int lastBatchID;
		public int TotalEntities;

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

		void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("Only run one instance of the LODSystem. This will be deactivated.");
				enabled = false;
			}
			else Instance = this;
			//EntityGroups = new Dictionary<int, List<IMcLODEntity>>(4);
			Entities = new List<IMcLODEntity>[MaxBatches];
			EntitiesByDistance = new SortedList<float, IMcLODEntity>();
			if (BatchOrDistance)
			{
				for (int i = 0; i < MaxBatches; i++)
				{
					Entities[i] = new List<IMcLODEntity>(32);
				}
			}
			TotalEntities = 0;
		}
		void Start()
		{
			if (RunAsynch)
			{
				if (BatchOrDistance)
					AsynchUpdateRoutine = StartCoroutine(UpdateAsynch());
				else
					AsynchUpdateRoutine = StartCoroutine(UpdateAsynchDistance());
				enabled = false;
			}
		}
		void OnDestroy()
		{
			if (AsynchUpdateRoutine != null)
				StopCoroutine(AsynchUpdateRoutine);
		}

		internal void RegisterEntity(IMcLODEntity entity, int groupID)
		{
			if (!Entities[lastBatchID].Contains(entity))
			{
				Entities[lastBatchID].Add(entity);
				TotalEntities++;
			}
			//entity.UpdateLOD(float.MaxValue);
			if (BatchOrDistance)
				NextBatch();
		}
		internal void DeregisterEntity(IMcLODEntity entity, int groupID)
		{
			if (Entities[lastBatchID].Contains(entity))
			{
				Entities[lastBatchID].Remove(entity);
				TotalEntities--;
			}
		}

		internal static void ShowHideEntity(IMcLODEntity myLOD, bool vissible)
		{
			myLOD.IsVissible = vissible;
			myLOD.SetLOD(vissible ? 0 : 99);
		}

		void NextBatch()
		{
			lastBatchID = (lastBatchID + 1) % MaxBatches;
		}

		void LateUpdate()
		{
			if (LogTime)
			{
				watch.Reset();
				watch.Start();
			}
			var camPos = Camera.position;
			//List<IMcLODEntity> group;
			//foreach (var item in EntityGroups)
			//{
			//	group = item.Value;
			float dist;
			var list = Entities[lastBatchID];
			NextBatch();

			for (int i = 0; i < list.Count; i++)
			{
				var entity = list[i];
				if ((UseCulling && !entity.IsVissibleForCamera) || !entity.IsVissible) 
					continue;
				dist = (entity.cachedTransform.position - camPos).magnitude;
				entity.UpdateLOD(dist);
			}
			if (LogTime)
			{
				watch.Stop();
				var time = watch.Elapsed.TotalMilliseconds;
				Debug.Log("LOD Time: " + time.ToString("0.000") + "ms");
			}
		}

		IEnumerator UpdateAsynch()
		{
			int counter = 0;
			while (true)
			{
				var camPos = Camera.position;
				//List<IMcLODEntity> group;
				//foreach (var item in EntityGroups)
				//{
				//	group = item.Value;
				float dist;
				var list = Entities[lastBatchID];
				NextBatch();

				if (list.Count <= 0)
					yield return null;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					var entity = list[i];
					if (entity == null || entity.cachedTransform == null)
					{
						list.RemoveAt(i);
						TotalEntities--;
						continue;
					}

					if ((UseCulling && !entity.IsVissibleForCamera) || !entity.IsVissible) 
						continue;
					dist = (entity.cachedTransform.position - camPos).magnitude;
					entity.UpdateLOD(dist);

					if (counter++ >= MaxAsynchBatchCount)
					{
						counter = 0;
						yield return null;
					}
				}
				yield return null;
			}
		}

		IEnumerator UpdateAsynchDistance()
		{
			int counter = 0;
			while (true)
			{
				var camPos = Camera.position;
				float dist;
				int lodLevel = 0;

				IMcLODEntity entity;
				EntitiesByDistance.Clear();

				//Sort by distance and update base Lod
				for (int i = 0; i < Entities[0].Count; i++)
				{
					entity = Entities[0][i];
					dist = (entity.cachedTransform.position - camPos).magnitude;
					EntitiesByDistance.Add(dist * entity.OneByPriority, entity);

					if (counter++ >= MaxAsynchBatchCount)
					{
						counter = 0;
						yield return null;
					}
				}

				int lodCounter = 0;
				//Update distance logic
				foreach (var entry in EntitiesByDistance)
				{
					if (!entry.Value.IsVissible) continue;
					lodCounter++;
					if (lodCounter > 10) lodLevel++;
					entry.Value.SetLOD(lodLevel);

					if (counter++ >= MaxAsynchBatchCount)
					{
						counter = 0;
						yield return null;
					}
				}

				yield return null;
			}
		}
	}
}