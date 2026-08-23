using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Defines methods to determine whether a point is inside a polygon.
    /// </summary>
    public interface IInclusionOfAPointInAPolygonAlgorithm
    {
        /// <summary>
        /// Determines whether a point is inside a polygon.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="polygonVertices">The vertices that form the polygon.</param>
        /// <returns><see langword="true"/> if <paramref name="point"/> is inside the polygon formed by <paramref name="polygonVertices"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonVertices"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="polygonVertices"/> is less than 3 (cannot form a polygon).</exception>
        bool IsPointInsidePolygon(Vector2 point, IList<Vector2> polygonVertices);
    }
}
