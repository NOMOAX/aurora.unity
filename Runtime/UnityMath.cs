using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of mathematical methods.
    /// </summary>
    public static class UnityMath
    {
        /// <summary>
        /// Gets a plane vector whose <see cref="Vector2.x"/> component is the cosine of the specified angle and whose <see cref="Vector2.y"/> component is the sine of the specified angle.
        /// </summary>
        /// <param name="angle">The angle, in radians.</param>
        /// <returns>A plane vector whose <see cref="Vector2.x"/> component is the cosine of <paramref name="angle"/> and whose <see cref="Vector2.y"/> component is the sine of <paramref name="angle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CosSin(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// Similar to <see cref="Rect.PointToNormalized"/>, but does not clamp each component of the output value to the [0, 1] range.
        /// </summary>
        /// <param name="pixelAdjustedRect">The rectangle.</param>
        /// <param name="point">The point.</param>
        /// <returns>The normalized position of <paramref name="point"/> within <paramref name="pixelAdjustedRect"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 GetUV(Rect pixelAdjustedRect, Vector2 point)
        {
            return PointToNormalizedUnclamped(pixelAdjustedRect, point);
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>.
        /// </summary>
        /// <param name="a">The start value for the linear interpolation.</param>
        /// <param name="b">The end value for the linear interpolation.</param>
        /// <param name="t">The interpolation. Each of its components will be used to interpolate the corresponding component of <paramref name="a"/> and <paramref name="b"/>.</param>
        /// <returns>The result of the linear interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, Vector2 t)
        {
            return new Vector2(Mathf.LerpUnclamped(a.x, b.x, t.x), Mathf.LerpUnclamped(a.y, b.y, t.y));
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>.
        /// </summary>
        /// <param name="a">The start value for the linear interpolation.</param>
        /// <param name="b">The end value for the linear interpolation.</param>
        /// <param name="t">The interpolation. Each of its components will be used to interpolate the corresponding component of <paramref name="a"/> and <paramref name="b"/>.</param>
        /// <returns>The result of the linear interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, Vector3 t)
        {
            return new Vector3(
                Mathf.LerpUnclamped(a.x, b.x, t.x),
                Mathf.LerpUnclamped(a.y, b.y, t.y),
                Mathf.LerpUnclamped(a.z, b.z, t.z)
            );
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/>.
        /// </summary>
        /// <param name="a">The start value for the linear interpolation.</param>
        /// <param name="b">The end value for the linear interpolation.</param>
        /// <param name="t">The interpolation. Each of its components will be used to interpolate the corresponding component of <paramref name="a"/> and <paramref name="b"/>.</param>
        /// <returns>The result of the linear interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, Vector4 t)
        {
            return new Vector4(
                Mathf.LerpUnclamped(a.x, b.x, t.x),
                Mathf.LerpUnclamped(a.y, b.y, t.y),
                Mathf.LerpUnclamped(a.z, b.z, t.z),
                Mathf.LerpUnclamped(a.w, b.w, t.w)
            );
        }

        /// <summary>
        /// Similar to <see cref="Rect.NormalizedToPoint"/>, but does not clamp each component of the return value to the [0, 1] range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedToPointUnclamped(Rect rectangle, Vector2 normalizedRectCoordinates)
        {
            return new Vector2(
                Mathf.LerpUnclamped(rectangle.xMin, rectangle.xMax, normalizedRectCoordinates.x),
                Mathf.LerpUnclamped(rectangle.yMin, rectangle.yMax, normalizedRectCoordinates.y)
            );
        }

        /// <summary>
        /// Similar to <see cref="Rect.PointToNormalized"/>, but does not clamp each component of the return value to the [0, 1] range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 PointToNormalizedUnclamped(Rect rectangle, Vector2 point)
        {
            return new Vector2(
                (float)InterpolationUtility.InverseLinearInterpolate(rectangle.xMin, rectangle.xMax, point.x),
                (float)InterpolationUtility.InverseLinearInterpolate(rectangle.yMin, rectangle.yMax, point.y)
            );
        }

        /// <summary>
        /// Gets the barycentric coordinates of point <paramref name="p"/>, or of its projection, on triangle <paramref name="a"/><paramref name="b"/><paramref name="c"/>.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="a">A vertex of the triangle.</param>
        /// <param name="b">Another vertex of the triangle.</param>
        /// <param name="c">Yet another vertex of the triangle.</param>
        /// <returns>If point <paramref name="p"/> is in plane <paramref name="a"/><paramref name="b"/><paramref name="c"/>, the barycentric coordinates of <paramref name="p"/> on triangle <paramref name="a"/><paramref name="b"/><paramref name="c"/>; otherwise, the barycentric coordinates of its projection on the plane.</returns>
        /// <remarks>
        /// The barycentric coordinates on a triangle are 3 values, generally written as α, β, γ; here a <see cref="Vector3"/> value aggregates them.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetBarycentricCoordinatesOfTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var u     = b - a;
            var v     = c - a;
            var n     = Vector3.Cross(u, v);
            var w     = p - a;
            var gamma = Vector3.Dot(Vector3.Cross(u, w), n) / n.sqrMagnitude;
            var beta  = Vector3.Dot(Vector3.Cross(w, v), n) / n.sqrMagnitude;
            var alpha = 1 - gamma - beta;
            return new Vector3(alpha, beta, gamma);
        }

        /// <summary>
        /// Given the barycentric coordinates of a point (or its projection) on a triangle, determines whether that point (or its projection) is inside the triangle.
        /// </summary>
        /// <param name="barycentricCoordinates">
        /// The barycentric coordinates of the point (or its projection) on the triangle.
        /// <br/>
        /// It is recommended to pass the return value of <see cref="GetBarycentricCoordinatesOfTriangle"/>. 
        /// </param>
        /// <param name="error">
        /// The maximum allowed error.
        /// <br/>
        /// Due to the imprecision of floating-point numbers, error is difficult to avoid; adjust this value according to the actual situation.
        /// </param>
        /// <returns>Determines whether the point whose barycentric coordinates (of itself or its projection) on the triangle are <paramref name="barycentricCoordinates"/> is inside the triangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInsideTriangle(Vector3 barycentricCoordinates, float error = 0.00001f)
        {
            var alpha = barycentricCoordinates.x;
            var beta  = barycentricCoordinates.y;
            var gamma = barycentricCoordinates.z;
            return -error <= alpha && alpha <= 1 + error && -error <= beta && beta <= 1 + error && -error <= gamma &&
                   gamma <= 1 + error;
        }

        /// <summary>
        /// Determines whether a point is inside a polygon.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="polygonVertices">The vertices that form the polygon.</param>
        /// <returns><see langword="true"/> if <paramref name="point"/> is inside the polygon formed by <paramref name="polygonVertices"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="polygonVertices"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">The length of <paramref name="polygonVertices"/> is less than 3 (cannot form a polygon).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInsidePolygon(Vector2 point, IList<Vector2> polygonVertices)
        {
            IInclusionOfAPointInAPolygonAlgorithm inclusionOfAPointInAPolygonAlgorithm = new WindingNumber();
            return inclusionOfAPointInAPolygonAlgorithm.IsPointInsidePolygon(point, polygonVertices);
        }
    }
}
