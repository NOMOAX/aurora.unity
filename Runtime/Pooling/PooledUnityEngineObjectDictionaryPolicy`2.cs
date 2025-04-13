using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// 管理池中的字典的策略。
    /// </summary>
    /// <typeparam name="TKey">字典的键的类型。它是 <see cref="Object"/> 类型或其派生类型。</typeparam>
    /// <typeparam name="TValue">字典的值的类型。</typeparam>
    public class PooledUnityEngineObjectDictionaryPolicy<TKey, TValue> : IPooledObjectPolicy<Dictionary<TKey, TValue>>
        where TKey : Object
    {
        /// <summary>
        /// 获取或设置池化的字典的初始容量。
        /// </summary>
        public int InitialCapacity { get; set; } = 17;

        /// <summary>
        /// 获取或设置允许被放入池的字典的最大长度。
        /// </summary>
        public int MaximumRetainedCount { get; set; } = 293;

        /// <inheritdoc />
        public Dictionary<TKey, TValue> Create()
        {
            return new Dictionary<TKey, TValue>(InitialCapacity, UnityEngineObjectEqualityComparer.Instance);
        }

        /// <inheritdoc />
        public void Get(Dictionary<TKey, TValue> obj)
        {
        }

        /// <inheritdoc />
        public bool Return(Dictionary<TKey, TValue> obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (obj.Count > MaximumRetainedCount)
            {
                return false;
            }
            if (!(obj.Comparer is UnityEngineObjectEqualityComparer))
            {
                return false;
            }
            obj.Clear();
            return true;
        }

        /// <inheritdoc />
        public void Dispose(Dictionary<TKey, TValue> obj)
        {
            obj?.Clear();
        }
    }
}
