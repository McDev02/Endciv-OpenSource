using System.Text;
using UnityEngine;

namespace Endciv
{
	static partial class Util
	{
		public static StringBuilder AppendBold ( this StringBuilder sb, string text )
		{
			return sb.Append ( "<b>" ).Append ( text ).Append ( "</b>" );
		}

		public static StringBuilder AppendBeginBold ( this StringBuilder sb )
		{
			return sb.Append ( "<b>" );
		}

		public static StringBuilder AppendEndBold ( this StringBuilder sb )
		{
			return sb.Append ( "</b>" );
		}

		public static StringBuilder AppendItalic ( this StringBuilder sb, string text )
		{
			return sb.Append ( "<i>" ).Append ( text ).Append ( "</i>" );
		}

		public static StringBuilder AppendBeginItalic ( this StringBuilder sb )
		{
			return sb.Append ( "<i>" );
		}

		public static StringBuilder AppendEndItalic ( this StringBuilder sb )
		{
			return sb.Append ( "</i>" );
		}

		public static StringBuilder AppendSize ( this StringBuilder sb, int fontSize, string text )
		{
			return sb.Append ( "<size=" ).Append ( fontSize ).Append ( '>' ).Append ( text ).Append ( "</i>" );
		}

		public static StringBuilder AppendBeginSize ( this StringBuilder sb, int fontSize )
		{
			return sb.Append ( "<size=" ).Append ( fontSize ).Append ( '>' );
		}

		public static StringBuilder AppendEndSize ( this StringBuilder sb )
		{
			return sb.Append ( "</i>" );
		}

		public static StringBuilder AppendColor ( this StringBuilder sb, Color32 color, string text )
		{
			return sb.Append ( "<color=#" )
				.Append ( (color.r << 24 | color.g << 16 | color.b << 8 | color.a).ToString ( "x8" ) )
				.Append ( '>' )
				.Append ( text )
				.Append ( "</color>" );
		}

		public static StringBuilder AppendBeginColor ( this StringBuilder sb, Color32 color )
		{
			return sb.Append ( "<color=#" )
				.Append ( (color.r << 24 | color.g << 16 | color.b << 8 | color.a).ToString ( "x8" ) )
				.Append ( '>' );
		}

		public static StringBuilder AppendEndColor ( this StringBuilder sb )
		{
			return sb.Append ( "</color>" );
		}

		public static StringBuilder AppendColor ( this StringBuilder sb, HTMLColor color, string text )
		{
			return sb.Append ( "<color=" )
				.Append ( color.ToString () )
				.Append ( '>' )
				.Append ( text )
				.Append ( "</color>" );
		}

		public static StringBuilder AppendBeginColor ( this StringBuilder sb, HTMLColor color )
		{
			return sb.Append ( "<color=" )
				.Append ( color.ToString () )
				.Append ( '>' );
		}

		public static uint RawValue ( this Color32 color )
		{
			return (uint)(color.r << 24 | color.g << 16 | color.b << 8 | color.a);
		}

		public enum HTMLColor : uint
		{
			aqua = 0x00ffffff,
			cyan = 0x00ffffff,
			black = 0x000000ff,
			blue = 0x0000ffff,
			brown = 0xa52a2aff,
			darkblue = 0x0000a0ff,
			green = 0x008000ff,
			grey = 0x808080ff,
			lightblue = 0xadd8e6ff,
			lime = 0x00ff00ff,
			magenta = 0xff00ffff,
			fuchsia = 0xff00ffff,
			maroon = 0x800000ff,
			navy = 0x000080ff,
			olive = 0x808000ff,
			orange = 0xffa500ff,
			purple = 0x800080ff,
			red = 0xff0000ff,
			silver = 0xc0c0c0ff,
			teal = 0x008080ff,
			white = 0xffffffff,
			yellow = 0xffff00ff,
		}
	}
}