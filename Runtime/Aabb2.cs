using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A two-dimensional axis-aligned bounding box.
    /// </summary>
    [Serializable]
    public struct Aabb2 : IFormattable, IEquatable<Aabb2>
    {
        [SerializeField]
        private float minX;

        [SerializeField]
        private float minY;

        [SerializeField]
        private float maxX;

        [SerializeField]
        private float maxY;

        private const string Format = nameof(Aabb2) + "(({0}, {1}), ({2}, {3}))";

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb2"/> struct.
        /// </summary>
        /// <param name="x">The x component of the initial point.</param>
        /// <param name="y">The y component of the initial point.</param>
        public Aabb2(float x, float y)
        {
            minX = x;
            minY = y;
            maxX = x;
            maxY = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb2"/> struct.
        /// </summary>
        /// <param name="point">The initial point.</param>
        public Aabb2(Vector2 point)
        {
            minX = point.x;
            minY = point.y;
            maxX = point.x;
            maxY = point.y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb2"/> struct.
        /// </summary>
        /// <param name="minX">The x component of the minimum value.</param>
        /// <param name="minY">The y component of the minimum value.</param>
        /// <param name="maxX">The x component of the maximum value.</param>
        /// <param name="maxY">The y component of the maximum value.</param>
        public Aabb2(float minX, float minY, float maxX, float maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aabb2"/> struct.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public Aabb2(Vector2 min, Vector2 max)
        {
            minX = min.x;
            minY = min.y;
            maxX = max.x;
            maxY = max.y;
        }

        /// <summary>
        /// Gets an <see cref="Aabb2"/> instance with the specified center and size.
        /// </summary>
        /// <param name="centerX">The x component of the center.</param>
        /// <param name="centerY">The y component of the center.</param>
        /// <param name="sizeX">The x component of the size.</param>
        /// <param name="sizeY">The y component of the size.</param>
        /// <returns>An <see cref="Aabb2"/> instance with the specified center and size.</returns>
        public static Aabb2 CenterSize(float centerX, float centerY, float sizeX, float sizeY)
        {
            var extendX = sizeX * 0.5f;
            var extendY = sizeY * 0.5f;
            return new Aabb2(centerX - extendX, centerY - extendY, centerX + extendX, centerY + extendY);
        }

        /// <summary>
        /// Gets an <see cref="Aabb2"/> instance with the specified center and size.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="size">The size.</param>
        /// <returns>An <see cref="Aabb2"/> instance with the specified center and size.</returns>
        public static Aabb2 CenterSize(Vector2 center, Vector2 size)
        {
            var extendX = size.x * 0.5f;
            var extendY = size.y * 0.5f;
            return new Aabb2(center.x - extendX, center.y - extendY, center.x + extendX, center.y + extendY);
        }

        /// <summary>
        /// Gets an <see cref="Aabb2"/> instance that contains all the passed-in points.
        /// </summary>
        /// <param name="points">All the points to contain.</param>
        /// <returns>An <see cref="Aabb2"/> instance that contains all the passed-in points.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The element count of <paramref name="points"/> is 0.</exception>
        public static Aabb2 Points(IEnumerable<Vector2> points)
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
            var maxX       = firstPoint.x;
            var maxY       = firstPoint.y;
            while (enumerator.MoveNext())
            {
                var point = enumerator.Current;
                minX = Mathf.Min(minX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxX = Mathf.Max(maxX, point.x);
                maxY = Mathf.Max(maxY, point.y);
            }
            return new Aabb2(minX, minY, maxX, maxY);
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
        /// Gets or sets the minimum value.
        /// </summary>
        public Vector2 Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(minX, minY);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                minX = value.x;
                minY = value.y;
            }
        }

        /// <summary>
        /// Gets or sets the center.
        /// </summary>
        public Vector2 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = value.x - (minX + (maxX - minX) * 0.5f);
                minX += deltaX;
                maxX += deltaX;
                var deltaY = value.y - (minY + (maxY - minY) * 0.5f);
                minY += deltaY;
                maxY += deltaY;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public Vector2 Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(maxX, maxY);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                maxX = value.x;
                maxY = value.y;
            }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public Vector2 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new(maxX - minX, maxY - minY);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = (value.x - (maxX - minX)) * 0.5f;
                minX -= deltaX;
                maxX += deltaX;
                var deltaY = (value.y - (maxY - minY)) * 0.5f;
                minY -= deltaY;
                maxY += deltaY;
            }
        }

        /// <summary>
        /// Gets or sets the half-size.
        /// </summary>
        public Vector2 Extends
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => new((maxX - minX) * 0.5f, (maxY - minY) * 0.5f);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var deltaX = value.x - (maxX - minX) * 0.5f;
                minX -= deltaX;
                maxX += deltaX;
                var deltaY = value.y - (maxY - minY) * 0.5f;
                minY -= deltaY;
                maxY += deltaY;
            }
        }

        /// <summary>
        /// Computes the actual position from a normalized position.
        /// </summary>
        /// <param name="t">The normalized position.</param>
        /// <returns>The actual position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Lerp(Vector2 t)
        {
            return new Vector2(
                (float)InterpolationUtility.LinearInterpolate(minX, maxX, t.x),
                (float)InterpolationUtility.LinearInterpolate(minY, maxY, t.y)
            );
        }

        /// <summary>
        /// Computes the normalized position from an actual position.
        /// </summary>
        /// <param name="point">The actual position.</param>
        /// <returns>The normalized position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Unlerp(Vector2 point)
        {
            return new Vector2(
                (float)InterpolationUtility.InverseLinearInterpolate(minX, maxX, point.x),
                (float)InterpolationUtility.InverseLinearInterpolate(minY, maxY, point.y)
            );
        }

        /// <summary>
        /// Includes the specified point.
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
        /// Includes the specified another 2D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 2D axis-aligned bounding box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Aabb2 other)
        {
            minX = Mathf.Min(minX, other.minX);
            minY = Mathf.Min(minY, other.minY);
            maxX = Mathf.Max(maxX, other.maxX);
            maxY = Mathf.Max(maxY, other.maxY);
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="point"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector2 point)
        {
            return minX <= point.x && minY <= point.y && maxX > point.x && maxY > point.y;
        }

        /// <summary>
        /// Returns a value indicating whether this instance contains another 2D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 2D axis-aligned bounding box</param>
        /// <returns><see langword="true"/> if this instance contains <paramref name="other"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb2 other)
        {
            return minX <= other.minX && minY <= other.minY && maxX >= other.maxX && maxY >= other.maxY;
        }

        /// <summary>
        /// Returns a value indicating whether this instance overlaps another 2D axis-aligned bounding box.
        /// </summary>
        /// <param name="other">Another 2D axis-aligned bounding box</param>
        /// <returns><see langword="true"/> if this instance overlaps <paramref name="other"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Aabb2 other)
        {
            return minX <= other.maxX && minY <= other.maxY && maxX >= other.minX && maxY >= other.minY;
        }

        /// <inheritdoc />
        public readonly bool Equals(Aabb2 other)
        {
            return minX.Equals(other.minX) && minY.Equals(other.minY) && maxX.Equals(other.maxX) &&
                   maxY.Equals(other.maxY);
        }

        /// <inheritdoc />
        public readonly override bool Equals(object obj)
        {
            return obj is Aabb2 other && Equals(other);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(Format, minX, minY, maxX, maxY);
        }

        /// <inheritdoc />
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                Format,
                minX.ToString(format, formatProvider),
                minY.ToString(format, formatProvider),
                maxX.ToString(format, formatProvider),
                maxY.ToString(format, formatProvider)
            );
        }

        /// <inheritdoc />
        public readonly override int GetHashCode()
        {
            unchecked
            {
                var hashCode = minX.GetHashCode();
                hashCode = (hashCode * 397) ^ minY.GetHashCode();
                hashCode = (hashCode * 397) ^ maxX.GetHashCode();
                hashCode = (hashCode * 397) ^ maxY.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value indicating whether two specified <see cref="Aabb2"/> values are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(Aabb2 left, Aabb2 right)
        {
            return left.minX == right.minX && left.minY == right.minY && left.maxX == right.maxX &&
                   left.maxY == right.maxY;
        }

        /// <summary>
        /// Returns a value indicating whether two specified <see cref="Aabb2"/> values are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Aabb2 left, Aabb2 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Converts a 2D axis-aligned bounding box to a rectangle.
        /// </summary>
        /// <param name="aabb2">The 2D axis-aligned bounding box.</param>
        /// <returns>The rectangle converted from <paramref name="aabb2"/>.</returns>
        public static explicit operator Rect(Aabb2 aabb2)
        {
            return Rect.MinMaxRect(aabb2.minX, aabb2.minY, aabb2.maxX, aabb2.maxY);
        }

        /// <summary>
        /// Converts a rectangle to a 2D axis-aligned bounding box.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>The 2D axis-aligned bounding box converted from <paramref name="rect"/>.</returns>
        public static explicit operator Aabb2(Rect rect)
        {
            return new Aabb2(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
        }

        /// <summary>
        /// Converts a 2D axis-aligned bounding box to a 3D axis-aligned bounding box.
        /// </summary>
        /// <param name="aabb2">The 2D axis-aligned bounding box.</param>
        /// <returns>The 3D axis-aligned bounding box converted from <paramref name="aabb2"/>.</returns>
        public static implicit operator Aabb3(Aabb2 aabb2)
        {
            return new Aabb3(aabb2.minX, aabb2.minY, 0, aabb2.maxX, aabb2.maxY, 0);
        }

        /// <summary>
        /// Converts a 3D axis-aligned bounding box to a 2D axis-aligned bounding box.
        /// </summary>
        /// <param name="aabb3">The 3D axis-aligned bounding box.</param>
        /// <returns>The 2D axis-aligned bounding box converted from <paramref name="aabb3"/>.</returns>
        public static implicit operator Aabb2(Aabb3 aabb3)
        {
            return new Aabb2(aabb3.MinX, aabb3.MinY, aabb3.MaxX, aabb3.MaxY);
        }
    }
}
