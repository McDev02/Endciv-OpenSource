using System;

namespace Endciv
{
    public class EntityCategoryAttribute : Attribute
    {
        public string categoryName;

        public EntityCategoryAttribute(string categoryName)
        {
            this.categoryName = categoryName;
        }
    }
}

