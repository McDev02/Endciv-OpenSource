using UnityEngine;
namespace Endciv
{
	public class PathfindingDebug : MonoBehaviour
	{
		[SerializeField] GridMap gridMap;
		[SerializeField] Transform GoalObject;
		Vector3 lastPos;
		[SerializeField] Transform AgentObject;
		[SerializeField] LineRenderer pathRenderer;

		[SerializeField] float AgentSpeed = 2f;
		[SerializeField] int minOpenArea = 0;

		[SerializeField] bool Recalculate;
		PathfinderJob Job;
		Vector2i Goal;
#if USE_FLOWFIELDS
		Velocity2[,] FlowMap;
#endif
		[SerializeField] bool HasJob;

		public bool CalculateFullMap = true;

		public int LastJobTime;
#if USE_FLOWFIELDS
		[SerializeField] bool HasFlowfield;
#endif
		[SerializeField] bool MoveAgent;

		private void Update()
		{
			if (HasJob)
			{
				if (Job.IsReady)
				{
					LastJobTime = Job.ElapsedMilliseconds;
					HasJob = false;
					RedrawPath();
#if USE_FLOWFIELDS
					HasFlowfield = true;
					FlowMap = Job.flowMap;
#endif
				}
				Recalculate = true;
			}
			else
			{
				if (Recalculate && (GoalObject.position - lastPos).magnitude > 0.01f)
				{
					lastPos = GoalObject.position;
					Recalculate = false;
					Vector2i node = gridMap.View.SampleTileWorld(GoalObject.transform.position.To2D());
					Vector2i origin = gridMap.View.SampleTileWorld(AgentObject.transform.position.To2D());

					HasJob = PathfindingManager.Instance.GetPathfindingJob(node, origin, CalculateFullMap ? EPathfindingMode.SearchAll : EPathfindingMode.AllOrigins, ref Job, minOpenArea);
				}
			}
#if USE_FLOWFIELDS
			if (HasFlowfield && MoveAgent)
			{
				Vector2 agentpos = Grid.View.WorldToLocal(AgentObject.transform.position.To2D());
				Vector2i closest = Grid.View.SampleTileLocal(agentpos);

				var vel = FlowMap[closest.X, closest.Y];

				AgentObject.Translate((Vector3)vel * (AgentSpeed * Time.deltaTime));
			}
#endif
		}

		void RedrawPath()
		{
			Vector3[] positions = new Vector3[Job.path.Count];
			Vector3 pos;
			for (int i = 0; i < positions.Length; i++)
			{
				pos = gridMap.View.GetTileWorldPosition(Job.path[i]).To3D(0.05f);
				positions[i] = pos;
			}

			pathRenderer.positionCount = positions.Length;
			pathRenderer.SetPositions(positions);
			pathRenderer.enabled = true;
		}
	}
}