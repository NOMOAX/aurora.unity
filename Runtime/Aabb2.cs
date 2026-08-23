using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 二维轴向包围盒（two-dimensional axis-aligned bounding box）。
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
        /// 初始化 <see cref="Aabb2"/> 结构的新实例。
        /// </summary>
        /// <param name="x">初始点的 x 分量。</param>
        /// <param name="y">初始点的 y 分量。</param>
        public Aabb2(float x, float y)
        {
            minX = x;
            minY = y;
            maxX = x;
            maxY = y;
        }

        /// <summary>
        /// 初始化 <see cref="Aabb2"/> 结构的新实例。
        /// </summary>
        /// <param name="point">初始点。</param>
        public Aabb2(Vector2 point)
        {
            minX = point.x;
            minY = point.y;
            maxX = point.x;
            maxY = point.y;
        }

        /// <summary>
        /// 初始化 <see cref="Aabb2"/> 结构的新实例。
        /// </summary>
        /// <param name="minX">最小值的 x 分量。</param>
        /// <param name="minY">最小值的 y 分量。</param>
        /// <param name="maxX">最大值的 x 分量。</param>
        /// <param name="maxY">最大值的 y 分量。</param>
        public Aabb2(float minX, float minY, float maxX, float maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        /// <summary>
        /// 初始化 <see cref="Aabb2"/> 结构的新实例。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        public Aabb2(Vector2 min, Vector2 max)
        {
            minX = min.x;
            minY = min.y;
            maxX = max.x;
            maxY = max.y;
        }

        /// <summary>
        /// 获取具有指定的中心和大小的 <see cref="Aabb2"/> 实例。
        /// </summary>
        /// <param name="centerX">中心的 x 分量。</param>
        /// <param name="centerY">中心的 y 分量。</param>
        /// <param name="sizeX">大小的 x 分量。</param>
        /// <param name="sizeY">大小的 y 分量。</param>
        /// <returns>具有指定的中心和大小的 <see cref="Aabb2"/> 实例。</returns>
        public static Aabb2 CenterSize(float centerX, float centerY, float sizeX, float sizeY)
        {
            var extendX = sizeX * 0.5f;
            var extendY = sizeY * 0.5f;
            return new Aabb2(centerX - extendX, centerY - extendY, centerX + extendX, centerY + extendY);
        }

        /// <summary>
        /// 获取具有指定的中心和大小的 <see cref="Aabb2"/> 实例。
        /// </summary>
        /// <param name="center">中心。</param>
        /// <param name="size">大小。</param>
        /// <returns>具有指定的中心和大小的 <see cref="Aabb2"/> 实例。</returns>
        public static Aabb2 CenterSize(Vector2 center, Vector2 size)
        {
            var extendX = size.x * 0.5f;
            var extendY = size.y * 0.5f;
            return new Aabb2(center.x - extendX, center.y - extendY, center.x + extendX, center.y + extendY);
        }

        /// <summary>
        /// 获取包含了传入的所有点的 <see cref="Aabb2"/> 实例。
        /// </summary>
        /// <param name="points">要包含的所有点。</param>
        /// <returns>包含了传入的所有点的 <see cref="Aabb2"/> 实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="points"/> 中的元素数为 0。</exception>
        public static Aabb2 Points(IEnumerable<Vector2> points)
        {
            if (points is null)
            {
                throw new ArgumentNullException(nameof(points));
            }
            using var enumerator = points.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new ArgumentException($"{nameof(points)} 中的元素数为 0");
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
        /// 获取或设置最小值的 x 分量。
        /// </summary>
        public float MinX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minX;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minX = value;
        }

        /// <summary>
        /// 获取或设置最小值的 y 分量。
        /// </summary>
        public float MinY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minY;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minY = value;
        }

        /// <summary>
        /// 获取或设置中心的 x 分量。
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
        /// 获取或设置中心的 y 分量。
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
        /// 获取或设置最大值的 x 分量。
        /// </summary>
        public float MaxX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxX;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxX = value;
        }

        /// <summary>
        /// 获取或设置最大值的 y 分量。
        /// </summary>
        public float MaxY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxY;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxY = value;
        }

        /// <summary>
        /// 获取或设置最小值。
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
        /// 获取或设置中心。
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
        /// 获取或设置最大值。
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
        /// 获取或设置大小。
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
        /// 获取或设置一半大小。
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
        /// 由标准化位置计算实际位置。
        /// </summary>
        /// <param name="t">标准化位置。</param>
        /// <returns>实际位置。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Lerp(Vector2 t)
        {
            return new Vector2(
                (float)InterpolationUtility.LinearInterpolate(minX, maxX, t.x),
                (float)InterpolationUtility.LinearInterpolate(minY, maxY, t.y)
            );
        }

        /// <summary>
        /// 由实际位置计算标准化位置。
        /// </summary>
        /// <param name="point">实际位置。</param>
        /// <returns>标准化位置。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector2 Unlerp(Vector2 point)
        {
            return new Vector2(
                (float)InterpolationUtility.InverseLinearInterpolate(minX, maxX, point.x),
                (float)InterpolationUtility.InverseLinearInterpolate(minY, maxY, point.y)
            );
        }

        /// <summary>
        /// 包含指定点。
        /// </summary>
        /// <param name="point">点。</param>
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
        /// 包含指定的另一个二维轴向包围盒。
        /// </summary>
        /// <param name="other">另一个二维轴向包围盒。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Aabb2 other)
        {
            minX = Mathf.Min(minX, other.minX);
            minY = Mathf.Min(minY, other.minY);
            maxX = Mathf.Max(maxX, other.maxX);
            maxY = Mathf.Max(maxY, other.maxY);
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否包含指定点。
        /// </summary>
        /// <param name="point">点。</param>
        /// <returns>如果此实例包含 <paramref name="point"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector2 point)
        {
            return minX <= point.x && minY <= point.y && maxX > point.x && maxY > point.y;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否包含另一个二维轴向包围盒。
        /// </summary>
        /// <param name="other">另一个二维轴向包围盒</param>
        /// <returns>如果此实例包含 <paramref name="other"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb2 other)
        {
            return minX <= other.minX && minY <= other.minY && maxX >= other.maxX && maxY >= other.maxY;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否与另一个二维轴向包围盒有重叠。
        /// </summary>
        /// <param name="other">另一个二维轴向包围盒</param>
        /// <returns>如果此实例与 <paramref name="other"/> 有重叠，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
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
        /// 返回一个值，该值指示两个指定的 <see cref="Aabb2"/> 值是否相等。
        /// </summary>
        /// <param name="left">要比较的第一个值。</param>
        /// <param name="right">要比较的第二个值。</param>
        /// <returns>如果 <paramref name="left"/> 和 <paramref name="right"/> 相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool operator ==(Aabb2 left, Aabb2 right)
        {
            return left.minX == right.minX && left.minY == right.minY && left.maxX == right.maxX &&
                   left.maxY == right.maxY;
        }

        /// <summary>
        /// 返回一个值，该值指示两个指定的 <see cref="Aabb2"/> 值是否相等。
        /// </summary>
        /// <param name="left">要比较的第一个值。</param>
        /// <param name="right">要比较的第二个值。</param>
        /// <returns>如果 <paramref name="left"/> 和 <paramref name="right"/> 不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool operator !=(Aabb2 left, Aabb2 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 将二维轴向包围盒转换为矩形。
        /// </summary>
        /// <param name="aabb2">二维轴向包围盒。</param>
        /// <returns>由 <paramref name="aabb2"/> 转换得到的矩形。</returns>
        public static explicit operator Rect(Aabb2 aabb2)
        {
            return Rect.MinMaxRect(aabb2.minX, aabb2.minY, aabb2.maxX, aabb2.maxY);
        }

        /// <summary>
        /// 将矩形转换为二维轴向包围盒。
        /// </summary>
        /// <param name="rect">矩形。</param>
        /// <returns>由 <paramref name="rect"/> 转换得到的二维轴向包围盒。</returns>
        public static explicit operator Aabb2(Rect rect)
        {
            return new Aabb2(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
        }

        /// <summary>
        /// 将二维轴向包围盒转换为三维轴向包围盒。
        /// </summary>
        /// <param name="aabb2">二维轴向包围盒。</param>
        /// <returns>由 <paramref name="aabb2"/> 转换得到的三维轴向包围盒。</returns>
        public static implicit operator Aabb3(Aabb2 aabb2)
        {
            return new Aabb3(aabb2.minX, aabb2.minY, 0, aabb2.maxX, aabb2.maxY, 0);
        }

        /// <summary>
        /// 将三维轴向包围盒转换为二维轴向包围盒。
        /// </summary>
        /// <param name="aabb3">三维轴向包围盒。</param>
        /// <returns>由 <paramref name="aabb3"/> 转换得到的二维轴向包围盒。</returns>
        public static implicit operator Aabb2(Aabb3 aabb3)
        {
            return new Aabb2(aabb3.MinX, aabb3.MinY, aabb3.MaxX, aabb3.MaxY);
        }
    }
}
