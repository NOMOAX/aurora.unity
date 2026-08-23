using System;
using UnityEngine;

namespace Aurora.Unity.Collections
{
    internal static class OctreeHelper
    {
        internal static Aabb3 GetChildAabb3(Aabb3 aabb3, int childIndex)
        {
            return childIndex switch
            {
                0 => new Aabb3(aabb3.MinX,    aabb3.MinY,    aabb3.MinZ, aabb3.CenterX, aabb3.CenterY, aabb3.CenterZ),
                1 => new Aabb3(aabb3.CenterX, aabb3.MinY,    aabb3.MinZ, aabb3.MaxX, aabb3.CenterY, aabb3.CenterZ),
                2 => new Aabb3(aabb3.MinX,    aabb3.CenterY, aabb3.MinZ, aabb3.CenterX, aabb3.MaxY, aabb3.CenterZ),
                3 => new Aabb3(aabb3.CenterX, aabb3.CenterY, aabb3.MinZ, aabb3.MaxX, aabb3.MaxY, aabb3.CenterZ),
                4 => new Aabb3(aabb3.MinX,    aabb3.MinY,    aabb3.CenterZ, aabb3.CenterX, aabb3.CenterY, aabb3.MaxZ),
                5 => new Aabb3(aabb3.CenterX, aabb3.MinY,    aabb3.CenterZ, aabb3.MaxX, aabb3.CenterY, aabb3.MaxZ),
                6 => new Aabb3(aabb3.MinX,    aabb3.CenterY, aabb3.CenterZ, aabb3.CenterX, aabb3.MaxY, aabb3.MaxZ),
                7 => new Aabb3(aabb3.CenterX, aabb3.CenterY, aabb3.CenterZ, aabb3.MaxX, aabb3.MaxY, aabb3.MaxZ),
                _ => throw new ArgumentOutOfRangeException(nameof(childIndex), childIndex, null)
            };
        }

        internal static float GetSquareDistance(Aabb3 aabb3, Vector3 point)
        {
            var nearestPoint = GetNearestPoint(aabb3, point);
            var vector       = nearestPoint - point;
            return vector.sqrMagnitude;
        }

        private static Vector3 GetNearestPoint(Aabb3 aabb3, Vector3 point)
        {
            return new Vector3(
                Mathf.Clamp(aabb3.MinX, aabb3.MaxX, point.x),
                Mathf.Clamp(aabb3.MinY, aabb3.MaxY, point.y),
                Mathf.Clamp(aabb3.MinZ, aabb3.MaxZ, point.z)
            );
        }
    }
}
