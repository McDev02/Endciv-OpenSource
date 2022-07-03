using System;
namespace Endciv
{
	public abstract class UserTool
	{
		internal abstract void DoBeforeEntering();
		internal abstract void DoBeforeLeaving();

		internal abstract void Process();
		internal abstract void Stop();
	}
}