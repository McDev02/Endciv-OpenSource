using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class EntityStaticData : BaseStaticData
	{
		/// <summary>
		/// Localized name
		/// </summary>
		public string Name { get { return LocalizationManager.GetText(LocalizationManager.StructurePath + ID + "/name"); } }		

		[HideInInspector]
		public List<FeatureStaticDataBase> FeatureStaticData;

		public T GetFeature<T>() where T : FeatureStaticDataBase
		{
			if (FeatureStaticData == null || FeatureStaticData.Count <= 0)
				return default(T);
			foreach (var feature in FeatureStaticData)
			{
				if (feature.GetType() == typeof(T))
				{
					return (T)feature;
				}
			}
			return default(T);
		}

		public int GetFeatureID<T>() where T : FeatureStaticDataBase
		{
			for (int i = 0; i < FeatureStaticData.Count; i++)
			{
				if (FeatureStaticData[i] is T)
					return i;
			}
			return -1;
		}

		public bool HasFeature(Type type)
		{
			if (FeatureStaticData == null || FeatureStaticData.Count <= 0)
				return false;
			foreach (var feature in FeatureStaticData)
			{
				if (feature.GetType() == type)
				{
					return true;
				}
			}
			return false;
		}
	}
}
