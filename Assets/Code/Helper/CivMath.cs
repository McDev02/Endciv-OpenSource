using UnityEngine;

namespace Endciv
{
	public static partial class CivMath
	{
		#region Constants

		public const float e = 2.718281828459045235f;
		public const float sqrt2 = 1.414213562373095f;

		public const float GoldenRatio = 0.6180339887f;
		public const double Tolerance = 0.00001;
		public const float sDensity = 10;
		public const float OneByPI = (float)(1.0 / 3.1415926535897932384626433832795);

		#endregion Constants

		public static readonly Perlin Perlin = new Perlin();

		#region Vector

		/// <summary>
		/// t ranges from 0 to 1
		/// </summary>
		public static Vector2 GetCircleDirection(float t)
		{
			return new Vector2(Mathf.Cos(Mathf.PI * 2 * t), Mathf.Sin(Mathf.PI * 2 * t));
		}

		/// <summary>
		/// t ranges from 0 to 1
		/// </summary>
		public static Vector2 GetCircleDirection2(float t)
		{
			return new Vector2(Mathf.Sin(Mathf.PI * 2 * t), Mathf.Cos(Mathf.PI * 2 * t));
		}

		public static Quaternion Vector3ToQuaternion(Vector3 Up)
		{
			Vector3 dir = Random.onUnitSphere;
			Vector3 up = Up.normalized;
			Vector3 right = up + dir;
			Vector3 forward = Vector3.Cross(right.normalized, up).normalized;

			Quaternion q = Quaternion.LookRotation(forward, up);

			return q;
		}

		public static Vector2i Min(Vector2i a, Vector2i b)
		{
			return new Vector2i(Min(a.X, b.X), Min(a.Y, b.Y));
		}
		public static Vector2i Max(Vector2i a, Vector2i b)
		{
			return new Vector2i(Max(a.X, b.X), Max(a.Y, b.Y));
		}

		public static Vector2 Min(Vector2 a, Vector2 b)
		{
			return new Vector2(Min(a.x, b.x), Min(a.y, b.y));
		}
		public static Vector2 Max(Vector2 a, Vector2 b)
		{
			return new Vector2(Max(a.x, b.x), Max(a.y, b.y));
		}

		public static int Min(int a, int b)
		{
			return a > b ? b : a;
		}
		public static int Max(int a, int b)
		{
			return a < b ? b : a;
		}
		public static float Min(float a, float b)
		{
			return a > b ? b : a;
		}
		public static float Max(float a, float b)
		{
			return a < b ? b : a;
		}

		/// <summary>
		/// Performs <see cref="Mathf.Floor"/> per component
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector2 FloorVector(Vector2 vec)
		{
			return new Vector2(Mathf.Floor(vec.x), Mathf.Floor(vec.y));
		}
		public static Vector2i FloorVectorInt(Vector2 vec)
		{
			return new Vector2i(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y));
		}

		/// <summary>
		/// Performs <see cref="Mathf.Floor"/> per component
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3 FloorVector(Vector3 vec)
		{
			return new Vector3(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z));
		}
		/// <summary>
		/// Performs <see cref="Mathf.Ceil"/> per component
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector2 CeilVector(Vector2 vec)
		{
			return new Vector2(Mathf.Ceil(vec.x), Mathf.Ceil(vec.y));
		}
		public static Vector2i CeilVectorInt(Vector2 vec)
		{
			return new Vector2i(Mathf.CeilToInt(vec.x), Mathf.CeilToInt(vec.y));
		}

		/// <summary>
		/// Performs <see cref="Mathf.Ceil"/> per component
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3 CeilVector(Vector3 vec)
		{
			return new Vector3(Mathf.Ceil(vec.x), Mathf.Ceil(vec.y), Mathf.Ceil(vec.z));
		}

		/// <summary>
		/// Scales a vector to a length between 0 and 1
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static float Clamp01(float v)
		{
			return v < 0 ? 0 : (v > 1 ? 1 : v);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and X
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static float Clamp0X(float v, float max)
		{
			return v < 0 ? 0 : (v > max ? max : v);
		}
		/// <summary>
		/// Scales a int to a length between 0 and X
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static int Clamp0X(int v, int max)
		{
			return v < 0 ? 0 : (v > max ? max : v);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and 1
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector2 ClampVector(Vector2 vec)
		{
			float vr = vec.magnitude;
			Vector2 v = vec.normalized;
			return v * Mathf.Clamp01(vr);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and value
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector2 ClampVector(Vector2 vec, float r)
		{
			float vr = vec.magnitude;
			Vector2 v = vec.normalized;
			return v * Mathf.Clamp(vr, 0, r);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and 1
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3 ClampVector(Vector3 vec)
		{
			float vr = vec.magnitude;
			Vector3 v = vec.normalized;
			return v * Mathf.Clamp01(vr);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and value
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3 ClampVector(Vector3 vec, float r)
		{
			float vr = vec.magnitude;
			Vector3 v = vec.normalized;
			return v * Mathf.Clamp(vr, 0, r);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and 1
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector4 ClampVector(Vector4 vec)
		{
			float vr = vec.magnitude;
			Vector4 v = vec.normalized;
			return v * Mathf.Clamp01(vr);
		}

		/// <summary>
		/// Scales a vector to a length between 0 and value
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector4 ClampVector(Vector4 vec, float r)
		{
			float vr = vec.magnitude;
			Vector4 v = vec.normalized;
			return v * Mathf.Clamp(vr, 0, r);
		}

		/// <summary>
		/// Returns the per component value modulo(%) by value. E.g. ModVector({1.6,0.8}, 1) -> (0.6,0.8)
		/// </summary>
		/// <param name="vec"></param>
		/// <param name="modulo"></param>
		/// <returns></returns>
		public static Vector2 ModVector(Vector2 vec, float modulo)
		{
			return new Vector2(vec.x % modulo, vec.y % modulo);
		}

		/// <summary>
		/// Returns the per component value modulo(%) by value. E.g. ModVector({1.6,0.8}, 1) -> (0.6,0.8)
		/// </summary>
		/// <param name="vec"></param>
		/// <param name="modulo"></param>
		/// <returns></returns>
		public static Vector3 ModVector(Vector3 vec, float modulo)
		{
			return new Vector3(vec.x % modulo, vec.y % modulo, vec.z % modulo);
		}

		/// <summary>
		/// Returns the per component value modulo(%) by value. E.g. ModVector({1.6,0.8}, 1) -> (0.6,0.8)
		/// </summary>
		/// <param name="vec"></param>
		/// <param name="modulo"></param>
		/// <returns></returns>
		public static Vector4 ModVector(Vector4 vec, float modulo)
		{
			return new Vector4(vec.x % modulo, vec.y % modulo, vec.z % modulo, vec.w % modulo);
		}

		/// <summary>
		/// Performs a per component multiplication
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector2 MulVector(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x * b.x, a.y * b.y);
		}

		/// <summary>
		/// Performs a per component multiplication
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector3 MulVector(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		/// <summary>
		/// Performs a per component multiplication
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector4 MulVector(Vector4 a, Vector4 b)
		{
			return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		}

		/// <summary>
		/// Performs a per component value powered by two.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector2 MulVector(Vector2 a)
		{
			return new Vector2(a.x * a.x, a.y * a.y);
		}

		/// <summary>
		/// Performs a per component value powered by two.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector3 MulVector(Vector3 a)
		{
			return new Vector3(a.x * a.x, a.y * a.y, a.z * a.z);
		}

		/// <summary>
		/// Performs a per component value powered by two.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vector4 MulVector(Vector4 a)
		{
			return new Vector4(a.x * a.x, a.y * a.y, a.z * a.z, a.w * a.w);
		}

		#endregion Vector		

		#region Rect

		/// <summary>
		/// Clamps the Rect on X and Y to a Quader from 0 to size
		/// </summary>
		/// <param name="size"></param>
		public static void RectClamp(ref Rect rect, float size)
		{
			rect.xMin = Mathf.Clamp(rect.xMin, 0, size);
			rect.yMin = Mathf.Clamp(rect.yMin, 0, size);
			rect.xMax = Mathf.Clamp(rect.xMax, 0, size);
			rect.yMax = Mathf.Clamp(rect.yMax, 0, size);
		}

		public static void RectLimit(ref Rect rect, float width, float height)
		{
			if (rect.xMin <= 0) rect.xMin = 0;
			else if (rect.xMax > width) rect.x -= (rect.xMax - width);
			if (rect.yMin <= 0) rect.yMin = 0;
			else if (rect.yMax > height) rect.y -= (rect.yMax - height);
		}

		public static Vector2 RectLimit(Rect rect, float width, float height)
		{
			Vector2 rectOffset = new Vector2();
			if (rect.xMin <= 0) rectOffset.x = rect.xMin;
			else if (rect.xMax > width) rectOffset.x = (rect.xMax - width);
			if (rect.yMin <= 0) rectOffset.y = rect.yMin;
			else if (rect.yMax > height) rectOffset.y = (rect.yMax - height);
			return -rectOffset;
		}

		public static void RectExtend(ref Rect rect, int size)
		{
			rect = new Rect(rect.xMin - size, rect.yMin - size, rect.width + size * 2, rect.height + size * 2);
		}

		public static void RectExtend(ref Rect rect, float size)
		{
			rect = new Rect(rect.x - size, rect.y - size, rect.width + size * 2, rect.height + size * 2);
		}

		public static void RectExtend(ref Rect rect, float top, float right, float bottom, float left)
		{
			top += rect.yMax;
			right += rect.xMax;
			bottom += rect.yMin;
			left += rect.xMin;
			rect = new Rect(left, top, right - left, top - bottom);
		}

		public static void RectTransform(ref Rect rect, float factor)
		{
			rect = new Rect(rect.xMin * factor, rect.yMin * factor, rect.width * factor, rect.height * factor);
		}

		public static Vector2 ClampPointInRect(Vector2 p, Rect rect)
		{
			var min = rect.min;
			var max = rect.max;
			p.x = Clamp(p.x, min.x, max.x);
			p.y = Clamp(p.y, min.y, max.y);
			return p;
		}

		public static float DistanceFromPointToRect(Vector2 p, Rect rect)
		{
			Vector2 d = ClampPointInRect(p, rect);
			return (d - p).magnitude;
		}

		public static Vector2i ClampPointInRect(Vector2i p, RectBounds rect)
		{
			var min = rect.Minimum;
			var max = rect.Maximum;
			p.X = Clamp(p.X, min.X, max.X);
			p.Y = Clamp(p.Y, min.Y, max.Y);
			return p;
		}

		public static float DistanceFromPointToRect(Vector2i p, RectBounds rect)
		{
			Vector2i d = ClampPointInRect(p, rect);
			return (d - p).Magnitude;
		}

		#endregion

		#region Geometry

		//public static Vector2 LinearRegression(Vector2[] Points)
		//{
		//	float avrX = 0, avrY = 0;
		//	foreach (Vector2 CV in Points)
		//	{
		//		avrX += CV.x;
		//		avrY += CV.y;
		//	}
		//	avrX /= Points.Length; avrY /= Points.Length;
		//
		//	float SSxy = 0, SSxx = 0;
		//	for (int i = 0; i < Points.Length; i++)
		//	{
		//		SSxy += (Points[i].x - avrX) * (Points[i].y - avrY);
		//		SSxx += Mathf.Pow((Points[i].x - avrX), 2);
		//	}
		//
		//	if (SSxx != 0)
		//		bLin = SSxy / SSxx;
		//	else
		//		Debug.LogError("Division by Zero at LinearRegression by SSxx");
		//
		//	aLin = avrY - bLin * avrX;
		//}

		/// <summary>
		/// Unclamped Lerp
		/// </summary>
		public static float Lerp(float v1, float v2, float t)
		{
			return (1 - t) * v1 + t * v2;
		}

		/// <summary>
		/// Lerp in fixed steps clapmped
		/// </summary>
		public static float LerpStep(float v1, float v2, float step)
		{
			var diff = v2 - v1;
			step = Mathf.Min(step, Mathf.Abs(diff));
			if (diff < 0)
				return v1 - step;
			else
				return v1 + step;
		}

		/// <summary>
		/// Unclamped Lerp
		/// </summary>
		public static double dLerp(double a, double b, double t)
		{
			return (1 - t) * a + t * b;
		}

		public static float AngleFull(Vector2 v)
		{
			return 360 * AngleFullFactor(v);
		}

		public static float AngleFullFactor(Vector2 v)
		{
			float angle = 0.5f * OneByPI * Mathf.Acos(Vector2.Dot(Vector2.up, v.normalized));
			return v.x < 0 ? 1 - angle : angle;
		}

		public static float fSmoothSqrt(float t)
		{
			return Mathf.Pow(t, 1 - t);
		}

		/// <summary>
		/// Performs quadratic interpolation. Values are more on the extremes.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static float fQuadratic(float t)
		{
			return Mathf.Pow(t, 2) * (-2 * t + 3);
		}
		/// <summary>
		/// Performs inverse quadratic interpolation. Values are more in the middle.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static float fQuadraticInverse(float t)
		{
			return t + t - fQuadratic(t);
		}

		//public static float fQubic(float t)
		//{//Seems to be wrong!
		//	return 0;// Mathf.Pow ( t, 3 ) * (6 * Mathf.Pow ( t, 2 ) * 15 * t + 10);
		//	Debug.Log ("Do not use CivMath.fQubic()!");
		//}

		public static float LinePointDistance(Vector2 A, Vector2 aDir, Vector2 Point)
		{
			Vector2 nDir = new Vector2(aDir.y, aDir.x);
			Vector2 sD;
			LineIntersection(A, aDir, Point, nDir, out sD);
			return (sD - Point).magnitude;
		}

		public static float LineSegmentPointDistance(Vector2 A, Vector2 B, Vector2 Point)
		{
			Vector2 nDir = new Vector2(-(B - A).y, (B - A).x);
			Vector2 sD = Vector2.zero;

			if (LineSegmentIntersection(A, B, Point - nDir * 999, Point + nDir * 999, out sD))
			{
				return (sD - Point).magnitude;
			}
			else
			{
				float dist1 = (A - Point).magnitude;
				float dist2 = (B - Point).magnitude;
				if (dist1 <= dist2)
					sD = A;
				else sD = B;
				return dist1;
			}
		}

		public static float LineSegmentPointDistance(Vector2 A, Vector2 B, Vector2 Point, out Vector2 sD)
		{
			Vector2 nDir = new Vector2(-(B - A).y, (B - A).x);
			sD = Vector2.zero;

			if (LineSegmentIntersection(A, B, Point - nDir * 999, Point + nDir * 999, out sD))
			{
				return (sD - Point).magnitude;
			}
			else
			{
				float dist1 = (A - Point).magnitude;
				float dist2 = (B - Point).magnitude;
				if (dist1 <= dist2)
					sD = A;
				else sD = B;
				return dist1;
			}
		}
		/// <summary>
		/// MAy be broken! Returns the distance from a point to a rect and outputs closest point. Caution, this method is also valid for points inside the rect and would then calculate the distance to the outline.
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="Point"></param>
		/// <param name="sD">Closest point on the edge of the box</param>
		/// <returns></returns>
		public static float BoxSegmentPointDistance(Rect rect, Vector2 Point, out Vector2 sD)
		{
			Debug.LogError("This method might be broken. Validate before using!");
			float dist = 9999999;
			float minDist = dist + 1;

			float sign = 1;

			Vector2 A = new Vector2(rect.xMin, rect.yMin);
			Vector2 B = new Vector2(rect.xMax, rect.yMin);
			Vector2 C = new Vector2(rect.xMax, rect.yMax);
			Vector2 D = new Vector2(rect.xMin, rect.yMax);

			Vector2 hit;
			sD = new Vector2();

			//Line A
			dist = LineSegmentPointDistance(A, B, Point, out hit);
			if (dist < minDist)
			{
				sD = hit;
				minDist = dist;
				sign = GetSign(A, B, Point);
			}

			//Line B
			dist = LineSegmentPointDistance(B, C, Point, out hit);
			if (dist < minDist)
			{
				sD = hit;
				minDist = dist;
				sign = GetSign(B, C, Point);
			}

			//Line C
			dist = LineSegmentPointDistance(C, D, Point, out hit);
			if (dist < minDist)
			{
				sD = hit;
				minDist = dist;
				sign = GetSign(C, D, Point);
			}

			//Line D
			dist = LineSegmentPointDistance(D, A, Point, out hit);
			if (dist < minDist)
			{
				sD = hit;
				minDist = dist;
				sign = GetSign(D, A, Point);
			}

			return sign * minDist;
		}

		public static float GetSign(Vector2 A, Vector2 B, Vector2 Point)
		{
			Vector2 tan = (B - A).normalized;
			Vector2 norm = new Vector2(tan.y, -tan.x);
			return (Vector2.Dot(norm, (Point - Vector2.Lerp(A, B, 0.5f)).normalized)) < 0 ? -1 : 1;
		}

		public static bool LineIntersection(Vector2 A, Vector2 aDir, Vector2 B, Vector2 bDir, out Vector2 S)
		{
			//Todo: Hacked to work. Replace with more efficient formular
			S = Vector2.zero;
			Vector2 A1 = A - aDir;
			Vector2 B1 = B - bDir;
			Vector2 A2 = A + aDir;
			Vector2 B2 = B + bDir;

			float s1_x, s1_y, s2_x, s2_y;
			s1_x = A2.x - A1.x;
			s1_y = A2.y - A1.y;
			s2_x = B2.x - B1.x;
			s2_y = B2.y - B1.y;

			float det = (-s2_x * s1_y + s1_x * s2_y);
			//If Parallel
			if (Mathf.Abs(det) < 0.1f)
				return false;

			//float s = (-s1_y*(A1.x - B1.x) + s1_x*(A1.y - B1.y))/det;
			float t = (s2_x * (A1.y - B1.y) - s2_y * (A1.x - B1.x)) / det;

			// Collision detected

			S = new Vector2(A1.x + (t * s1_x), A1.y + (t * s1_y));
			return true;
		}

		public static bool LineSegmentIntersection(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out Vector2 S)
		{
			S = Vector2.zero;

			float s1_x, s1_y, s2_x, s2_y;
			s1_x = A2.x - A1.x;
			s1_y = A2.y - A1.y;
			s2_x = B2.x - B1.x;
			s2_y = B2.y - B1.y;

			float det = (-s2_x * s1_y + s1_x * s2_y);
			if (Mathf.Abs(det) < 0.1f)
				return false;

			float s, t;
			s = (-s1_y * (A1.x - B1.x) + s1_x * (A1.y - B1.y)) / det;
			t = (s2_x * (A1.y - B1.y) - s2_y * (A1.x - B1.x)) / det;

			if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
			{
				// Collision detected
				S = new Vector2(A1.x + (t * s1_x), A1.y + (t * s1_y));
				return true;
			}

			return false; // No collision
		}

		public static float TriangleArea(Vector2 A, Vector2 B, Vector2 C)
		{
			float a = (B - A).magnitude;
			float b = (C - B).magnitude;
			float c = (A - C).magnitude;

			float s = (a + b + c) / 2f;

			float area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
			return area;
		}

		public static float TriangleArea(Vector3 A, Vector3 B, Vector3 C)
		{
			float a = (B - A).magnitude;
			float b = (C - B).magnitude;
			float c = (A - C).magnitude;

			float s = (a + b + c) / 2f;

			return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
		}

		public static float TriangleArea(Vector4 A, Vector4 B, Vector4 C)
		{
			float a = (B - A).magnitude;
			float b = (C - B).magnitude;
			float c = (A - C).magnitude;

			float s = (a + b + c) / 2f;

			return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
		}

		public static float TriangleAreaGlobe(Vector3 A, Vector3 B, Vector3 C)
		{
			float a = CivMath.OrthodromeDistance(A, B);
			float b = CivMath.OrthodromeDistance(C, B);
			float c = CivMath.OrthodromeDistance(A, C);

			float s = (a + b + c) / 2f;

			return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
		}

		#endregion Geometry

		#region Methods

		public static float GetClosestMultipleOf(float baseValue, float multiple)
		{
			return Mathf.Round(baseValue / multiple) * multiple;
		}
		public static int GetClosestMultipleOf(float baseValue, int multiple)
		{
			return Mathf.RoundToInt(baseValue / multiple) * multiple;
		}

		public static float GetNextMultipleOf(float baseValue, float multiple)
		{
			return Mathf.Ceil(baseValue / multiple) * multiple;
		}
		public static int GetNextMultipleOf(float baseValue, int multiple)
		{
			return Mathf.CeilToInt(baseValue / multiple) * multiple;
		}

		public static int IndexOfMin(int[] self)
		{
			if (self == null)
			{
				throw new System.ArgumentNullException("self");
			}

			if (self.Length == 0)
			{
				throw new System.ArgumentException("List is empty.", "self");
			}

			int min = self[0];
			int minIndex = 0;

			for (int i = 1; i < self.Length; ++i)
			{
				if (self[i] < min)
				{
					min = self[i];
					minIndex = i;
				}
			}

			return minIndex;
		}

		public static int IndexOfMin(double[] self)
		{
			if (self == null)
			{
				throw new System.ArgumentNullException("self");
			}

			if (self.Length == 0)
			{
				throw new System.ArgumentException("List is empty.", "self");
			}

			double min = self[0];
			int minIndex = 0;

			for (int i = 1; i < self.Length; ++i)
			{
				if (self[i] < min)
				{
					min = self[i];
					minIndex = i;
				}
			}

			return minIndex;
		}

		public static float Clamp(float value, float min, float max)
		{
			return (value < min) ? min : (value > max) ? max : value;
		}
		public static double Clamp(double value, double min, double max)
		{
			return (value < min) ? min : (value > max) ? max : value;
		}
		public static int Clamp(int value, int min, int max)
		{
			return (value < min) ? min : (value > max) ? max : value;
		}

		public static float Exp(float pow)
		{
			return Mathf.Pow(e, pow);
		}

		public static double Pow(double pow)
		{
			return pow * pow;
		}

		public static Vector3 GetSphereCoord(float a, float m)
		{
			return new Vector3(Mathf.Sin(m) * Mathf.Cos(a), Mathf.Sin(m) * Mathf.Sin(a), Mathf.Cos(m));
		}

		public static int Clamp1X(int input)
		{
			return input < 1 ? 1 : input;
		}

		public static float Clamp1X(float input)
		{
			return input < 1 ? 1 : input;
		}

		/// <summary>
		/// Transformes values ranging from (-1 to 1) to (0 to 1)
		/// </summary>
		public static float NegativeToZero(this float v)
		{
			return (v + 1) * 0.5f;
		}
		/// <summary>
		/// Transformes values ranging from (0 to 1) to (-1 to 1)
		/// </summary>
		public static float ZeroToNegative(this float v)
		{
			return v * 2 - 1;
		}

		public static float Min(float[] values)
		{
			if (values == null || values.Length <= 0)
				return 0;
			float min = float.MaxValue;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] < min)
					min = values[i];
			}
			return min;
		}

		public static Vector3 GetSphereCoord2(float a, float m, float r)
		{
			a *= Mathf.PI;
			m *= Mathf.PI;
			Vector3 right = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
			Vector3 P = (Vector3.up * Mathf.Cos(m) + right * Mathf.Sin(m));

			return r * P;
		}

		public static Vector3 GetSphereCoord2(Vector2 vec, float r)
		{
			float a = vec.x;
			float m = vec.y;

			a *= Mathf.PI;
			m *= Mathf.PI;
			Vector3 right = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
			Vector3 P = (Vector3.up * Mathf.Cos(m) + right * Mathf.Sin(m));

			return r * P;
		}


		public static Vector3 GetSphereCoord(float r, float a, float m)
		{
			return r * new Vector3(Mathf.Sin(m) * Mathf.Cos(a), Mathf.Sin(m) * Mathf.Sin(a), Mathf.Cos(m));
		}

		public static Vector3 GetPolarCoord3(Vector3 pos)
		{
			float r, a, m;
			r = pos.magnitude;
			pos = pos.normalized;
			a = Mathf.Atan2(pos.y, pos.x);
			m = Mathf.Acos(pos.z);
			return new Vector3(r, a, m);
		}

		public static Vector2 GetPolarCoord2(Vector3 pos)
		{
			float a, m;
			pos = pos.normalized;
			a = Mathf.Atan2(pos.y, pos.x);
			m = Mathf.Acos(pos.z);
			return new Vector2(a, m);
		}

		public static Vector2 GetTexCoordFromSphere(Vector3 pos)
		{
			float a, m;
			pos = pos.normalized;
			a = Mathf.Atan2(-pos.z, -pos.x) / (Mathf.PI * 2);
			m = Mathf.Acos(-pos.y) / Mathf.PI;
			a = (5 + a) % 1;
			m = (5 + m) % 1;
			return new Vector2(a, m);
		}

		public static Vector3 InterpolateSpherePos(Vector2 a, Vector2 b, float t)
		{
			return GetPolarCoord2(Vector2.Lerp(a, b, t));
		}

		public static float OrthodromeAngle(Vector3 a, Vector3 b)
		{
			//return Mathf.Acos(Mathf.Sin(a.x)*Mathf.Sin(b.x)+Mathf.Cos(a.x)*Mathf.Cos(b.x)*Mathf.Cos(b.y-a.y));
			return Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
		}

		public static float OrthodromeDistance(Vector3 a, Vector3 b, float radius = 1)
		{
			return OrthodromeAngle(a, b) * radius;
		}

		public static float OrthodromeDistance(float ang, float radius = 1)
		{
			return ang * radius;
		}

		public static Vector3 OrthodromeIntersection(Vector3 org1, Vector3 dst1, Vector3 org2, Vector3 dst2,
			float radius = 1) //t=[0,1]
		{
			org1 = org1.normalized;
			dst1 = dst1.normalized;
			org2 = org2.normalized;
			dst2 = dst2.normalized;

			Vector3 P;

			Vector3 norm1 = Vector3.Cross(org1, dst1).normalized;
			Vector3 norm2 = Vector3.Cross(org2, dst2).normalized;
			Vector3 dir1 = Vector3.Cross(norm1, norm2).normalized;
			Vector3 dir2 = -dir1;

			//enought check?
			float dist1 = OrthodromeDistance(dir1 * radius, org1) + OrthodromeDistance(dir1 * radius, org2) +
						  OrthodromeDistance(dir1 * radius, dst1) + OrthodromeDistance(dir1 * radius, dst2);
			float dist2 = OrthodromeDistance(dir2 * radius, org1) + OrthodromeDistance(dir2 * radius, org2) +
						  OrthodromeDistance(dir2 * radius, dst1) + OrthodromeDistance(dir2 * radius, dst2);

			if (dist1 < dist2)
				P = dir1 * radius;
			else
				P = dir2 * radius;

			return P;
		}

		public static Vector3 GetOrthodromePointRelative(Vector3 org, Vector3 dst, float t, float radius = 1) //t=[0,1]
		{
			org = org.normalized;
			dst = dst.normalized;

			Vector3 P;
			if ((org - dst).magnitude <= Tolerance)
			{
				P = org * radius;
			}
			else
			{
				float ang = OrthodromeAngle(org, dst);

				Vector3 norm = Vector3.Cross(org, dst).normalized;
				Vector3 up = org;
				Vector3 right = Vector3.Cross(norm, up).normalized;

				P = (up * Mathf.Cos(ang * t) + right * Mathf.Sin(ang * t)) * radius;

				//Vector3 P = new Vector3(Mathf.Sin(t*ang),Mathf.Cos(t*ang),0);
			}
			return P;
		}

		public static Vector3 GetOrthodromePointRelativeWeighted(Vector4 org, Vector4 dst, float t, float radius = 1)
		//t=[0,1]
		{
			float ang = OrthodromeAngle(org, dst);

			Vector3 norm = Vector3.Cross(org, dst).normalized;
			Vector3 up = org.normalized;
			Vector3 right = Vector3.Cross(norm, up).normalized;

			Vector4 P = (up * Mathf.Cos(ang * t) + right * Mathf.Sin(ang * t)) * radius;
			P.w = Mathf.Lerp(org.w, dst.w, t);
			//Vector3 P = new Vector3(Mathf.Sin(t*ang),Mathf.Cos(t*ang),0);
			return P;

			//return Vector3.Cross(org, dst);
		}

		public static Vector3 GetOrthodromePoint(Vector3 org, Vector3 dst, float dist, float radius = 1)
		//t=[0,distance]
		{
			//float ang = OrthodromeAngle ( org, dst );

			Vector3 norm = Vector3.Cross(org, dst).normalized;
			Vector3 up = org.normalized;
			Vector3 right = Vector3.Cross(norm, up).normalized;

			dist /= radius;
			Vector3 P = (up * Mathf.Cos(dist) + right * Mathf.Sin(dist)) * radius;

			//Vector3 P = new Vector3(Mathf.Sin(t*ang),Mathf.Cos(t*ang),0);
			return P;

			//return Vector3.Cross(org, dst);
		}

		public static Vector3 GetOrthodromeMidPoint(Vector3 org, Vector3 dst, float radius = 1) //t=[0,distance]
		{
			float t = 0.5f;
			float ang = OrthodromeAngle(org, dst);

			Vector3 norm = Vector3.Cross(org, dst).normalized;
			Vector3 up = org.normalized;
			Vector3 right = Vector3.Cross(norm, up).normalized;

			Vector3 P = (up * Mathf.Cos(ang * t) + right * Mathf.Sin(ang * t)) * radius;

			//Vector3 P = new Vector3(Mathf.Sin(t*ang),Mathf.Cos(t*ang),0);
			return P;

			//return Vector3.Cross(org, dst);
		}

		public static int triangularToIndex(int r, int s, int n)
		{
			n += 2;
			if ((r >= 0 && s >= 0) && r < n && s < n - r)
			{
				int ms = n - 1;
				int c = -1;
				for (int i = 0; i <= r; i++)
				{
					if (i == r)
						ms = s;
					c += 1 + ms--;
				}
				return c;
			}
			else return -1;
		}

		/*
		public static void DebugDrawOrthodromeLine(Vector3 org, Vector3 dst, float density)
		{
			float dist = OrthodromeDistance(org, dst);
			float step = density;
			int count = Mathf.FloorToInt(dist / density + 0.9999f);
			//if(count <= 40){
			while (step <= dist)
			{
				float t = step / dist;
				float t0 = (step - density) / dist;
				Debug.DrawLine(GetOrthodromePointRelative(org, dst, t0), GetOrthodromePointRelative(org, dst, t));
				step += density;
			}
		}*/

		/*
		public static Vector3 GetOrthodromePoint(Vector2 org, Vector2 dst, float t) //t=[0,1]
		{
			float ang = OrthodromeAngle(org, dst); Debug.Log(ang.ToString("0.00"));
			Vector2 Pc = new Vector2(0, t + ang) + org;

			//Vector3 P = new Vector3(Mathf.Sin(t*ang),Mathf.Cos(t*ang),0);
			Vector3 P = GetGlobeCoord(Pc);
			return P;

			//return Vector3.Cross(org, dst);
		}*/

		#endregion Methods

		#region DebugDraw

		public static void GizmoDrawCircle(Transform trans, float radius)
		{
			Vector3 lpos = Vector3.zero;

			for (int i = 0; i <= 32; i++)
			{
				float angle = (float)i * 2 / 32f;
				Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.PI),
					0,
					Mathf.Sin(angle * Mathf.PI));
				pos *= radius;
				pos = trans.localToWorldMatrix.MultiplyPoint(pos);

				if (i > 0)
					Gizmos.DrawLine(lpos, pos);

				lpos = pos;
			}
		}

		public static void DebugDrawOnSphere(Vector3 pos, float length)
		{
			if (Vector3.Dot(pos, Camera.main.transform.forward) < 1)
				Debug.DrawLine(pos, pos + pos.normalized * length);
		}

		public static void DebugDrawOnSphere(Vector3 pos, float length, Color col)
		{
			if (Vector3.Dot(pos, Camera.main.transform.forward) < 1)
				Debug.DrawLine(pos, pos + pos.normalized * length, col);
		}

		public static void DebugDrawLineOnSphere(Vector3 pA, Vector3 pB)
		{
			if (Vector3.Dot(pA + pB, Camera.main.transform.forward) < 1)
				Debug.DrawLine(pA, pB);
		}

		public static void DebugDrawLineOnSphere(Vector3 pA, Vector3 pB, Color col)
		{
			if (Vector3.Dot(pA + pB, Camera.main.transform.forward) < 1)
				Debug.DrawLine(pA, pB, col);
		}

		public static void DebugDrawOrthodromeLine(Vector3 org, Vector3 dst, float radius = 1)
		{
			float density = sDensity;
			float dist = Mathf.Min(OrthodromeDistance(org, dst), radius * 0.5f);
			float step = density;
			//int count = Mathf.FloorToInt(dist / density + 0.9999f);

			bool last = false;
			while (step <= dist || !last)
			{
				if (step > dist)
					last = true;

				float t = Mathf.Min(1f, step / dist);
				float t0 = (step - density) / dist;

				Debug.DrawLine(GetOrthodromePointRelative(org, dst, t0), GetOrthodromePointRelative(org, dst, t));
				step += density;
			}
		}

		public static void DebugDrawOrthodromeLine(Vector3 org, Vector3 dst, float density, float radius = 1)
		{
			density = Mathf.Max(0.1f, Mathf.Abs(density));
			float dist = Mathf.Min(OrthodromeDistance(org, dst), radius * 0.5f);
			float step = density;
			//int count = Mathf.FloorToInt(dist / density + 0.9999f);

			bool last = false;
			while (step <= dist || !last)
			{
				if (step > dist)
					last = true;

				float t = Mathf.Min(1f, step / dist);
				float t0 = (step - density) / dist;

				Debug.DrawLine(GetOrthodromePointRelative(org, dst, t0), GetOrthodromePointRelative(org, dst, t));
				step += density;
			}
		}

		public static void DebugDrawOrthodromeLine(Vector3 org, Vector3 dst, float density, Color col)
		{
			density = Mathf.Max(0.1f, Mathf.Abs(density));
			float dist = OrthodromeDistance(org, dst);
			float step = 0;
			float lastStep = step;
			float t, t0;
			step += density;
			int count = Mathf.FloorToInt(dist / density + 0.9999f);
			//if(count <= 40){
			while (step <= dist)
			{
				t = step / dist;
				t0 = lastStep / dist;
				Debug.DrawLine(GetOrthodromePointRelative(org, dst, t0), GetOrthodromePointRelative(org, dst, t), col);
				lastStep = step;
				step += density;
				count++;
			}
			t = 1;
			t0 = lastStep / dist;
			Debug.DrawLine(GetOrthodromePointRelative(org, dst, t0), GetOrthodromePointRelative(org, dst, t), col);
		}

		#endregion DebugDraw
	}
}