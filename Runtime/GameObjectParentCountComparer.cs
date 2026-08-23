using System.Collections.Generic;
using Aurora.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A comparer that compares game objects by their parent count.
    /// </summary>
    public sealed class GameObjectParentCountComparer : IComparer<GameObject>
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static GameObjectParentCountComparer Instance { get; } = new();

        /// <summary>
        /// Gets the single instance, but the comparison result is reversed relative to <see cref="Instance"/>.
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
            if (!gameObject)
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
