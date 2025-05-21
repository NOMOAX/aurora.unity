using System.Collections.Generic;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 用于比较 <see cref="Object"/> 的相等性。
    /// </summary>
    public sealed class UnityEngineObjectEqualityComparer : IEqualityComparer<Object>
    {
        /// <summary>
        /// 获取单一实例。
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
