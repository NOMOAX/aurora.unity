using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Collections
{
    /// <summary>
    /// An octree.
    /// </summary>
    /// <typeparam name="TElementPosition">The type of the position of an octree element.</typeparam>
    public abstract class Octree<TElementPosition>
    {
        private readonly ICreateNodeHandler _createNodeHandler;

        private readonly Node _rootNode;

        private readonly Aabb3 _aabb3;

        private readonly int _levels;

        private readonly int _maxElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="Octree{TElementPosition}"/> class.
        /// </summary>
        /// <param name="createNodeHandler">The handler used to create octree nodes.</param>
        /// <param name="aabb3">The range of the octree.</param>
        /// <param name="levels">
        /// The maximum number of octree node levels.
        /// <br/>
        /// The recommended value is 5.
        /// </param>
        /// <param name="maxElements">
        /// The maximum number of elements a single octree node directly holds.
        /// <br/>
        /// The recommended value is 16.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="createNodeHandler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The root node created by calling <paramref name="createNodeHandler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="aabb3"/> is not a number or is infinity, or <paramref name="levels"/> is less than 1, or <paramref name="maxElements"/> is less than 1.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Octree(ICreateNodeHandler createNodeHandler, Aabb3 aabb3, int levels, int maxElements)
        {
            if (createNodeHandler is null)
            {
                throw new ArgumentNullException(nameof(createNodeHandler));
            }
            if (aabb3.MinX is float.NaN || float.IsInfinity(aabb3.MinX) || aabb3.MaxX is float.NaN ||
                float.IsInfinity(aabb3.MaxX) || aabb3.MinY is float.NaN || float.IsInfinity(aabb3.MinY) ||
                aabb3.MaxY is float.NaN || float.IsInfinity(aabb3.MaxY) || aabb3.MinZ is float.NaN ||
                float.IsInfinity(aabb3.MinZ) || aabb3.MaxZ is float.NaN || float.IsInfinity(aabb3.MaxZ))
            {
                throw new ArgumentOutOfRangeException(nameof(aabb3), aabb3, null);
            }
            if (levels < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levels), levels, null);
            }
            if (maxElements < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxElements), maxElements, null);
            }
            var rootNode = createNodeHandler.CreateNode(this, null, 0, aabb3);
            if (rootNode is null)
            {
                throw new ArgumentException("CreateNodeHandler is invalid.", nameof(createNodeHandler));
            }
            _createNodeHandler = createNodeHandler;
            _rootNode          = rootNode;
            _aabb3             = aabb3;
            _levels            = levels;
            _maxElements       = maxElements;
        }

        /// <summary>
        /// Gets the root node.
        /// </summary>
        public Node RootNode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rootNode;
        }

        /// <summary>
        /// Gets the range.
        /// </summary>
        public Aabb3 Aabb3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _aabb3;
        }

        /// <summary>
        /// Gets the maximum number of node levels.
        /// </summary>
        public int Levels
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _levels;
        }

        /// <summary>
        /// Gets the maximum number of elements a single node directly holds.
        /// </summary>
        /// <remarks>
        /// There are exceptional cases, see the table below:
        /// <list type="bullet">
        /// <item><description>The node level has reached the maximum value (<see cref="Levels"/> minus 1)</description></item>
        /// <item><description>An element cannot be contained by any child node, or can be contained by multiple child nodes simultaneously</description></item>
        /// </list>
        /// </remarks>
        public int MaxElements
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxElements;
        }

        /// <summary>
        /// Determines whether the octree contains the position of the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns><see langword="true"/> if the octree contains <see cref="IOctreeElement{TPosition}.Position"/> of <paramref name="element"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(IOctreeElement<TElementPosition> element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return _rootNode.Contains(element);
        }

        /// <summary>
        /// Determines whether the octree contains the specified position.
        /// </summary>
        /// <param name="elementPosition">The position.</param>
        /// <returns><see langword="true"/> if the octree contains <paramref name="elementPosition"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TElementPosition elementPosition)
        {
            return _rootNode.Contains(elementPosition);
        }

        /// <summary>
        /// Adds an element to the octree.
        /// </summary>
        /// <param name="element">The element to add to the octree.</param>
        /// <returns><see langword="true"/> if successfully added to the octree; otherwise <see langword="false"/>, the reason for failure is "even the octree root node cannot hold the element to add to the octree".</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(IOctreeElement<TElementPosition> element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (!_rootNode.Contains(element))
            {
                return false;
            }
            _rootNode.Add(element);
            return true;
        }

        /// <summary>
        /// Gets all elements contained in the specified sphere in the octree.
        /// </summary>
        /// <param name="center">The sphere center.</param>
        /// <param name="radius">The sphere radius</param>
        /// <param name="results">The list used to hold the results.</param>
        /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="center"/> is not a number or is infinity, or <paramref name="radius"/> is not a number or is negative or is positive infinity.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetElementsInSphere(Vector3 center, float radius, List<IOctreeElement<TElementPosition>> results)
        {
            if (center.x is float.NaN || float.IsInfinity(center.x) || center.y is float.NaN ||
                float.IsInfinity(center.y) || center.z is float.NaN || float.IsInfinity(center.z))
            {
                throw new ArgumentOutOfRangeException(nameof(center), center, null);
            }
            if (radius is float.NaN or < 0 or float.PositiveInfinity)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, null);
            }
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            _rootNode.GetElementsInSphere(center, radius, results);
        }

        /// <summary>
        /// Gets all elements contained in the specified range in the octree.
        /// </summary>
        /// <param name="aabb3">The range.</param>
        /// <param name="results">The list used to hold the results.</param>
        /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="aabb3"/> is not a number or is infinity.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetElementsInAabb3(Aabb3 aabb3, List<IOctreeElement<TElementPosition>> results)
        {
            if (aabb3.MinX is float.NaN || float.IsInfinity(aabb3.MinX) || aabb3.MaxX is float.NaN ||
                float.IsInfinity(aabb3.MaxX) || aabb3.MinY is float.NaN || float.IsInfinity(aabb3.MinY) ||
                aabb3.MaxY is float.NaN || float.IsInfinity(aabb3.MaxY) || aabb3.MinZ is float.NaN ||
                float.IsInfinity(aabb3.MinZ) || aabb3.MaxZ is float.NaN || float.IsInfinity(aabb3.MaxZ))
            {
                throw new ArgumentOutOfRangeException(nameof(aabb3), aabb3, null);
            }
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            _rootNode.GetElementsInAabb3(aabb3, results);
        }

        /// <summary>
        /// An octree node.
        /// </summary>
        public abstract class Node
        {
            private readonly Octree<TElementPosition> _tree;

            private readonly Node _parent;

            private readonly int _level;

            private readonly Aabb3 _aabb3;

            private readonly List<IOctreeElement<TElementPosition>> _elements = new();

            private Node[] _children;

            protected Node(Octree<TElementPosition> tree, Node parent, int level, Aabb3 aabb3)
            {
                _tree   = tree;
                _parent = parent;
                _level  = level;
                _aabb3  = aabb3;
            }

            /// <summary>
            /// Gets the octree that this node belongs to.
            /// </summary>
            public Octree<TElementPosition> Tree
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _tree;
            }

            /// <summary>
            /// Gets the parent node.
            /// </summary>
            public Node Parent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _parent;
            }

            /// <summary>
            /// Gets the level.
            /// </summary>
            /// <remarks>The root node level is 0.</remarks>
            public int Level
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _level;
            }

            /// <summary>
            /// Gets the range.
            /// </summary>
            public Aabb3 Aabb3
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _aabb3;
            }

            /// <summary>
            /// Gets the number of elements held (directly and indirectly).
            /// </summary>
            public int Count
            {
                get
                {
                    var count = _elements.Count;
                    if (_children != null)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            count += _children[i].Count;
                        }
                    }
                    return count;
                }
            }

            /// <summary>
            /// Determines whether the octree node contains the position of the specified element.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <returns><see langword="true"/> if the octree node contains <see cref="IOctreeElement{TPosition}.Position"/> of <paramref name="element"/>; otherwise <see langword="false"/>.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(IOctreeElement<TElementPosition> element)
            {
                if (element is null)
                {
                    throw new ArgumentNullException(nameof(element));
                }
                return Contains(_aabb3, element.Position);
            }

            /// <summary>
            /// Determines whether the octree node contains the specified position.
            /// </summary>
            /// <param name="elementPosition">The position.</param>
            /// <returns><see langword="true"/> if the octree node contains <paramref name="elementPosition"/>; otherwise <see langword="false"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TElementPosition elementPosition)
            {
                return Contains(_aabb3, elementPosition);
            }

            /// <summary>
            /// Determines whether the specified range contains the specified position.
            /// </summary>
            /// <param name="aabb3">The range.</param>
            /// <param name="elementPosition">The position.</param>
            /// <returns><see langword="true"/> if <paramref name="aabb3"/> contains <paramref name="elementPosition"/>; otherwise <see langword="false"/>.</returns>
            protected abstract bool Contains(Aabb3 aabb3, TElementPosition elementPosition);

            /// <summary>
            /// Gets the directly held child elements.
            /// </summary>
            /// <param name="results">The list used to hold the results.</param>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElements(List<IOctreeElement<TElementPosition>> results)
            {
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                results.AddRange(_elements);
            }

            /// <summary>
            /// Gets the directly held child elements of the specified type.
            /// </summary>
            /// <param name="results">The list used to hold the results.</param>
            /// <typeparam name="T">The type of the child elements.</typeparam>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElements<T>(List<T> results) where T : IOctreeElement<TElementPosition>
            {
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                foreach (var element in _elements)
                {
                    if (element is T t)
                    {
                        results.Add(t);
                    }
                }
            }

            /// <summary>
            /// Gets the 8 child nodes.
            /// </summary>
            /// <param name="results">The array used to hold the results.</param>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">The length of <paramref name="results"/> is less than 8, so the results cannot be stored in it.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetChildren(Node[] results)
            {
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                if (_children is null)
                {
                    return;
                }
                if (results.Length < 8)
                {
                    throw new ArgumentException("The array length is insufficient", nameof(results));
                }
                Array.Copy(_children, 0, results, 0, 8);
            }

            /// <summary>
            /// Gets all elements contained in the specified sphere in the octree node.
            /// </summary>
            /// <param name="center">The sphere center.</param>
            /// <param name="radius">The sphere radius</param>
            /// <param name="results">The list used to hold the results.</param>
            /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="center"/> is not a number or is infinity, or <paramref name="radius"/> is not a number or is negative or is positive infinity.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElementsInSphere(
                Vector3                                center,
                float                                  radius,
                List<IOctreeElement<TElementPosition>> results)
            {
                if (center.x is float.NaN || float.IsInfinity(center.x) || center.y is float.NaN ||
                    float.IsInfinity(center.y) || center.z is float.NaN || float.IsInfinity(center.z))
                {
                    throw new ArgumentOutOfRangeException(nameof(center), center, null);
                }
                if (radius is float.NaN or < 0 or float.PositiveInfinity)
                {
                    throw new ArgumentOutOfRangeException(nameof(radius), radius, null);
                }
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                var squareRadius = radius * radius;
                GetElementsInSphereCore(center, squareRadius, results);
            }

            private void GetElementsInSphereCore(
                Vector3                                       center,
                float                                         squareRadius,
                ICollection<IOctreeElement<TElementPosition>> results)
            {
                var squareDistance = OctreeHelper.GetSquareDistance(_aabb3, center);
                if (!(squareDistance <= squareRadius))
                {
                    return;
                }
                foreach (var element in _elements)
                {
                    if (GetSquareDistance(element.Position, center) <= squareRadius)
                    {
                        results.Add(element);
                    }
                }
                if (_children is null)
                {
                    return;
                }
                for (var i = 0; i < 8; i++)
                {
                    _children[i].GetElementsInSphereCore(center, squareRadius, results);
                }
            }

            /// <summary>
            /// Gets all elements contained in the specified range in the octree node.
            /// </summary>
            /// <param name="aabb3">The range.</param>
            /// <param name="results">The list used to hold the results.</param>
            /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="aabb3"/> is not a number or is infinity.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElementsInAabb3(Aabb3 aabb3, List<IOctreeElement<TElementPosition>> results)
            {
                if (aabb3.MinX is float.NaN || float.IsInfinity(aabb3.MinX) || aabb3.MaxX is float.NaN ||
                    float.IsInfinity(aabb3.MaxX) || aabb3.MinY is float.NaN || float.IsInfinity(aabb3.MinY) ||
                    aabb3.MaxY is float.NaN || float.IsInfinity(aabb3.MaxY) || aabb3.MinZ is float.NaN ||
                    float.IsInfinity(aabb3.MinZ) || aabb3.MaxZ is float.NaN || float.IsInfinity(aabb3.MaxZ))
                {
                    throw new ArgumentOutOfRangeException(nameof(aabb3), aabb3, null);
                }
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                GetElementsInAabb3Core(aabb3, results);
            }

            private void GetElementsInAabb3Core(Aabb3 aabb3, List<IOctreeElement<TElementPosition>> results)
            {
                if (!_aabb3.Overlaps(aabb3))
                {
                    return;
                }
                foreach (var element in _elements)
                {
                    if (Contains(aabb3, element.Position))
                    {
                        results.Add(element);
                    }
                }
                if (_children is null)
                {
                    return;
                }
                for (var i = 0; i < 8; i++)
                {
                    _children[i].GetElementsInAabb3Core(aabb3, results);
                }
            }

            /// <summary>
            /// Gets the squared distance between the specified position and the specified point.
            /// </summary>
            /// <param name="elementPosition">The position.</param>
            /// <param name="point">The point.</param>
            /// <returns>The squared distance between <paramref name="elementPosition"/> and <paramref name="point"/>.</returns>
            protected abstract float GetSquareDistance(TElementPosition elementPosition, Vector3 point);

            internal void Add(IOctreeElement<TElementPosition> element)
            {
                // This node is not a leaf node and can split
                if (_level < _tree._levels - 1)
                {
                    // No child nodes
                    if (_children is null)
                    {
                        // Set it as a direct element of itself first, then check whether splitting is needed
                        _elements.Add(element);
                        element.SetOwner(this);
                        TrySplitIfExceeded();
                    }
                    // There are child nodes
                    // Try to put it into a child node first
                    // If that is not possible, set it as a direct element of itself
                    else
                    {
                        var childIndex = GetOnlyContainsElementChildIndex(element);
                        if (childIndex >= 0)
                        {
                            _children[childIndex].Add(element);
                        }
                        else
                        {
                            _elements.Add(element);
                            element.SetOwner(this);
                        }
                    }
                }
                // This node is a leaf node; there is no choice but to set it as a direct element of itself
                else
                {
                    _elements.Add(element);
                    element.SetOwner(this);
                }
            }

            private void TrySplitIfExceeded()
            {
                if (_elements.Count <= _tree._maxElements)
                {
                    return;
                }
                // The element count exceeds the maximum value
                // Try to split, but it will not necessarily split, because if none of these elements can be put into a child node, splitting is meaningless
                for (var i = 0; i < _elements.Count;)
                {
                    var element    = _elements[i];
                    var childIndex = GetOnlyContainsElementChildIndex(element);
                    // This element should be put into a child node
                    if (childIndex >= 0)
                    {
                        _elements.RemoveAt(i);
                        if (_children is null)
                        {
                            // Create a child node
                            // The method that calls this method has already checked _level < _tree._levels - 1, so there is no need to check again here; a child node can be created
                            _children = PredefinedPools<Node>.ArrayLength8.Get();
                            for (var j = 0; j < 8; j++)
                            {
                                var childLevel = _level + 1;
                                var childAabb3 = OctreeHelper.GetChildAabb3(_aabb3, j);
                                _children[j] = _tree._createNodeHandler.CreateNode(_tree, this, childLevel, childAabb3);
                            }
                        }
                        _children[childIndex].Add(element);
                        // After removing an element from _elements, the following elements all move forward, so i is not incremented
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            private int GetOnlyContainsElementChildIndex(IOctreeElement<TElementPosition> element)
            {
                var containsElementChildIndex = -1;
                for (var i = 0; i < 8; i++)
                {
                    if (!Contains(OctreeHelper.GetChildAabb3(_aabb3, i), element.Position))
                    {
                        continue;
                    }
                    if (containsElementChildIndex >= 0)
                    {
                        return -1;
                    }
                    containsElementChildIndex = i;
                }
                return containsElementChildIndex;
            }

            /// <summary>
            /// Removes the specified element from the octree node.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">
            /// <paramref name="element"/> is not an element directly held by this octree node.
            /// <br/>
            /// Please use the latest value of the parameter at the time <see cref="IOctreeElement{TPosition}.SetOwner"/> was executed.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(IOctreeElement<TElementPosition> element)
            {
                if (element is null)
                {
                    throw new ArgumentNullException(nameof(element));
                }
                if (!_elements.Remove(element))
                {
                    throw new ArgumentException("This node does not directly hold this element", nameof(element));
                }
                element.SetOwner(null);
                OnElementRemoved();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void OnElementRemoved()
            {
                if (Count > _tree._maxElements)
                {
                    return;
                }
                MergeDownwards();
                _parent?.MergeUpwards();
            }

            private void MergeDownwards()
            {
                if (_children is null)
                {
                    return;
                }
                MergeDownwardsCore();
            }

            private void MergeUpwards()
            {
                if (Count > _tree._maxElements)
                {
                    return;
                }
                MergeDownwardsCore();
                _parent?.MergeUpwards();
            }

            private void MergeDownwardsCore()
            {
                for (var i = 0; i < 8; i++)
                {
                    var child = _children[i];
                    child.MergeDownwards();
                    _elements.AddRange(child._elements);
                    foreach (var childElement in child._elements)
                    {
                        childElement.SetOwner(this);
                    }
                    child._elements.Clear();
                }
                PredefinedPools<Node>.ArrayLength8.Return(_children);
                _children = null;
            }
        }

        /// <summary>
        /// Defines the method to create an octree node.
        /// </summary>
        public interface ICreateNodeHandler
        {
            /// <summary>
            /// Creates an octree node.
            /// </summary>
            /// <param name="tree">The octree the node belongs to.</param>
            /// <param name="parent">The parent node of the node. If the node is the root node, it is <see langword="null"/>.</param>
            /// <param name="level">The level of the node (the root node level is 0).</param>
            /// <param name="aabb3"></param>
            /// <returns>The created octree node.</returns>
            /// <remarks>These parameters should be passed as-is to the constructor of <see cref="Node"/>.</remarks>
            Node CreateNode(Octree<TElementPosition> tree, Node parent, int level, Aabb3 aabb3);
        }
    }
}
