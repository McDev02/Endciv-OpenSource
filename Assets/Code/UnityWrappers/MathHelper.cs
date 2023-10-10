#region Assembly UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Standort unbekannt
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngineInternal;

namespace Endciv
{
    //
    // Zusammenfassung:
    //     A collection of common math functions.
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct Mathf
    {
        //
        // Zusammenfassung:
        //     The well-known 3.14159265358979... value (Read Only).
        public const float PI = (float)Math.PI;

        //
        // Zusammenfassung:
        //     A representation of positive infinity (Read Only).
        public const float Infinity = float.PositiveInfinity;

        //
        // Zusammenfassung:
        //     A representation of negative infinity (Read Only).
        public const float NegativeInfinity = float.NegativeInfinity;

        //
        // Zusammenfassung:
        //     Degrees-to-radians conversion constant (Read Only).
        public const float Deg2Rad = (float)Math.PI / 180f;

        //
        // Zusammenfassung:
        //     Radians-to-degrees conversion constant (Read Only).
        public const float Rad2Deg = 57.29578f;

        //
        // Zusammenfassung:
        //     A tiny floating point value (Read Only).
        public static readonly float Epsilon = (MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal);


        //
        // Zusammenfassung:
        //     Returns the sine of angle f.
        //
        // Parameter:
        //   f:
        //     The input angle, in radians.
        //
        // Rückgabewerte:
        //     The return value between -1 and +1.
        public static float Sin(float f)
        {
            return (float)Math.Sin(f);
        }

        //
        // Zusammenfassung:
        //     Returns the cosine of angle f.
        //
        // Parameter:
        //   f:
        //     The input angle, in radians.
        //
        // Rückgabewerte:
        //     The return value between -1 and 1.
        public static float Cos(float f)
        {
            return (float)Math.Cos(f);
        }

        //
        // Zusammenfassung:
        //     Returns the tangent of angle f in radians.
        //
        // Parameter:
        //   f:
        public static float Tan(float f)
        {
            return (float)Math.Tan(f);
        }

        //
        // Zusammenfassung:
        //     Returns the arc-sine of f - the angle in radians whose sine is f.
        //
        // Parameter:
        //   f:
        public static float Asin(float f)
        {
            return (float)Math.Asin(f);
        }

        //
        // Zusammenfassung:
        //     Returns the arc-cosine of f - the angle in radians whose cosine is f.
        //
        // Parameter:
        //   f:
        public static float Acos(float f)
        {
            return (float)Math.Acos(f);
        }

        //
        // Zusammenfassung:
        //     Returns the arc-tangent of f - the angle in radians whose tangent is f.
        //
        // Parameter:
        //   f:
        public static float Atan(float f)
        {
            return (float)Math.Atan(f);
        }

        //
        // Zusammenfassung:
        //     Returns the angle in radians whose Tan is y/x.
        //
        // Parameter:
        //   y:
        //
        //   x:
        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        //
        // Zusammenfassung:
        //     Returns square root of f.
        //
        // Parameter:
        //   f:
        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt(f);
        }

        //
        // Zusammenfassung:
        //     Returns the absolute value of f.
        //
        // Parameter:
        //   f:
        public static float Abs(float f)
        {
            return Math.Abs(f);
        }

        //
        // Zusammenfassung:
        //     Returns the absolute value of value.
        //
        // Parameter:
        //   value:
        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        //
        // Zusammenfassung:
        //     Returns the smallest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static float Min(float a, float b)
        {
            return (a < b) ? a : b;
        }

        //
        // Zusammenfassung:
        //     Returns the smallest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static float Min(params float[] values)
        {
            int num = values.Length;
            if (num == 0)
            {
                return 0f;
            }

            float num2 = values[0];
            for (int i = 1; i < num; i++)
            {
                if (values[i] < num2)
                {
                    num2 = values[i];
                }
            }

            return num2;
        }

        //
        // Zusammenfassung:
        //     Returns the smallest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static int Min(int a, int b)
        {
            return (a < b) ? a : b;
        }

        //
        // Zusammenfassung:
        //     Returns the smallest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static int Min(params int[] values)
        {
            int num = values.Length;
            if (num == 0)
            {
                return 0;
            }

            int num2 = values[0];
            for (int i = 1; i < num; i++)
            {
                if (values[i] < num2)
                {
                    num2 = values[i];
                }
            }

            return num2;
        }

        //
        // Zusammenfassung:
        //     Returns largest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static float Max(float a, float b)
        {
            return (a > b) ? a : b;
        }

        //
        // Zusammenfassung:
        //     Returns largest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static float Max(params float[] values)
        {
            int num = values.Length;
            if (num == 0)
            {
                return 0f;
            }

            float num2 = values[0];
            for (int i = 1; i < num; i++)
            {
                if (values[i] > num2)
                {
                    num2 = values[i];
                }
            }

            return num2;
        }

        //
        // Zusammenfassung:
        //     Returns the largest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static int Max(int a, int b)
        {
            return (a > b) ? a : b;
        }

        //
        // Zusammenfassung:
        //     Returns the largest of two or more values.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   values:
        public static int Max(params int[] values)
        {
            int num = values.Length;
            if (num == 0)
            {
                return 0;
            }

            int num2 = values[0];
            for (int i = 1; i < num; i++)
            {
                if (values[i] > num2)
                {
                    num2 = values[i];
                }
            }

            return num2;
        }

        //
        // Zusammenfassung:
        //     Returns f raised to power p.
        //
        // Parameter:
        //   f:
        //
        //   p:
        public static float Pow(float f, float p)
        {
            return (float)Math.Pow(f, p);
        }

        //
        // Zusammenfassung:
        //     Returns e raised to the specified power.
        //
        // Parameter:
        //   power:
        public static float Exp(float power)
        {
            return (float)Math.Exp(power);
        }

        //
        // Zusammenfassung:
        //     Returns the logarithm of a specified number in a specified base.
        //
        // Parameter:
        //   f:
        //
        //   p:
        public static float Log(float f, float p)
        {
            return (float)Math.Log(f, p);
        }

        //
        // Zusammenfassung:
        //     Returns the natural (base e) logarithm of a specified number.
        //
        // Parameter:
        //   f:
        public static float Log(float f)
        {
            return (float)Math.Log(f);
        }

        //
        // Zusammenfassung:
        //     Returns the base 10 logarithm of a specified number.
        //
        // Parameter:
        //   f:
        public static float Log10(float f)
        {
            return (float)Math.Log10(f);
        }

        //
        // Zusammenfassung:
        //     Returns the smallest integer greater to or equal to f.
        //
        // Parameter:
        //   f:
        public static float Ceil(float f)
        {
            return (float)Math.Ceiling(f);
        }

        //
        // Zusammenfassung:
        //     Returns the largest integer smaller than or equal to f.
        //
        // Parameter:
        //   f:
        public static float Floor(float f)
        {
            return (float)Math.Floor(f);
        }

        //
        // Zusammenfassung:
        //     Returns f rounded to the nearest integer.
        //
        // Parameter:
        //   f:
        public static float Round(float f)
        {
            return (float)Math.Round(f);
        }

        //
        // Zusammenfassung:
        //     Returns the smallest integer greater to or equal to f.
        //
        // Parameter:
        //   f:
        public static int CeilToInt(float f)
        {
            return (int)Math.Ceiling(f);
        }

        //
        // Zusammenfassung:
        //     Returns the largest integer smaller to or equal to f.
        //
        // Parameter:
        //   f:
        public static int FloorToInt(float f)
        {
            return (int)Math.Floor(f);
        }

        //
        // Zusammenfassung:
        //     Returns f rounded to the nearest integer.
        //
        // Parameter:
        //   f:
        public static int RoundToInt(float f)
        {
            return (int)Math.Round(f);
        }

        //
        // Zusammenfassung:
        //     Returns the sign of f.
        //
        // Parameter:
        //   f:
        public static float Sign(float f)
        {
            return (f >= 0f) ? 1f : (-1f);
        }

        //
        // Zusammenfassung:
        //     Clamps the given value between the given minimum float and maximum float values.
        //     Returns the given value if it is within the min and max range.
        //
        // Parameter:
        //   value:
        //     The floating point value to restrict inside the range defined by the min and
        //     max values.
        //
        //   min:
        //     The minimum floating point value to compare against.
        //
        //   max:
        //     The maximum floating point value to compare against.
        //
        // Rückgabewerte:
        //     The float result between the min and max values.
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        //
        // Zusammenfassung:
        //     Clamps the given value between a range defined by the given minimum integer and
        //     maximum integer values. Returns the given value if it is within min and max.
        //
        // Parameter:
        //   value:
        //     The integer point value to restrict inside the min-to-max range
        //
        //   min:
        //     The minimum integer point value to compare against.
        //
        //   max:
        //     The maximum integer point value to compare against.
        //
        // Rückgabewerte:
        //     The int result between min and max values.
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        //
        // Zusammenfassung:
        //     Clamps value between 0 and 1 and returns value.
        //
        // Parameter:
        //   value:
        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }

        //
        // Zusammenfassung:
        //     Linearly interpolates between a and b by t.
        //
        // Parameter:
        //   a:
        //     The start value.
        //
        //   b:
        //     The end value.
        //
        //   t:
        //     The interpolation value between the two floats.
        //
        // Rückgabewerte:
        //     The interpolated float result between the two float values.
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        //
        // Zusammenfassung:
        //     Linearly interpolates between a and b by t with no limit to t.
        //
        // Parameter:
        //   a:
        //     The start value.
        //
        //   b:
        //     The end value.
        //
        //   t:
        //     The interpolation between the two floats.
        //
        // Rückgabewerte:
        //     The float value as a result from the linear interpolation.
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        //
        // Zusammenfassung:
        //     Same as Lerp but makes sure the values interpolate correctly when they wrap around
        //     360 degrees.
        //
        // Parameter:
        //   a:
        //
        //   b:
        //
        //   t:
        public static float LerpAngle(float a, float b, float t)
        {
            float num = Repeat(b - a, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }

            return a + num * Clamp01(t);
        }

        //
        // Zusammenfassung:
        //     Moves a value current towards target.
        //
        // Parameter:
        //   current:
        //     The current value.
        //
        //   target:
        //     The value to move towards.
        //
        //   maxDelta:
        //     The maximum change that should be applied to the value.
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Sign(target - current) * maxDelta;
        }

        //
        // Zusammenfassung:
        //     Same as MoveTowards but makes sure the values interpolate correctly when they
        //     wrap around 360 degrees.
        //
        // Parameter:
        //   current:
        //
        //   target:
        //
        //   maxDelta:
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float num = DeltaAngle(current, target);
            if (0f - maxDelta < num && num < maxDelta)
            {
                return target;
            }

            target = current + num;
            return MoveTowards(current, target, maxDelta);
        }

        //
        // Zusammenfassung:
        //     Interpolates between min and max with smoothing at the limits.
        //
        // Parameter:
        //   from:
        //
        //   to:
        //
        //   t:
        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        public static float Gamma(float value, float absmax, float gamma)
        {
            bool flag = false;
            if (value < 0f)
            {
                flag = true;
            }

            float num = Abs(value);
            if (num > absmax)
            {
                return flag ? (0f - num) : num;
            }

            float num2 = Pow(num / absmax, gamma) * absmax;
            return flag ? (0f - num2) : num2;
        }

        //
        // Zusammenfassung:
        //     Compares two floating point values and returns true if they are similar.
        //
        // Parameter:
        //   a:
        //
        //   b:
        public static bool Approximately(float a, float b)
        {
            return Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);
        }


        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            smoothTime = Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            float value = current - target;
            float num4 = target;
            float num5 = maxSpeed * smoothTime;
            value = Clamp(value, 0f - num5, num5);
            target = current - value;
            float num6 = (currentVelocity + num * value) * deltaTime;
            currentVelocity = (currentVelocity - num * num6) * num3;
            float num7 = target + (value + num6) * num3;
            if (num4 - current > 0f == num7 > num4)
            {
                num7 = num4;
                currentVelocity = (num7 - num4) / deltaTime;
            }

            return num7;
        }


        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        {
            target = current + DeltaAngle(current, target);
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        //
        // Zusammenfassung:
        //     Loops the value t, so that it is never larger than length and never smaller than
        //     0.
        //
        // Parameter:
        //   t:
        //
        //   length:
        public static float Repeat(float t, float length)
        {
            return Clamp(t - Floor(t / length) * length, 0f, length);
        }

        //
        // Zusammenfassung:
        //     PingPong returns a value that will increment and decrement between the value
        //     0 and length.
        //
        // Parameter:
        //   t:
        //
        //   length:
        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Abs(t - length);
        }

        //
        // Zusammenfassung:
        //     Calculates the linear parameter t that produces the interpolant value within
        //     the range [a, b].
        //
        // Parameter:
        //   a:
        //     Start value.
        //
        //   b:
        //     End value.
        //
        //   value:
        //     Value between start and end.
        //
        // Rückgabewerte:
        //     Percentage of value between start and end.
        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return Clamp01((value - a) / (b - a));
            }

            return 0f;
        }

        //
        // Zusammenfassung:
        //     Calculates the shortest difference between two given angles given in degrees.
        //
        // Parameter:
        //   current:
        //
        //   target:
        public static float DeltaAngle(float current, float target)
        {
            float num = Repeat(target - current, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }

            return num;
        }

        internal static long RandomToLong(System.Random r)
        {
            byte[] array = new byte[8];
            r.NextBytes(array);
            return (long)(BitConverter.ToUInt64(array, 0) & 0x7FFFFFFFFFFFFFFFL);
        }
    }
}