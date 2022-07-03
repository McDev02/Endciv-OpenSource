using System;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// A Command argument interface
	/// </summary>
	public interface IArgument
	{
		/// <summary>
		/// Argument name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Argument description
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Argument value type
		/// </summary>
		Type ValueType { get; }

		/// <summary>
		/// Generate a hint argument value iterator
		/// </summary>
		/// <param name="hint"> hint value </param>
		/// <returns>iterator</returns>
		IEnumerable<string> ValueHints ( string hint );

		/// <summary>
		/// Try parse a value to the target type
		/// </summary>
		/// <param name="value"> raw value </param>
		/// <param name="result"> parsed value </param>
		/// <returns> returns true if success </returns>
		bool TryParse ( string value, out object result );
	}
}