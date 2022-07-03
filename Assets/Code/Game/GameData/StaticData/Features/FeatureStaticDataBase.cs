using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public abstract class FeatureStaticDataBase : ScriptableObject
	{
		[HideInInspector]
		public EntityStaticData entity;

		public bool autoRun = true;

		public abstract FeatureBase GetRuntimeFeature();

		public virtual void Init() { }

		public virtual void OnFeatureStaticDataChanged()
		{

		}
	}
}
