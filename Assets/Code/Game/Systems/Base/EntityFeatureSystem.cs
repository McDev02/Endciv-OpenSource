using System;
using System.Collections.Generic;
namespace Endciv
{
	public abstract class EntityFeatureSystem<T> : BaseGameSystem where T : FeatureBase
	{
		public List<T> FeaturesCombined;
		public List<List<T>> FeaturesByFaction;
		public int FeatureCount { get; private set; }

		public Action OnFeatureAdded;
		public Action OnFeatureRemoved;

		public EntityFeatureSystem(int factions) : base()
		{
			//Player faction
			FeaturesByFaction = new List<List<T>>(factions);
			FeaturesByFaction.Add(new List<T>(32));
			FeaturesCombined = new List<T>(16);

			for (int i = 1; i < factions; i++)
			{
				FeaturesByFaction.Add(new List<T>(8));
			}
			FeatureCount = 0;
		}

		internal virtual void RegisterFeature(T feature)
		{
			if (FeaturesCombined.Contains(feature))
				UnityEngine.Debug.LogError(typeof(T).ToString() + " already registered.");
			else
			{				
				FeaturesCombined.Add(feature);
				FeaturesByFaction[feature.FactionID].Add(feature);
			}
			FeatureCount = FeaturesByFaction.Count;
		}
		internal virtual void DeregisterFeature(T feature, int faction = -1)
		{
			if (faction < 0) faction = feature.FactionID;

			FeaturesByFaction[faction].Remove(feature);
			FeaturesCombined.Remove(feature);

			FeatureCount = FeaturesByFaction.Count;
		}

		internal virtual void UpdateFaction(T feature)
		{
			for (int i = 0; i < FeaturesByFaction.Count; i++)
			{
				if (i == feature.FactionID)
				{
					if (!FeaturesByFaction[i].Contains(feature)) FeaturesByFaction[i].Add(feature);
				}
				else if (FeaturesByFaction[i].Contains(feature)) FeaturesByFaction[i].Remove(feature);
			}
		}
	}
}