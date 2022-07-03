using System;

namespace Endciv
{
    [Serializable]
    public abstract class FeatureStaticData<T> : FeatureStaticDataBase
		where T : FeatureBase, new()
    {
		public sealed override FeatureBase GetRuntimeFeature()
		{
			return new T();
		}
    }
}
