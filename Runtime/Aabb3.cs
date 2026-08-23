using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A three-dimensional axis-aligned bounding box.
    /// </summary>
    [Serializable]
    public struct Aabb3 : IFormattable, IEquatable<Aabb3>
    {
        [SerializeField]
        private float minX;

        [SerializeField]
        private float minY;

        [SerializeField]
        private float minZ;

        [SerializeField]
        private float maxX;

        [SerializeField]
        private float maxY;

        [SerializeField]
        private float maxZ;

        private const string Format = nameof(Aabb3) + "(({0}, {1}, {2}), ({3}, {4}, {5}))";

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb3"/> struct.
        /// </summary>
        /// <param name="x">The x component of the initial point.</param>
        /// <param name="y">The y component of the initial point.</param>
        /// <param name="z">The z component of the initial point.</param>
        public Aabb3(float x, float y, float z)
        {
            minX = x;
            minY = y;
            minZ = z;
            maxX = x;
            maxY = y;
            maxZ = z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb3"/> struct.
        /// </summary>
        /// <param name="point">The initial point.</param>
        public Aabb3(Vector3 point)
        {
            minX = point.x;
            minY = point.y;
            minZ = point.z;
            maxX = point.x;
            maxY = point.y;
            maxZ = point.z;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb3"/> struct.
        /// </summary>
        /// <param name="minX">The x component of the minimum value.</param>
        /// <param name="minY">The y component of the minimum value.</param>
        /// <param name="minZ">The z component of the minimum value.</param>
        /// <param name="maxX">The x component of the maximum value.</param>
        /// <param name="maxY">The y component of the maximum value.</param>
        /// <param name="maxZ">The z component of the maximum value.</param>
        public Aabb3(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb3"/> struct.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public Aabb3(Vector3 min, Vector3 max)
        {
            minX = min.x;
            minY = min.y;
            minZ = min.z;
            maxX = max.x;
            maxY = max.y;
            maxZ = max.z;
        }

        /// <summary>
        /// Gets an <see cref="Aabb3"/> instance with the specified center and size.
        /// </summary>
        /// <param name="centerX">The x component of the center.</param>
        /// <param name="centerY">The y component of the center.</param>
        /// <param name="centerZ">The z component of the center.</param>
        /// <param name="sizeX">The x component of the size.</param>
        /// <param name="sizeY">The y component of the size.</param>
        /// <param name="sizeZ">The z component of the size.</param>
        /// <returns>An <see cref="Aabb3"/> instance with the specified center and size.</returns>
        public static Aabb3 CenterSize(
            float centerX,
            float centerY,
            float centerZ,
            float sizeX,
            float sizeY,
            float sizeZ)
        {
            var extendX = sizeX * 0.5f;
            var extendY = sizeY * 0.5f;
            var extendZ = sizeZ * 0.5f;
            return new Aabb3(
                centerX - extendX,
                centerY - extendY,
                centerZ - extendZ,
                centerX + extendX,
                centerY + extendY,
                centerZ + extendZ
            );
        }

        /// <summary>
        /// Gets an <see cref="Aabb3"/> instance with the specified center and size.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="size">The size.</param>
        /// <returns>An <see cref="Aabb3"/> instance with the specified center and size.</returns>
        public static Aabb3 CenterSize(Vector3 center, Vector3 size)
        {
            var extendX = size.x * 0.5f;
            var extendY = size.y * 0.5f;
            var extendZ = size.z * 0.5f;
            return new Aabb3(
                center.x - extendX,
                center.y - extendY,
                center.z - extendZ,
                center.x + extendX,
                center.y + extendY,
                center.z + extendZ
            );
        }

        /// <summary>
        /// Gets an <see cref="Aabb3"/> instance that contains all the passed-in points.
        /// </summary>
        /// <param name="points">All the points to contain.</param>
        /// <returns>An <see cref="Aabb3"/> instance that contains all the passed-in points.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The element count of <paramref name="points"/> is 0.</exception>
        public static Aabb3 Points(IEnumerable<Vector3> points)
        {
            if (points is null)
            {
                throw new ArgumentNullException(nameof(points));
            }
            using var enumerator = points.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new ArgumentException($"{nameof(points)} has 0 elements");
            }
            var firstPoint = enumerator.Current;
            var minX       = firstPoint.x;
            var minY       = firstPoint.y;
            var minZ       = firstPoint.z;
            var maxX       = firstPoint.x;
            var maxY       = firstPoint.y;
            var maxZ       = firstPoint.z;
            while (enumerator.MoveNext())
            {
                var point = enumerator.Current;
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                minZ = Mathf.Min(minZ, point.z);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
                maxZ = Mathf.Max(maxZ, point.z);
            }
            return new Aabb3(minX, minY, minZ, maxX, maxY, maxZ);
        }

        /// <summary>
        /// Gets or sets the x component of the minimum value.
        /// </summary>
        public float MinX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minX;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minX = value;
        }

        /// <summary>
        /// Gets or sets the y component of the minimum value.
        /// </summary>
        public float MinY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minY;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minY = value;
        }

        /// <summary>
        /// Gets or sets the z component of the minimum value.
        /// </summary>
        public float MinZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minZ;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minZ = value;
        }

        /// <summary>
        /// Gets or sets the x component of the center.
        /// </summary>
        public float CenterX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minX + (maxX - minX) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var delta = value - (minX + (maxX - minX) * 0.5f);
                minX += delta;
                maxX += delta;
            }
        }

        /// <summary>
        /// Gets or sets the y component of the center.
        /// </summary>
        public float CenterY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minY + (maxY - minY) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var delta = value - (minY + (maxY - minY) * 0.5f);
                minY += delta;
                maxY += delta;
            }
        }

        /// <summary>
        /// Gets or sets the z component of the center.
        /// </summary>
        public float CenterZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minZ + (maxZ - minZ) * 0.5f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var delta = value - (minZ + (maxZ - minZ) * 0.5f);
                minZ += delta;
                maxZ += delta;
            }
        }

        /// <summary>
        /// Gets or sets the x component of the maximum value.
        /// </summary>
        public float MaxX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxX;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxX = value;
        }

        /// <summary>
        /// Gets or sets the y component of the maximum value.
        /// </summary>
        public float MaxY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxY;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxY = value;
        }

        /// <summary>
        /// Gets or sets the z component of the maximum value.
        /// </summary>
        public float MaxZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxZ;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxZ = value;
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public Vector3 Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(minX, minY, minZ);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                minX = value.x;
                minY = value.y;
                minZ = value.z;
            }
        }

        /// <summary>
        /// Gets or sets the center.
        /// </summary>
        public Vector3 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f, minZ + (maxZ - minZ) * 0.5f);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = value.x - (minX + (maxX - minX) * 0.5f);
                minX += deltaX;
                maxX += deltaX;
                var deltaY = value.y - (minY + (maxY - minY) * 0.5f);
                minY += deltaY;
                maxY += deltaY;
                var deltaZ = value.z - (minZ + (maxZ - minZ) * 0.5f);
                minZ += deltaZ;
                maxZ += deltaZ;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public Vector3 Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(maxX, maxY, maxZ);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                maxX = value.x;
                maxY = value.y;
                maxZ = value.z;
            }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public Vector3 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(maxX - minX, maxY - minY, maxZ - minZ);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = (value.x - (maxX - minX)) * 0.5f;
                minX -= deltaX;
                maxX += deltaX;
                var deltaY = (value.y - (maxY - minY)) * 0.5f;
                minY -= deltaY;
                maxY += deltaY;
                var deltaZ = (value.z - (maxZ - minZ)) * 0.5f;
                minZ -= deltaZ;
                maxZ += deltaZ;
            }
        }

        /// <summary>
        /// Gets or sets the half-size.
        /// </summary>
        public Vector3 Extends
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new((maxX - minX) * 0.5f, (maxY - minY) * 0.5f, (maxZ - minZ) * 0.5f);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = value.x - (maxX - minX) * 0.5f;
                minX -= deltaX;
                maxX += deltaX;
                var deltaY = value.y - (maxY - minY) * 0.5f;
                minY -= deltaY;
                maxY += deltaY;
                var deltaZ = value.z - (maxZ - minZ) * 0.5f;
                minZ -= deltaZ;
                maxZ += deltaZ;
            }
        }

        /// <summary>
        /// Computes the actual position from a normalized position.
        /// </summary>
        /// <param name="t">The normalized position.</param>
        /// <returns>The actual position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 Lerp(Vector3 t)
        {
            return new Vector3(
                (float)InterpolationUtility.LinearInterpolate(minX, maxX, t.x),
                (float)InterpolationUtility.LinearInterpolate(minY, maxY, t.y),
                (float)InterpolationUtility.LinearInterpolate(minZ, maxZ, t.z)
            );
        }

        /// <summary>
        /// Computes the normalized position from an actual position.
        /// </summary>
        /// <param name="point">The actual position.</param>
        /// <returns>The normalized position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 Unlerp(Vector3 point)
        {
            return new Vector3(
                (float)InterpolationUtility.InverseLinearInterpolate(minX, maxX, point.x),
                (float)InterpolationUtility.InverseLinearInterpolate(minY, maxY, point.y),
                (float)InterpolationUtility.InverseLinearInterpolate(minZ, maxZ, point.z)
            );
        }

        /// <summary>
        /// Includes the specified point (ignoring the z component).
        /// </summary>
        /// <param name="point">The point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Vector2 point)
        {
            var x = point.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            var y = point.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        /// <summary>
        /// Includes the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Vector3 point)
        {
            var x = point.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            var y = point.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
            var z = point.z;
            minZ = Mathf.Min(minZ, z);
            maxZ = Mathf.Max(maxZ, z);
        }

        /// <summary>
        /// Includes the specified another 2D axis-aligned bounding box (without modifying the z component).
        /// </summary>
        /// <param name="other">Another 2D axis-aligned bounding box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Aabb2 other)
        {
            minX = Mathf.Min(minX, other.MinX);
            minY = Mathf.Min(minY, other.MinY);
            maxX = Mathf.Max(maxX, other.MaxX);
            maxY = Mathf.Max(maxY, other.MaxY);
        }

        /// <summary>
        /// Includes the specified another 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 3D axis-aligned bounding box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Aabb3 other)
        {
            minX = Mathf.Min(minX, other.minX);
            minY = Mathf.Min(minY, other.minY);
            minZ = Mathf.Min(minZ, other.minZ);
            maxX = Mathf.Max(maxX, other.maxX);
            maxY = Mathf.Max(maxY, other.maxY);
            maxZ = Mathf.Max(maxZ, other.maxZ);
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains the specified point (ignoring the z component).
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="point"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector2 point)
        {
            return minX <= point.x && minY <= point.y && maxX > point.x && maxY > point.y;
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="point"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector3 point)
        {
            return minX <= point.x && minY <= point.y && minZ <= point.z && maxX > point.x && maxY > point.y &&
                   maxZ > point.z;
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains the specified 2D axis-aligned bounding box (ignoring the z component).
        /// </summary>
        /// <param name="aabb2">The 2D axis-aligned bounding box.</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="aabb2"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb2 aabb2)
        {
            return minX <= aabb2.MinX && minY <= aabb2.MinY && maxX >= aabb2.MaxX && maxY >= aabb2.MaxY;
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains another 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 3D axis-aligned bounding box.</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="other"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb3 other)
        {
            return minX <= other.minX && minY <= other.minY && minZ <= other.minZ && maxX >= other.maxX &&
                   maxY >= other.maxY && maxZ >= other.maxZ;
        }

        /// <summary>
        /// Returns a value indicating whether this instance overlaps the specified 2D axis-aligned bounding box (ignoring the z component).
        /// </summary>
        /// <param name="aabb2">The 2D axis-aligned bounding box.</param>
        /// <returns><see langword="true"/> if this instance overlaps <paramref name="aabb2"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Aabb2 aabb2)
        {
            return minX <= aabb2.MaxX && minY <= aabb2.MaxY && maxX >= aabb2.MinX && maxY >= aabb2.MinY;
        }

        /// <summary>
        /// Returns a value indicating whether this instance overlaps another 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 3D axis-aligned bounding box.</param>
        /// <returns><see langword="true"/> if this instance overlaps <paramref name="other"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Aabb3 other)
        {
            return minX <= other.maxX && minY <= other.maxY && minZ <= other.maxZ && maxX >= other.minX &&
                   maxY >= other.minY && maxZ >= other.minZ;
        }

        /// <inheritdoc />
        public readonly bool Equals(Aabb3 other)
        {
            return minX.Equals(other.minX) && minY.Equals(other.minY) && minZ.Equals(other.minZ) &&
                   maxX.Equals(other.maxX) && maxY.Equals(other.maxY) && maxZ.Equals(other.maxZ);
        }

        /// <inheritdoc />
        public readonly override bool Equals(object obj)
        {
            return obj is Aabb3 other && Equals(other);
        }

        /// <inheritdoc />
        public readonly override string ToString()
        {
            return string.Format(Format, minX, minY, minZ, maxX, maxY, maxZ);
        }

        /// <inheritdoc />
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                Format,
                minX.ToString(format, formatProvider),
                minY.ToString(format, formatProvider),
                minZ.ToString(format, formatProvider),
                maxX.ToString(format, formatProvider),
                maxY.ToString(format, formatProvider),
                maxZ.ToString(format, formatProvider)
            );
        }

        /// <inheritdoc />
        public readonly override int GetHashCode()
        {
            unchecked
            {
                var hashCode = minX.GetHashCode();
                hashCode = (hashCode * 397) ^ minY.GetHashCode();
                hashCode = (hashCode * 397) ^ minZ.GetHashCode();
                hashCode = (hashCode * 397) ^ maxX.GetHashCode();
                hashCode = (hashCode * 397) ^ maxY.GetHashCode();
                hashCode = (hashCode * 397) ^ maxZ.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value indicating whether two specified <see cref="Aabb3"/> values are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(Aabb3 left, Aabb3 right)
        {
            return left.minX == right.minX && left.minY == right.minY && left.minZ == right.minZ &&
                   left.maxX == right.maxX && left.maxY == right.maxY && left.maxZ == right.maxZ;
        }

        /// <summary>
        /// Returns a value indicating whether two specified <see cref="Aabb3"/> values are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Aabb3 left, Aabb3 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Converts a 3D axis-aligned bounding box to a Unity 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="aabb3">The 3D axis-aligned bounding box.</param>
        /// <returns>The Unity 3D axis-aligned bounding box converted from <paramref name="aabb3"/>.</returns>
        public static explicit operator Bounds(Aabb3 aabb3)
        {
            return new Bounds(aabb3.Center, aabb3.Size);
        }

        /// <summary>
        /// Converts a Unity 3D axis-aligned bounding box to a 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="bounds">The Unity 3D axis-aligned bounding box.</param>
        /// <returns>The 3D axis-aligned bounding box converted from <paramref name="bounds"/>.</returns>
        public static explicit operator Aabb3(Bounds bounds)
        {
            return CenterSize(bounds.center, bounds.size);
        }
    }
}
