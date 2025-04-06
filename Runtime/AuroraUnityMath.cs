using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组数学方法。
    /// </summary>
    public static class AuroraUnityMath
    {
        /// <summary>
        /// 获取一个平面向量，它的 <see cref="Vector2.x"/> 分量是指定角的余弦，<see cref="Vector2.y"/> 分量是指定角的正弦。
        /// </summary>
        /// <param name="angle">以弧度为单位的角。</param>
        /// <returns>一个平面向量，它的 <see cref="Vector2.x"/> 分量是 <paramref name="angle"/> 的余弦，<see cref="Vector2.y"/> 分量是 <paramref name="angle"/> 的正弦。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CosSin(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// 与 <see cref="Rect.PointToNormalized"/> 类似，但不会将输出值的每个分量限制在 [0, 1] 范围内。
        /// </summary>
        /// <param name="pixelAdjustedRect">矩形。</param>
        /// <param name="point">点。</param>
        /// <returns><paramref name="point"/> 处于 <paramref name="pixelAdjustedRect"/> 中的标准化位置。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 GetUV(Rect pixelAdjustedRect, Vector2 point)
        {
            return PointToNormalizedUnclamped(pixelAdjustedRect, point);
        }

        /// <summary>
        /// 与 <see cref="Mathf.InverseLerp"/> 类似，但不会将返回值限制在 [0, 1] 范围内。
        /// </summary>
        /// <param name="a">范围的开始值。</param>
        /// <param name="b">范围的结束值。</param>
        /// <param name="value">要计算在 [<paramref name="a"/>, <paramref name="b"/>] 范围内的插值的值。</param>
        /// <returns><paramref name="value"/> 在 [<paramref name="a"/>, <paramref name="b"/>] 范围内的插值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            return a != b ? (value - a) / (b - a) : default;
        }

        /// <summary>
        /// 与 <see cref="Mathf.InverseLerp"/> 类似，但不会将返回值限制在 [0, 1] 范围内。
        /// </summary>
        /// <param name="a">范围的开始值。</param>
        /// <param name="b">范围的结束值。</param>
        /// <param name="value">要计算在 [<paramref name="a"/>, <paramref name="b"/>] 范围内的插值的值。</param>
        /// <param name="returnValueWhenAEqualToB">当 <paramref name="a"/> 等于 <paramref name="b"/> 时，返回此值。</param>
        /// <returns><paramref name="value"/> 在 [<paramref name="a"/>, <paramref name="b"/>] 范围内的插值。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerpUnclamped(float a, float b, float value, float returnValueWhenAEqualToB)
        {
            return a != b ? (value - a) / (b - a) : returnValueWhenAEqualToB;
        }

        /// <summary>
        /// 在 <paramref name="a"/> 与 <paramref name="b"/> 之间通过 <paramref name="t"/> 进行线性插值。
        /// </summary>
        /// <param name="a">进行线性插值的开始值。</param>
        /// <param name="b">进行线性插值的结束值。</param>
        /// <param name="t">插值。它的各个分量将分别用于对 <paramref name="a"/> 与 <paramref name="b"/> 的对应分量进行插值。</param>
        /// <returns>线性插值的结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, Vector2 t)
        {
            return new Vector2(Mathf.LerpUnclamped(a.x, b.x, t.x), Mathf.LerpUnclamped(a.y, b.y, t.y));
        }

        /// <summary>
        /// 在 <paramref name="a"/> 与 <paramref name="b"/> 之间通过 <paramref name="t"/> 进行线性插值。
        /// </summary>
        /// <param name="a">进行线性插值的开始值。</param>
        /// <param name="b">进行线性插值的结束值。</param>
        /// <param name="t">插值。它的各个分量将分别用于对 <paramref name="a"/> 与 <paramref name="b"/> 的对应分量进行插值。</param>
        /// <returns>线性插值的结果。</returns>
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
        /// 在 <paramref name="a"/> 与 <paramref name="b"/> 之间通过 <paramref name="t"/> 进行线性插值。
        /// </summary>
        /// <param name="a">进行线性插值的开始值。</param>
        /// <param name="b">进行线性插值的结束值。</param>
        /// <param name="t">插值。它的各个分量将分别用于对 <paramref name="a"/> 与 <paramref name="b"/> 的对应分量进行插值。</param>
        /// <returns>线性插值的结果。</returns>
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
        /// 与 <see cref="Rect.NormalizedToPoint"/> 类似，但不会将返回值的各分量限制在 [0, 1] 范围内。
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
        /// 与 <see cref="Rect.PointToNormalized"/> 类似，但不会将返回值的各分量限制在 [0, 1] 范围内。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 PointToNormalizedUnclamped(Rect rectangle, Vector2 point)
        {
            return new Vector2(
                InverseLerpUnclamped(rectangle.xMin, rectangle.xMax, point.x),
                InverseLerpUnclamped(rectangle.yMin, rectangle.yMax, point.y)
            );
        }

        /// <summary>
        /// 获取点 <paramref name="p"/> 或者其投影在三角形 <paramref name="a"/><paramref name="b"/><paramref name="c"/> 上的重心坐标。
        /// </summary>
        /// <param name="p">一个点。</param>
        /// <param name="a">三角形的一个顶点。</param>
        /// <param name="b">三角形的另一个顶点。</param>
        /// <param name="c">三角形的又一个顶点。</param>
        /// <returns>如果点 <paramref name="p"/> 在平面 <paramref name="a"/><paramref name="b"/><paramref name="c"/> 中，则为 <paramref name="p"/> 在三角形 <paramref name="a"/><paramref name="b"/><paramref name="c"/> 上的重心坐标；否则，为其在平面的投影的重心坐标。</returns>
        /// <remarks>
        /// 三角形上的重心坐标为 3 个值，一般分别写作 α、β、γ，这里使用一个 <see cref="Vector3"/> 值将它们聚合起来。
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
            var alpha = 1f - gamma - beta;
            return new Vector3(alpha, beta, gamma);
        }

        /// <summary>
        /// 已知点或其投影在三角形上的重心坐标，判断该点或其投影是否在三角形内。
        /// </summary>
        /// <param name="barycentricCoordinates">
        /// 点或其投影在三角形上的重心坐标。
        /// <br/>
        /// 建议传入 <see cref="GetBarycentricCoordinatesOfTriangle"/> 方法的返回值。
        /// </param>
        /// <param name="error">
        /// 允许的最大误差。
        /// <br/>
        /// 由于浮点数的不精确性，误差难以避免，请实际情况调整该值。
        /// </param>
        /// <returns>判断其本身或其投影在三角形上的重心坐标为 <paramref name="barycentricCoordinates"/> 的点是否在三角形内。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInsideTriangle(Vector3 barycentricCoordinates, float error = 0.00001f)
        {
            var alpha = barycentricCoordinates.x;
            var beta  = barycentricCoordinates.y;
            var gamma = barycentricCoordinates.z;
            return -error <= alpha && alpha <= 1f + error && -error <= beta && beta <= 1f + error && -error <= gamma &&
                   gamma <= 1f + error;
        }

        /// <summary>
        /// 判断点是否处于多边形内部。
        /// </summary>
        /// <param name="point">点。</param>
        /// <param name="polygonVertices">连成多边形的各点。</param>
        /// <returns>如果 <paramref name="point"/> 位于由 <paramref name="polygonVertices"/> 连成的多边形内，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="polygonVertices"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="System.ArgumentException"><paramref name="polygonVertices"/> 的长度小于 3（不能连成多边形）。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInsidePolygon(Vector2 point, IList<Vector2> polygonVertices)
        {
            IInclusionOfAPointInAPolygonAlgorithm inclusionOfAPointInAPolygonAlgorithm = new WindingNumber();
            return inclusionOfAPointInAPolygonAlgorithm.IsPointInsidePolygon(point, polygonVertices);
        }
    }
}
