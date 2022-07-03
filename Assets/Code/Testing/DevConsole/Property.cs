using System;

namespace Endciv
{
	public sealed class Property<T> : BaseCommand
		where T : IConvertible
	{
		private Func<T> m_GetCallback;
		private Action<T> m_SetCallback;

		public override bool ExecuteEmptyArguments
		{
			get { return true; }
		}

		public Property ( string name, string description, Func<T> getCallback, Action<T> setCallback )
			: base ( name, description, new DefaultArgument<T> ( "value", description ) )
		{
			m_GetCallback = getCallback;
			m_SetCallback = setCallback;
		}

		public override void Execute ( object[] args )
		{
			if ( args == null )
			{
				if ( m_GetCallback != null )
				{
					T value = m_GetCallback ();
				}
				else
				{

				}
			}
			else
			{
				if ( m_SetCallback != null )
				{
					m_SetCallback ( (T)args[0] );
				}
				else
				{

				}
			}
		}
	}
}
