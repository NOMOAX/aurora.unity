using System;
using UnityEngine;

namespace Aurora.Unity.Collections
{
    internal static class QuadtreeHelper
    {
        internal static Aabb2 GetChildAabb2(Aabb2 aabb2, int childIndex)
        {
            return childIndex switch
            {
                0 => new Aabb2(aabb2.MinX,    aabb2.MinY,    aabb2.CenterX, aabb2.CenterY),
                1 => new Aabb2(aabb2.CenterX, aabb2.MinY,    aabb2.MaxX,    aabb2.CenterY),
                2 => new Aabb2(aabb2.MinX,    aabb2.CenterY, aabb2.CenterX, aabb2.MaxY),
                3 => new Aabb2(aabb2.CenterX, aabb2.CenterY, aabb2.MaxX,    aabb2.MaxY),
                _ => throw new ArgumentOutOfRangeException(nameof(childIndex), childIndex, null)
            };
        }

        internal static float GetSquareDistance(Aabb2 aabb2, Vector2 point)
        {
            var nearestPoint = GetNearestPoint(aabb2, point);
            var vector       = nearestPoint - point;
            return vector.sqrMagnitude;
        }

        private static Vector2 GetNearestPoint(Aabb2 aabb2, Vector2 point)
        {
            return new Vector2(
                Mathf.Clamp(aabb2.MinX, aabb2.MaxX, point.x),
                Mathf.Clamp(aabb2.MinY, aabb2.MaxY, point.y)
            );
        }
    }
}
