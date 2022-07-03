using System;
using System.Collections.Generic;

namespace Endciv
{
	public sealed class DefaultArgument<T> : IArgument
		where T : IConvertible
	{
		public string Name { get; private set; }

		public string Description { get; private set; }

		public Type ValueType { get; private set; }

		public DefaultArgument ( string name )
			: this ( name, string.Empty )
		{
		}

		public DefaultArgument ( string name, string description )
		{
			if ( string.IsNullOrEmpty ( name ) )
				throw new ArgumentNullException ( "name" );

			Name = name;
			Description = description;
			ValueType = typeof ( T );
		}

		public IEnumerable<string> ValueHints ( string value )
		{
			return null;
		}

		public bool TryParse ( string text, out object result )
		{
			try
			{
				result = Convert.ChangeType ( text, typeof ( T ), System.Globalization.CultureInfo.InvariantCulture );
				return true;
			}
			catch
			{
				result = default ( T );
				return false;
			}
		}
	}
}