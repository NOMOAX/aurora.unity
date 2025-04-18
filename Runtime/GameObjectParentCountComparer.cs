using System.Collections.Generic;
using Aurora.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 根据游戏物体的父级数量来比较游戏物体的比较器。
    /// </summary>
    public sealed class GameObjectParentCountComparer : IComparer<GameObject>
    {
        /// <summary>
        /// 获取单一实例。
        /// </summary>
        public static GameObjectParentCountComparer Instance { get; } = new GameObjectParentCountComparer();

        /// <summary>
        /// 获取单一实例，但比较结果与 <see cref="Instance"/> 相反。
        /// </summary>
        public static IComparer<GameObject> InstanceReversed { get; } = new ReversedComparer<GameObject>(Instance);

        /// <inheritdoc />
        public int Compare(GameObject x, GameObject y)
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

        private static int GetParentCount(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return -1;
            }
            var parentCount = -1;
            var transform   = gameObject.transform;
            do
            {
                parentCount++;
                transform = transform.parent;
            } while (!ReferenceEquals(transform, null));
            return parentCount;
        }
    }
}
