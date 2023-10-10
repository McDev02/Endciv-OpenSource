

namespace Endciv
{
	public class AiAgentSettings : ScriptableObject
	{
		public MinMax HealthSatisfaction;
		public MinMax HungerSatisfaction;
		public MinMax ThirstSatisfaction;
		public MinMax RestingSatisfaction;
		public MinMax StressSatisfaction;
		public MinMax CleaningSatisfaction;
	}
}