using System;

namespace Endciv
{
    public class RequireFeatureAttribute : Attribute
    {
        public Type[] requiredTypes;

        public RequireFeatureAttribute(params Type[] types)
        {
            requiredTypes = types;
        }        
    }
}

