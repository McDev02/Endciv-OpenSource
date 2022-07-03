using UnityEngine;
using System;

namespace Endciv
{
	public class GameMechanicSettings : ScriptableObject
	{
		[SerializeField] public AISettings aiSettings;
	}

	[Serializable]
	public struct AISettings
	{
		//Shedules
		public CitizenShedule childShedule;
		public CitizenShedule undefinedShedule;
		public CitizenShedule labourShedule;

		[SerializeField] public CitizenClass[] citizenClasses;
	}
}