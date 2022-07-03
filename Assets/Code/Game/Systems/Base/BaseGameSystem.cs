namespace Endciv
{
	public abstract class BaseGameSystem : IRunLogic
	{
		protected Stopwatch watch;
		//protected SystemsManager SystemsManager;

		public BaseGameSystem()
		{
			SystemName = this.GetType().ToString();
			watch = new Stopwatch();
		}

		public string SystemName { get; protected set; }
		public bool IsRunning { get; protected set; }

		public abstract void UpdateGameLoop();
		public abstract void UpdateStatistics();
	}
}