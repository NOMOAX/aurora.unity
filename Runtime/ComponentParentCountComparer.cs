using System.Collections.Generic;
using Aurora.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A comparer that compares components by the parent count of the game object they belong to.
    /// </summary>
    public sealed class ComponentParentCountComparer : IComparer<Component>
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static ComponentParentCountComparer Instance { get; } = new();

        /// <summary>
        /// Gets the single instance, but the comparison result is reversed relative to <see cref="Instance"/>.
        /// </summary>
        public static IComparer<Component> InstanceReversed { get; } = new ReversedComparer<Component>(Instance);

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
            if (!component)
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
