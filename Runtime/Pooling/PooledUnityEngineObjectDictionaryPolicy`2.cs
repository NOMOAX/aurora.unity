using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// The policy for managing pooled dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys. It is <see cref="Object"/> or a type derived from it.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    public class PooledUnityEngineObjectDictionaryPolicy<TKey, TValue> : IPooledObjectPolicy<Dictionary<TKey, TValue>>
        where TKey : Object
    {
        /// <summary>
        /// Gets or sets the initial capacity of the pooled dictionary.
        /// </summary>
        public int InitialCapacity { get; set; } = 17;

        /// <summary>
        /// Gets or sets the maximum length of a dictionary that is allowed to be put into the pool.
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
            if (obj.Comparer is not UnityEngineObjectEqualityComparer)
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
