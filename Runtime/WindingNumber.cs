using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Determines whether a point is inside a polygon by computing how many times each edge of the polygon winds around the point.
    /// </summary>
    public struct WindingNumber : IInclusionOfAPointInAPolygonAlgorithm
    {
        bool IInclusionOfAPointInAPolygonAlgorithm.IsPointInsidePolygon(Vector2 point, IList<Vector2> polygonVertices)
        {
            if (polygonVertices is null)
            {
                throw new ArgumentNullException(nameof(polygonVertices));
            }
            var count = polygonVertices.Count;
            if (count < 3)
            {
                throw new ArgumentException(null, nameof(polygonVertices));
            }
            var windingNumber = 0;
            for (var i = 0; i < count - 1; i++)
            {
                windingNumber += A(point, polygonVertices[i], polygonVertices[i + 1]);
            }
            windingNumber += A(point, polygonVertices[count - 1], polygonVertices[0]);
            return windingNumber != 0;
        }

        private static sbyte A(Vector2 point, Vector2 polygonVertex0, Vector2 polygonVertex1)
        {
            if (polygonVertex0.y <= point.y)
            {
                if (polygonVertex1.y > point.y)
                {
                    if (CrossZ(polygonVertex1 - polygonVertex0, point - polygonVertex0) > 0)
                    {
                        return 1;
                    }
                }
            }
            else
            {
                if (polygonVertex1.y <= point.y)
                {
                    if (CrossZ(polygonVertex1 - polygonVertex0, point - polygonVertex0) < 0)
                    {
                        return -1;
                    }
                }
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CrossZ(Vector2 left, Vector2 right)
        {
            return left.x * right.y - left.y * right.x;
        }
    }
}
