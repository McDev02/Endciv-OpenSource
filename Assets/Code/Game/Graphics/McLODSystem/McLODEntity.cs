using UnityEngine;
using System;

namespace McLOD
{
	public abstract class McLODEntity<T> : MonoBehaviour, IMcLODEntity where T : McLODSetting
	{
		protected int lastLOD_ID;
		protected int LOD_ID;
		public int LODGroup;
		public float[] Distances;
		public float CurrentDistance;

		public T[] States;
		protected int lastStateID;

		public bool IsVissible{ get;  set; }
		public bool IsVissibleForCamera { get; private set; }
		public Transform cachedTransform { get; private set; }

		[SerializeField] float priority = 1;
		public float Priority { get; private set; }
		public float OneByPriority { get; private set; }

		void Start()
		{
			IsVissible = true;

			priority = Mathf.Max(0.1f, priority);
			Priority = priority;
			OneByPriority = 1f / Priority;

			cachedTransform = transform;
			McLOD.Instance.RegisterEntity(this, LODGroup);
			lastLOD_ID = -1;
			lastStateID = States.Length - 1;
		}

		void OnDestroy()
		{
			if (McLOD.Instance == null)
				return;
			McLOD.Instance.DeregisterEntity(this, LODGroup);
		}

		protected virtual void OnBecameVisible()
		{
			IsVissibleForCamera = true;
		}
		protected virtual void OnBecameInvisible()
		{
			IsVissibleForCamera = false;
		}

		protected abstract void ApplyLODState(T state);

		void IMcLODEntity.UpdateLOD(float distance)
		{
			CurrentDistance = distance;
			for (LOD_ID = 0; LOD_ID < Distances.Length - 1; LOD_ID++)
			{
				if (distance <= Distances[LOD_ID]) break;
			}

			if (lastLOD_ID != LOD_ID)
				ApplyLODState(States[LOD_ID]);
			lastLOD_ID = LOD_ID;
		}

		public void SetLOD(int id)
		{
			id = Mathf.Clamp(id, 0, States.Length - 1);
			ApplyLODState(States[id]);
			lastLOD_ID = id;
		}
	}
}