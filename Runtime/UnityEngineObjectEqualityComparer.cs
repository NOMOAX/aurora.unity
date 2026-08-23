using System.Collections.Generic;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Used to compare the equality of <see cref="Object"/>.
    /// </summary>
    public sealed class UnityEngineObjectEqualityComparer : IEqualityComparer<Object>
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static UnityEngineObjectEqualityComparer Instance { get; } = new();

        private UnityEngineObjectEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(Object x, Object y)
        {
            return ReferenceEquals(x, y) || x is not null && y is not null &&
                   UnityEngineObjectUtility.InternalGetInstanceId(x) ==
                   UnityEngineObjectUtility.InternalGetInstanceId(y);
        }

        /// <inheritdoc />
        public int GetHashCode(Object obj)
        {
            return UnityEngineObjectUtility.InternalGetInstanceId(obj);
        }
    }
}
