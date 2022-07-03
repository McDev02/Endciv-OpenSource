using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(StructureFeatureStaticData), typeof(EntityFeatureStaticData))]
    public class GraveyardStaticData : FeatureStaticData<GraveyardFeature>
    {
    }
}
