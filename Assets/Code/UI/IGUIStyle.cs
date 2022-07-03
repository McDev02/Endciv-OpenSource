using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Endciv
{
	internal interface IGUIStyle
	{
		 List<GUIStyle> Styles { get; }
		void UpdateStyle();
	}
}
