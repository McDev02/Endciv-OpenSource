using UnityEngine;
using System;
using System.Collections.Generic;

namespace Endciv
{
	public class CitizenClass : AiAgentSettings
	{
		public MinMax FoodVariationSatisfaction;
		public MinMax FoodQualitySatisfaction;
		public MinMax ToiletSatisfaction;
		public MinMax SettlementSatisfaction;
		public MinMax HomeSatisfaction;

		public float satisfactionAdaption = 0.1f;
	}
}