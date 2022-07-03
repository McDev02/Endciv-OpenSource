using System;

namespace Endciv
{   
    public sealed class DefaultCommand : BaseCommand
	{
		private System.Action m_SimpleCallback;
		private Action<object[]> m_Callback;

		public DefaultCommand ( string name, System.Action callback )
			: this ( name, string.Empty, callback ) { }

		public DefaultCommand ( string name, string description, System.Action callback )
			: base ( name, description, null )
		{	//Still Buggy
			if ( callback == null )
				throw new ArgumentNullException ( "callback" );
			m_SimpleCallback = callback;
		}

		public DefaultCommand ( string name, Action<object[]> callback )
			: this ( name, string.Empty, callback, null ) { }

		public DefaultCommand ( string name, Action<object[]> callback, params IArgument[] arguments )
			: this ( name, string.Empty, callback, arguments ) { }

		public DefaultCommand ( string name, string description, Action<object[]> callback )
			: this ( name, description, callback, null ) { }

		public DefaultCommand ( string name, string description, Action<object[]> callback, params IArgument[] arguments )
			: base ( name, description, arguments )
		{
			if ( callback == null )
				throw new ArgumentNullException ( "callback" );

			m_Callback = callback;
		}

		public override void Execute ( object[] args )
		{
			if ( m_SimpleCallback != null )
			{
				m_SimpleCallback ();
			}
			if ( m_Callback != null )
			{
				m_Callback ( args );
			}
		}
	}
}