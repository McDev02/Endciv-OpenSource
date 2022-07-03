using System;
using System.Collections.Generic;

namespace Endciv
{
	[Serializable]
	public class ExpeditionFeatureSaveData : ISaveable
	{
		public Dictionary<string, int> assignees = new Dictionary<string, int>();
		public int tickWhenExpeditionStarted;
		public int state;
		public int timer;
		public LocationSaveData gatherLocation;
		public LocationSaveData expeditionLocation;
		public bool isVisible;
		public string tooltip;

		public ISaveable CollectData()
		{
			return this;
		}
	}
}
