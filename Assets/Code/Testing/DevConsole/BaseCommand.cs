using System;
using System.Collections.Generic;

namespace Endciv
{
	public abstract class BaseCommand : ICommand
	{
		private IArgument[] m_Arguments;

        private bool m_ExecuteEmptyArguments;

		public string Name { get; private set; }

		public string Description { get; protected set; }

		public IList<IArgument> Arguments { get { return m_Arguments; } }

		public virtual bool ExecuteEmptyArguments { get { return m_ExecuteEmptyArguments; } }

		public BaseCommand ( string name, string description, params IArgument[] arguments )
		{
			if ( string.IsNullOrEmpty ( name ) )
				throw new ArgumentNullException ( "name" );

			Name = name;
			Description = description;
			m_Arguments = arguments;
            if (m_Arguments == null)
                m_ExecuteEmptyArguments = true;
		}

		public abstract void Execute ( object[] args );
	}
}