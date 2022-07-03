using System;
using UnityEngine;

// base on https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Collision.cs

namespace Endciv
{
    public static partial class CivMath
    {
        public struct RectBoundsf
        {
            public Vector2 Minimum;
            public Vector2 Maximum;
        }

        public struct Ray2
        {
            public Vector2 Position;
            public Vector2 Direction;
            public Vector2 Normal { get { return new Vector2(Direction.y, -Direction.x); } }

            public Ray2(Vector2 position, Vector2 direction)
            {
                Position = position;
                Direction = direction;
            }
        }

        /// <summary>
        /// Only suitable for values between 0 and 1.
        /// </summary>
        internal static float SqrtFast(float v)
        {
            v = 1 - v;
            return 1 - v * v;
        }

        public struct Plane2
        {
            /// <summary>
            /// The normal vector of the plane.
            /// </summary>
            public Vector2 Normal;

            /// <summary>
            /// The distance of the plane along its normal from the origin.
            /// </summary>
            public float D;
        }

        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        public static bool LineIntersectsRect(Vector2 p1, Vector2 p2, RectBounds r, out Vector2 hitPoint)
        {
            return
                SegmentIntersectsSegment(p1, p2, (Vector2)r.Minimum, new Vector2(r.Maximum.X, r.Minimum.Y),
                    out hitPoint) ||
                SegmentIntersectsSegment(p1, p2, new Vector2(r.Maximum.X, r.Minimum.Y), (Vector2)r.Maximum,
                    out hitPoint) ||
                SegmentIntersectsSegment(p1, p2, (Vector2)r.Maximum, new Vector2(r.Minimum.X, r.Maximum.Y),
                    out hitPoint) ||
                SegmentIntersectsSegment(p1, p2, new Vector2(r.Minimum.X, r.Maximum.Y), (Vector2)r.Minimum,
                    out hitPoint);
        }

        public static bool SegmentIntersectsSegment(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 result)
        {
            var b = a2 - a1;
            var d = b2 - b1;
            var bDotDPerp = b.x * d.y - b.y * d.x;

            // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            if (bDotDPerp == 0)
            {
                result = new Vector2();
                return false;
            }

            float scale = 1f / bDotDPerp;

            var c = b1 - a1;

            var t = (c.x * d.y - c.y * d.x) * scale;
            if (t < 0 || t > 1)
            {
                result = new Vector2();
                return false;
            }

            var u = (c.x * b.y - c.y * b.x) * scale;
            if (u < 0 || u > 1)
            {
                result = new Vector2();
                return false;
            }

            result = a1 + b * t;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a Ray and a Box.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray2 ray, ref RectBoundsf box, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;
            float inverse, t1, t2, temp;

            if (IsZero(ray.Direction.x))
            {
                if (ray.Position.x < box.Minimum.x || ray.Position.x > box.Maximum.x)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                inverse = 1.0f / ray.Direction.x;
                t1 = (box.Minimum.x - ray.Position.x) * inverse;
                t2 = (box.Maximum.x - ray.Position.x) * inverse;

                if (t1 > t2)
                {
                    temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (IsZero(ray.Direction.y))
            {
                if (ray.Position.y < box.Minimum.y || ray.Position.y > box.Maximum.y)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                inverse = 1.0f / ray.Direction.y;
                t1 = (box.Minimum.y - ray.Position.y) * inverse;
                t2 = (box.Maximum.y - ray.Position.y) * inverse;

                if (t1 > t2)
                {
                    temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a Ray and a Plane.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref Ray2 ray, ref Plane2 plane, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            float direction;
            Dot(ref plane.Normal, ref ray.Direction, out direction);

            if (IsZero(direction))
            {
                distance = 0f;
                return false;
            }

            float position;
            Dot(ref plane.Normal, ref ray.Position, out position);
            distance = (-plane.D - position) / direction;

            if (distance < 0f)
            {
                distance = 0f;
                return false;
            }

            return true;
        }

        public static void Dot(ref Vector2 left, ref Vector2 right, out float result)
        {
            result = (left.x * right.x) + (left.y * right.y);
        }
    }
}