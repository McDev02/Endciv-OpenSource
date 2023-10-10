
using System;
using System.Collections.Generic;

namespace Endciv
{
	public class CitizenShedule : ScriptableObject
	{
		public enum ESheduleType
		{
			Sleep,		//Sleeping
			Work,		//Working
			Lunch,		//Eating
			SpareTime,	//Doing anything but work, may also go home
			Hometime	//Stay at home if available, but no sleep Is like spare time
		}

		public List<SheduleState> states;

		[Serializable]
		public struct SheduleState
		{
			public ESheduleType type;
			public float beginTime;
			public int id;

			public SheduleState(float begin)
			{
				type = ESheduleType.Work;
				beginTime = begin;
				this.id = -1;
			}
		}

		public SheduleState GetCurrentShedule(float time)
		{
			for (int i = states.Count - 1; i >= 0; i--)
			{
				if (states[i].beginTime <= time)
					return states[i];
			}
			return states[0];
		}
	}
}