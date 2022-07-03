namespace Endciv
{
	public sealed class Trigger : BaseCommand
	{
		private bool m_InitialValue;
		private System.Action m_OnCallback;
		private System.Action m_OffCallback;

		public override bool ExecuteEmptyArguments
		{
			get { return true; }
		}

		public Trigger ( bool initialState, string name, string description, System.Action onCallback, System.Action offCallback )
			: base ( name, description, new DefaultArgument<bool> ( "value", description ) )
		{
			m_InitialValue = initialState;
			m_OnCallback = onCallback;
			m_OffCallback = offCallback;
		}

		public override void Execute ( object[] args )
		{
			if ( args == null )
			{

			}
			else
			{
				m_InitialValue = (bool)args[0];
				if ( m_InitialValue )
				{
					if ( m_OnCallback != null )
						m_OnCallback ();
				}
				else
				{
					if ( m_OffCallback != null )
						m_OffCallback ();
				}

			}
		}
	}
}
