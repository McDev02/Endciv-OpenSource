using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// A <see cref="EndCiv.DevConsole"/> Command interface
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Command name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Command description
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Command arguments
		/// </summary>
		IList<IArgument> Arguments { get; }

		/// <summary>
		/// Execute even args is empty
		/// </summary>
		bool ExecuteEmptyArguments { get; }

		/// <summary>
		/// Execute the command
		/// </summary>
		/// <param name="args"> command arguments </param>
		void Execute ( object[] args );
	}
}