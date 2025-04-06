using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 定义判断点是否处于多边形内部的方法。
    /// </summary>
    public interface IInclusionOfAPointInAPolygonAlgorithm
    {
        /// <summary>
        /// 判断点是否处于多边形内部。
        /// </summary>
        /// <param name="point">点。</param>
        /// <param name="polygonVertices">连成多边形的各点。</param>
        /// <returns>如果 <paramref name="point"/> 位于由 <paramref name="polygonVertices"/> 连成的多边形内，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonVertices"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="polygonVertices"/> 的长度小于 3（不能连成多边形）。</exception>
        bool IsPointInsidePolygon(Vector2 point, IList<Vector2> polygonVertices);
    }
}
