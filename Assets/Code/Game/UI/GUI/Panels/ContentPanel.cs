namespace Endciv
{
	public abstract class ContentPanel : GUIAnimatedPanel, IRunLogic
	{
		public bool IsRunning { get; private set; }
		public abstract void UpdateData();

		public virtual void Run()
		{
			IsRunning = true;
		}
		public virtual void Stop()
		{
			IsRunning = false;
		}
	}
}