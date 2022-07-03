using System;

namespace Endciv
{
    [Serializable]
    public class HeaterStaticData : FeatureStaticData<HeaterFeature>
    {
		public int range;
		public float heat;
	}
}
