using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 三维轴向包围盒（three-dimensional axis-aligned bounding box）。
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

        private const string Format = nameof(Aabb3) + "(({0}f, {1}f, {2}f), ({3}f, {4}f, {5}f))";

        /// <summary>
        /// 初始化 <see cref="Aabb3"/> 结构的新实例。
        /// </summary>
        /// <param name="x">初始点的 x 分量。</param>
        /// <param name="y">初始点的 y 分量。</param>
        /// <param name="z">初始点的 z 分量。</param>
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
        /// 初始化 <see cref="Aabb3"/> 结构的新实例。
        /// </summary>
        /// <param name="point">初始点。</param>
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
        /// 初始化 <see cref="Aabb3"/> 结构的新实例。
        /// </summary>
        /// <param name="minX">最小值的 x 分量。</param>
        /// <param name="minY">最小值的 y 分量。</param>
        /// <param name="minZ">最小值的 z 分量。</param>
        /// <param name="maxX">最大值的 x 分量。</param>
        /// <param name="maxY">最大值的 y 分量。</param>
        /// <param name="maxZ">最大值的 z 分量。</param>
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
        /// 初始化 <see cref="Aabb3"/> 结构的新实例。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
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
        /// 获取具有指定的中心和大小的 <see cref="Aabb3"/> 实例。
        /// </summary>
        /// <param name="centerX">中心的 x 分量。</param>
        /// <param name="centerY">中心的 y 分量。</param>
        /// <param name="centerZ">中心的 z 分量。</param>
        /// <param name="sizeX">大小的 x 分量。</param>
        /// <param name="sizeY">大小的 y 分量。</param>
        /// <param name="sizeZ">大小的 z 分量。</param>
        /// <returns>具有指定的中心和大小的 <see cref="Aabb3"/> 实例。</returns>
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
        /// 获取具有指定的中心和大小的 <see cref="Aabb3"/> 实例。
        /// </summary>
        /// <param name="center">中心。</param>
        /// <param name="size">大小。</param>
        /// <returns>具有指定的中心和大小的 <see cref="Aabb3"/> 实例。</returns>
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
        /// 获取包含了传入的所有点的 <see cref="Aabb3"/> 实例。
        /// </summary>
        /// <param name="points">要包含的所有点。</param>
        /// <returns>包含了传入的所有点的 <see cref="Aabb3"/> 实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="points"/> 中的元素数为 0。</exception>
        public static Aabb3 Points(IEnumerable<Vector3> points)
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
        /// 获取或设置最小值的 z 分量。
        /// </summary>
        public float MinZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => minZ;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => minZ = value;
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
        /// 获取或设置中心的 z 分量。
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
        /// 获取或设置最大值的 z 分量。
        /// </summary>
        public float MaxZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => maxZ;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => maxZ = value;
        }

        /// <summary>
        /// 获取或设置最小值。
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
        /// 获取或设置中心。
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
        /// 获取或设置最大值。
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
        /// 获取或设置大小。
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
        /// 获取或设置一半大小。
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
        /// 由标准化位置计算实际位置。
        /// </summary>
        /// <param name="t">标准化位置。</param>
        /// <returns>实际位置。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 Lerp(Vector3 t)
        {
            return new Vector3(
                (float) InterpolationUtility.LinearInterpolate(minX, maxX, t.x),
                (float) InterpolationUtility.LinearInterpolate(minY, maxY, t.y),
                (float) InterpolationUtility.LinearInterpolate(minZ, maxZ, t.z)
            );
        }

        /// <summary>
        /// 由实际位置计算标准化位置。
        /// </summary>
        /// <param name="point">实际位置。</param>
        /// <returns>标准化位置。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 Unlerp(Vector3 point)
        {
            return new Vector3(
                (float) InterpolationUtility.InverseLinearInterpolate(minX, maxX, point.x),
                (float) InterpolationUtility.InverseLinearInterpolate(minY, maxY, point.y),
                (float) InterpolationUtility.InverseLinearInterpolate(minZ, maxZ, point.z)
            );
        }

        /// <summary>
        /// 包含指定点（不考虑 z 分量）。
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
        /// 包含指定点。
        /// </summary>
        /// <param name="point">点。</param>
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
        /// 包含指定的另一个二维轴向包围盒（不修改 z 分量）。
        /// </summary>
        /// <param name="other">另一个二维轴向包围盒。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Aabb2 other)
        {
            minX = Mathf.Min(minX, other.MinX);
            minY = Mathf.Min(minY, other.MinY);
            maxX = Mathf.Max(maxX, other.MaxX);
            maxY = Mathf.Max(maxY, other.MaxY);
        }

        /// <summary>
        /// 包含指定的另一个三维轴向包围盒。
        /// </summary>
        /// <param name="other">另一个三维轴向包围盒。</param>
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
        /// 返回一个值，该值指示此实例是否包含指定点（不考虑 z 分量）。
        /// </summary>
        /// <param name="point">点。</param>
        /// <returns>如果此实例包含 <paramref name="point"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector2 point)
        {
            return minX <= point.x && minY <= point.y && maxX > point.x && maxY > point.y;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否包含指定点。
        /// </summary>
        /// <param name="point">点。</param>
        /// <returns>如果此实例包含 <paramref name="point"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Vector3 point)
        {
            return minX <= point.x && minY <= point.y && minZ <= point.z && maxX > point.x && maxY > point.y &&
                   maxZ > point.z;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否包含指定二维轴向包围盒（不考虑 z 分量）。
        /// </summary>
        /// <param name="aabb2">二维轴向包围盒。</param>
        /// <returns>如果此实例包含 <paramref name="aabb2"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb2 aabb2)
        {
            return minX <= aabb2.MinX && minY <= aabb2.MinY && maxX >= aabb2.MaxX && maxY >= aabb2.MaxY;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否包含另一个三维轴向包围盒。
        /// </summary>
        /// <param name="other">另一个三维轴向包围盒。</param>
        /// <returns>如果此实例包含 <paramref name="other"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(Aabb3 other)
        {
            return minX <= other.minX && minY <= other.minY && minZ <= other.minZ && maxX >= other.maxX &&
                   maxY >= other.maxY && maxZ >= other.maxZ;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否与指定二维轴向包围盒有重叠（不考虑 z 分量）。
        /// </summary>
        /// <param name="aabb2">二维轴向包围盒。</param>
        /// <returns>如果此实例与 <paramref name="aabb2"/> 有重叠，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Overlaps(Aabb2 aabb2)
        {
            return minX <= aabb2.MaxX && minY <= aabb2.MaxY && maxX >= aabb2.MinX && maxY >= aabb2.MinY;
        }

        /// <summary>
        /// 返回一个值，该值指示此实例是否与另一个三维轴向包围盒有重叠。
        /// </summary>
        /// <param name="other">另一个三维轴向包围盒。</param>
        /// <returns>如果此实例与 <paramref name="other"/> 有重叠，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
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
        /// 返回一个值，该值指示两个指定的 <see cref="Aabb3"/> 值是否相等。
        /// </summary>
        /// <param name="left">要比较的第一个值。</param>
        /// <param name="right">要比较的第二个值。</param>
        /// <returns>如果 <paramref name="left"/> 和 <paramref name="right"/> 相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool operator ==(Aabb3 left, Aabb3 right)
        {
            return left.minX == right.minX && left.minY == right.minY && left.minZ == right.minZ &&
                   left.maxX == right.maxX && left.maxY == right.maxY && left.maxZ == right.maxZ;
        }

        /// <summary>
        /// 返回一个值，该值指示两个指定的 <see cref="Aabb3"/> 值是否相等。
        /// </summary>
        /// <param name="left">要比较的第一个值。</param>
        /// <param name="right">要比较的第二个值。</param>
        /// <returns>如果 <paramref name="left"/> 和 <paramref name="right"/> 不相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool operator !=(Aabb3 left, Aabb3 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 将三位轴向包围盒转换为 Unity 三维轴向包围盒。
        /// </summary>
        /// <param name="aabb3">三维轴向包围盒。</param>
        /// <returns>由 <paramref name="aabb3"/> 转换得到的 Unity 三维轴向包围盒。</returns>
        public static explicit operator Bounds(Aabb3 aabb3)
        {
            return new Bounds(aabb3.Center, aabb3.Size);
        }

        /// <summary>
        /// 将 Unity 三维轴向包围盒转换为三位轴向包围盒。
        /// </summary>
        /// <param name="bounds">Unity 三维轴向包围盒。</param>
        /// <returns>由 <paramref name="bounds"/> 转换得到的三维轴向包围盒。</returns>
        public static explicit operator Aabb3(Bounds bounds)
        {
            return CenterSize(bounds.center, bounds.size);
        }
    }
}
