using System.Collections.Generic;
namespace Endciv
{
	public enum EWorkerType { Worker, Transporter }
	public interface IAIJob
	{
		/// <summary>
		/// Defines if units can permanently work here
		/// </summary>
		bool IsWorkplace { get; }
		/// <summary>
		/// Occupations this workplace is involved with
		/// </summary>
		EOccupation[] Occupations { get; }
		/// <summary>
		/// How many people can work at this spot concurrently and how many jobs it provides
		/// </summary>
		int MaxWorkers { get; }
		/// <summary>
		/// How many people have a permanent job
		/// </summary>
		int WorkerCount { get; }
		/// <summary>
		/// If there is currently work for units
		/// </summary>
		bool HasWork { get; }
		/// <summary>
		/// Is the system disabled
		/// </summary>
		bool Disabled { get; set; }

		float Priority { get; }


		void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker);
		void DeregisterWorker(CitizenAIAgentFeature unit);

		//CitizenTask GetTask(EOccupation occupation, AIAgentFeature unit);

		/// <summary>
		///  Workers who work at this place in general
		/// </summary>
		List<CitizenAIAgentFeature> Workers { get; }  //Also use array?

		/// <summary>
		///  Workers who perform transportation work
		/// </summary>
		//   List<CitizenAIAgentFeature> Transporters { get; }

		/// <summary>
		/// Method used as argument on Task callback
		/// </summary>
		void OnTaskComplete(AIAgentFeatureBase unit);

        /// <summary>
        /// Callback on Task initialization
        /// </summary>
        void OnTaskStart();

		AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit);
	}
}