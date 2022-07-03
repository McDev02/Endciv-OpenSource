using System;

namespace Endciv
{
    [Serializable]
    [RequireFeature(typeof(EntityFeatureStaticData))]
    public class GridAgentStaticData : FeatureStaticData<GridAgentFeature>
    {
        public float Speed = 1;
        public float WalkingAnimationSpeed = 1;
        public float SteeringSpeed = 2;
        public MinMax Steering;
        public float PathfindingCenterOffset = 0f;
        public float destinationReachedBias = 0.2f;
        public float openAreaMinimum = 0;
    }
}
