using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// The policy for managing pooled hash sets.
    /// </summary>
    /// <typeparam name="T">The type of the hash set members. It is <see cref="Object"/> or a type derived from it.</typeparam>
    public class PooledUnityEngineObjectHashSetPolicy<T> : IPooledObjectPolicy<HashSet<T>> where T : Object
    {
        /// <summary>
        /// Gets or sets the initial capacity of the pooled hash set.
        /// </summary>
        public int InitialCapacity { get; set; } = 17;

        /// <summary>
        /// Gets or sets the maximum length of a hash set that is allowed to be put into the pool.
        /// </summary>
        public int MaximumRetainedCount { get; set; } = 293;

        /// <inheritdoc />
        public HashSet<T> Create()
        {
            return new HashSet<T>(InitialCapacity, UnityEngineObjectEqualityComparer.Instance);
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
