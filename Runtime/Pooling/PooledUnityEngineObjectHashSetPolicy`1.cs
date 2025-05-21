using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// 管理池中的哈希集的策略。
    /// </summary>
    /// <typeparam name="T">哈希集的成员的类型。它是 <see cref="Object"/> 类型或其派生类型。</typeparam>
    public class PooledUnityEngineObjectHashSetPolicy<T> : IPooledObjectPolicy<HashSet<T>> where T : Object
    {
        /// <summary>
        /// 获取或设置池化的哈希集的初始容量。
        /// </summary>
        public int InitialCapacity { get; set; } = 17;

        /// <summary>
        /// 获取或设置允许被放入池的哈希集的最大长度。
        /// </summary>
        public int MaximumRetainedCount { get; set; } = 293;

        /// <inheritdoc />
        public HashSet<T> Create()
        {
            // return new HashSet<T>(InitialCapacity, UnityEngineObjectEqualityComparer.Instance);

            // TODO 待 Unity 2020 逐渐退出历史舞台后，改用上面被注释掉的代码
            var hashSet = new HashSet<T>(new T[InitialCapacity], UnityEngineObjectEqualityComparer.Instance);
            hashSet.Clear();
            return hashSet;
        }

        /// <inheritdoc />
        public void Get(HashSet<T> obj)
        {
        }

        /// <inheritdoc />
        public bool Return(HashSet<T> obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (obj.Count > MaximumRetainedCount)
            {
                return false;
            }
            if (obj.Comparer is not UnityEngineObjectEqualityComparer)
            {
                return false;
            }
            obj.Clear();
            return true;
        }

        /// <inheritdoc />
        public void Dispose(HashSet<T> obj)
        {
            obj?.Clear();
        }
    }
}
