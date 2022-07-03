using UnityEngine;
using System.Diagnostics;
using System;

namespace Endciv
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class TooltipAttribute : PropertyAttribute
    {
        public string Tooltip { get; set; }

        public TooltipAttribute()
        {
        }

        public TooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }

    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class LocaIdAttribute : TooltipAttribute
    {
    }

	public class StaticDataIDAttribute : PropertyAttribute
	{
		public string path;
		public Type[] requiredTypes;

		public StaticDataIDAttribute()
		{
			path = string.Empty;
		}

		public StaticDataIDAttribute(string path, params Type[] requiredTypes)
		{
			this.path = path;
			this.requiredTypes = requiredTypes;
		}
	}
}
