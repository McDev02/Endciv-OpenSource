using UnityEngine;
using System;

namespace Endciv
{
	[CreateAssetMenu(fileName = "WorldData", menuName = "Settings/WorldData", order = 1)]
	public class WorldData : ScriptableObject //Load/save?
	{
		public int morningRelativeLength;
		public DaytimePreset morningWeather;
		public int noonRelativeLength;
		public DaytimePreset noonWeather;
		public int eveningRelativeLength;
		public DaytimePreset eveningWeather;
		public int nightRelativeLength;
		public DaytimePreset nightWeather;
		[NonSerialized] public int dayTickLength;
		public DaytimePreset rainyWeather;

		public int springRelativeDays;
		public SeasonPreset springData;
		public int summerRelativeDays;
		public SeasonPreset summerData;
		public int fallRelativeDays;
		public SeasonPreset fallData;
		public int winterRelativeDays;
		public SeasonPreset winterData;
		[NonSerialized] public int yearDayLength;

		public WorldData Clone()
		{
			var p = ScriptableObject.CreateInstance<WorldData>();
			p.morningRelativeLength = morningRelativeLength;
			p.morningWeather = morningWeather;
			p.noonRelativeLength = noonRelativeLength;
			p.noonWeather = noonWeather;
			p.eveningRelativeLength = eveningRelativeLength;
			p.eveningWeather = eveningWeather;
			p.nightRelativeLength = nightRelativeLength;
			p.nightWeather = nightWeather;

			p.rainyWeather = rainyWeather;

			p.springData = springData;
			p.springRelativeDays = springRelativeDays;
			p.summerData = summerData;
			p.summerRelativeDays = summerRelativeDays;
			p.fallData = fallData;
			p.fallRelativeDays = fallRelativeDays;
			p.winterData = winterData;
			p.winterRelativeDays = winterRelativeDays;

			return p;
		}
	}
}