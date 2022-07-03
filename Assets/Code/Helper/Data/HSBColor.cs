using UnityEngine;

namespace Endciv
{
	[System.Serializable]
	public struct HSBColor : System.IEquatable<HSBColor>
	{
		public float H;
		public float S;
		public float B;
		public float A;

		public HSBColor(float hue, float saturation, float brightness, float alpha = 1f)
		{
			this.H = hue;
			this.S = saturation;
			this.B = brightness;
			this.A = alpha;
		}

		public HSBColor(Color col)
		{
			this = FromColor(col);
		}

		public Color ToColor()
		{
			return ToColor(this);
		}

		public bool Equals(HSBColor other)
		{
			return H == other.H
				   && S == other.S
				   && B == other.B
				   && A == other.A;
		}

		public override string ToString()
		{
			return "H:" + H + " S:" + S + " B:" + B;
		}

		public static HSBColor FromColor(Color color)
		{
			HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

			float r = color.r;
			float g = color.g;
			float b = color.b;

			float max = Mathf.Max(r, Mathf.Max(g, b));

			if (max <= 0)
			{
				return ret;
			}

			float min = Mathf.Min(r, Mathf.Min(g, b));
			float dif = max - min;

			if (max > min)
			{
				if (g == max)
				{
					ret.H = (b - r) / dif * 60f + 120f;
				}
				else if (b == max)
				{
					ret.H = (r - g) / dif * 60f + 240f;
				}
				else if (b > g)
				{
					ret.H = (g - b) / dif * 60f + 360f;
				}
				else
				{
					ret.H = (g - b) / dif * 60f;
				}
				if (ret.H < 0)
				{
					ret.H = ret.H + 360f;
				}
			}
			else
			{
				ret.H = 0;
			}

			ret.H *= 1f / 360f;
			ret.S = (dif / max) * 1f;
			ret.B = max;

			return ret;
		}

		public static Color ToColor(HSBColor hsbColor)
		{
			float r = hsbColor.B;
			float g = hsbColor.B;
			float b = hsbColor.B;
			if (hsbColor.S != 0)
			{
				float max = hsbColor.B;
				float dif = hsbColor.B * hsbColor.S;
				float min = hsbColor.B - dif;

				float h = hsbColor.H * 360f;

				if (h < 60f)
				{
					r = max;
					g = h * dif / 60f + min;
					b = min;
				}
				else if (h < 120f)
				{
					r = -(h - 120f) * dif / 60f + min;
					g = max;
					b = min;
				}
				else if (h < 180f)
				{
					r = min;
					g = max;
					b = (h - 120f) * dif / 60f + min;
				}
				else if (h < 240f)
				{
					r = min;
					g = -(h - 240f) * dif / 60f + min;
					b = max;
				}
				else if (h < 300f)
				{
					r = (h - 240f) * dif / 60f + min;
					g = min;
					b = max;
				}
				else if (h <= 360f)
				{
					r = max;
					g = min;
					b = -(h - 360f) * dif / 60 + min;
				}
				else
				{
					r = 0;
					g = 0;
					b = 0;
				}
			}

			return new Color(r, g, b, hsbColor.A);
		}

		public static HSBColor Lerp(Color a, Color b, float t)
		{
			return Lerp(FromColor(a), FromColor(b), t);
		}

		public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
		{
			float h, s;

			//check special case black (color.b==0): interpolate neither hue nor saturation!
			//check special case grey (color.s==0): don't interpolate hue!
			if (a.B == 0)
			{
				h = b.H;
				s = b.S;
			}
			else if (b.B == 0)
			{
				h = a.H;
				s = a.S;
			}
			else
			{
				if (a.S == 0)
				{
					h = b.H;
				}
				else if (b.S == 0)
				{
					h = a.H;
				}
				else
				{
					// works around bug with LerpAngle
					float angle = Mathf.LerpAngle(a.H * 360f, b.H * 360f, t);
					while (angle < 0f)
						angle += 360f;
					while (angle > 360f)
						angle -= 360f;
					h = angle / 360f;
				}
				s = Mathf.Lerp(a.S, b.S, t);
			}
			return new HSBColor(h, s, Mathf.Lerp(a.B, b.B, t), Mathf.Lerp(a.A, b.A, t));
		}
	}
}