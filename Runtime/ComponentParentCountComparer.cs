using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 根据组件所在的游戏物体的父级数量来比较组件的比较器。
    /// </summary>
    public sealed class ComponentParentCountComparer : IComparer<Component>
    {
        /// <summary>
        /// 获取单一实例。
        /// </summary>
        public static ComponentParentCountComparer Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        } = new ComponentParentCountComparer();

        /// <summary>
        /// 获取单一实例，但比较结果与 <see cref="Instance"/> 相反。
        /// </summary>
        public static IComparer<Component> InstanceReversed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        } = new ReversedComparer<Component>(Instance);

        /// <inheritdoc />
        public int Compare(Component x, Component y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }
            if (y is null)
            {
                return 1;
            }
            if (ReferenceEquals(x, y))
            {
                return 0;
            }
            var parentCountX = GetParentCount(x);
            var parentCountY = GetParentCount(y);
            return parentCountX.CompareTo(parentCountY);
        }

        private static int GetParentCount(Component component)
        {
            if (component == null)
            {
                return -1;
            }
            var parentCount = -1;
            var transform   = component as Transform ?? component.transform;
            do
            {
                parentCount++;
                transform = transform.parent;
            } while (!ReferenceEquals(transform, null));
            return parentCount;
        }
    }
}
