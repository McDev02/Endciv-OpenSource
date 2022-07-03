namespace McLOD
{
	public interface IMcLODEntity
	{
		bool IsVissible { get; set; }
		bool IsVissibleForCamera { get; }
		UnityEngine.Transform cachedTransform { get; }
		float Priority { get; }
		float OneByPriority { get; }

		void UpdateLOD(float dist);
		void SetLOD(int id);
	}
}