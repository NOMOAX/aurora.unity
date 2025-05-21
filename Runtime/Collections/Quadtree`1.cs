using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity.Collections
{
    /// <summary>
    /// 四叉树。
    /// </summary>
    /// <typeparam name="TElementPosition">四叉树的元素的位置的类型。</typeparam>
    public abstract class Quadtree<TElementPosition>
    {
        private readonly ICreateNodeHandler _createNodeHandler;

        private readonly Node _rootNode;

        private readonly Aabb2 _aabb2;

        private readonly int _levels;

        private readonly int _maxElements;

        /// <summary>
        /// 初始化 <see cref="Quadtree{TElementPosition}"/> 类的新实例。
        /// </summary>
        /// <param name="createNodeHandler">用于创建四叉树结点的处理程序。</param>
        /// <param name="aabb2">四叉树的范围。</param>
        /// <param name="levels">
        /// 四叉树结点层次个数的最大值。
        /// <br/>
        /// 推荐值为 5。
        /// </param>
        /// <param name="maxElements">
        /// 四叉树单个结点直接持有元素数量的最大值。
        /// <br/>
        /// 推荐值为 8。
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="createNodeHandler"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">调用 <paramref name="createNodeHandler"/> 创建的根结点为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="aabb2"/> 的任何分量为非数字或无穷大，或者 <paramref name="levels"/> 小于 1，或者 <paramref name="maxElements"/> 小于 1。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Quadtree(ICreateNodeHandler createNodeHandler, Aabb2 aabb2, int levels, int maxElements)
        {
            if (createNodeHandler is null)
            {
                throw new ArgumentNullException(nameof(createNodeHandler));
            }
            if (float.IsNaN(aabb2.MinX) || float.IsInfinity(aabb2.MinX) || float.IsNaN(aabb2.MaxX) ||
                float.IsInfinity(aabb2.MaxX) || float.IsNaN(aabb2.MinY) || float.IsInfinity(aabb2.MinY) ||
                float.IsNaN(aabb2.MaxY) || float.IsInfinity(aabb2.MaxY))
            {
                throw new ArgumentOutOfRangeException(nameof(aabb2), aabb2, null);
            }
            if (levels < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levels), levels, null);
            }
            if (maxElements < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxElements), maxElements, null);
            }
            var rootNode = createNodeHandler.CreateNode(this, null, 0, aabb2);
            if (rootNode is null)
            {
                throw new ArgumentException("CreateNodeHandler is invalid.", nameof(createNodeHandler));
            }
            _createNodeHandler = createNodeHandler;
            _rootNode          = rootNode;
            _aabb2             = aabb2;
            _levels            = levels;
            _maxElements       = maxElements;
        }

        /// <summary>
        /// 获取根结点。
        /// </summary>
        public Node RootNode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rootNode;
        }

        /// <summary>
        /// 获取范围。
        /// </summary>
        public Aabb2 Aabb2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _aabb2;
        }

        /// <summary>
        /// 获取结点层次个数的最大值。
        /// </summary>
        public int Levels
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _levels;
        }

        /// <summary>
        /// 获取单个结点直接持有元素数量的最大值。
        /// </summary>
        /// <remarks>
        /// 存在例外情况，见下表：
        /// <list type="bullet">
        /// <item><description>结点的层次已达到最大值（<see cref="Levels"/> 减 1）</description></item>
        /// <item><description>元素不能被任何子结点包含，或者能被多个子结点同时包含</description></item>
        /// </list>
        /// </remarks>
        public int MaxElements
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxElements;
        }

        /// <summary>
        /// 判断四叉树是否包含指定元素的位置。
        /// </summary>
        /// <param name="element">元素。</param>
        /// <returns>如果四叉树包含 <paramref name="element"/> 的 <see cref="IQuadtreeElement{TPosition}.Position"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(IQuadtreeElement<TElementPosition> element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            return _rootNode.Contains(element);
        }

        /// <summary>
        /// 判断四叉树是否包含指定的位置。
        /// </summary>
        /// <param name="elementPosition">位置。</param>
        /// <returns>如果四叉树包含 <paramref name="elementPosition"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TElementPosition elementPosition)
        {
            return _rootNode.Contains(elementPosition);
        }

        /// <summary>
        /// 将元素添加到四叉树。
        /// </summary>
        /// <param name="element">要添加到四叉树的元素。</param>
        /// <returns>如果成功地添加到了四叉树中，则为 <see langword="true"/>；否则为 <see langword="false"/>，添加失败的原因是“就连四叉树的根结点都无法容纳要添加到四叉树的元素”。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(IQuadtreeElement<TElementPosition> element)
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
        /// 获取四叉树中被指定的圆包含的所有元素。
        /// </summary>
        /// <param name="center">圆心。</param>
        /// <param name="radius">圆半径</param>
        /// <param name="results">用于存放结果的列表。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="center"/> 的任何分量为非数字或无穷大，或者 <paramref name="radius"/> 为非数字或负数或正无穷大。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetElementsInCircle(Vector2 center, float radius, List<IQuadtreeElement<TElementPosition>> results)
        {
            if (float.IsNaN(center.x) || float.IsInfinity(center.x) || float.IsNaN(center.y) ||
                float.IsInfinity(center.y))
            {
                throw new ArgumentOutOfRangeException(nameof(center), center, null);
            }
            if (float.IsNaN(radius) || radius < 0 || float.IsPositiveInfinity(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, null);
            }
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            _rootNode.GetElementsInCircle(center, radius, results);
        }

        /// <summary>
        /// 获取四叉树中被指定的范围包含的所有元素。
        /// </summary>
        /// <param name="aabb2">范围。</param>
        /// <param name="results">用于存放结果的列表。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="aabb2"/> 的任何分量为非数字或无穷大。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetElementsInAabb2(Aabb2 aabb2, List<IQuadtreeElement<TElementPosition>> results)
        {
            if (float.IsNaN(aabb2.MinX) || float.IsInfinity(aabb2.MinX) || float.IsNaN(aabb2.MaxX) ||
                float.IsInfinity(aabb2.MaxX) || float.IsNaN(aabb2.MinY) || float.IsInfinity(aabb2.MinY) ||
                float.IsNaN(aabb2.MaxY) || float.IsInfinity(aabb2.MaxY))
            {
                throw new ArgumentOutOfRangeException(nameof(aabb2), aabb2, null);
            }
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            _rootNode.GetElementsInAabb2(aabb2, results);
        }

        /// <summary>
        /// 四叉树结点。
        /// </summary>
        public abstract class Node
        {
            private readonly Quadtree<TElementPosition> _tree;

            private readonly Node _parent;

            private readonly int _level;

            private readonly Aabb2 _aabb2;

            private readonly List<IQuadtreeElement<TElementPosition>> _elements = new();

            private Node[] _children;

            protected Node(Quadtree<TElementPosition> tree, Node parent, int level, Aabb2 aabb2)
            {
                _tree   = tree;
                _parent = parent;
                _level  = level;
                _aabb2  = aabb2;
            }

            /// <summary>
            /// 获取所在的四叉树。
            /// </summary>
            public Quadtree<TElementPosition> Tree
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _tree;
            }

            /// <summary>
            /// 获取父结点。
            /// </summary>
            public Node Parent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _parent;
            }

            /// <summary>
            /// 获取层次。
            /// </summary>
            /// <remarks>根结点的层次为 0。</remarks>
            public int Level
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _level;
            }

            /// <summary>
            /// 获取范围。
            /// </summary>
            public Aabb2 Aabb2
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _aabb2;
            }

            /// <summary>
            /// 获取（直接和非直接地）持有的元素数。
            /// </summary>
            public int Count
            {
                get
                {
                    var count = _elements.Count;
                    if (_children != null)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            count += _children[i].Count;
                        }
                    }
                    return count;
                }
            }

            /// <summary>
            /// 判断四叉树结点是否包含指定元素的位置。
            /// </summary>
            /// <param name="element">元素。</param>
            /// <returns>如果四叉树结点包含 <paramref name="element"/> 的 <see cref="IQuadtreeElement{TPosition}.Position"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
            /// <exception cref="ArgumentNullException"><paramref name="element"/> 为 <see langword="null"/>。</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(IQuadtreeElement<TElementPosition> element)
            {
                if (element is null)
                {
                    throw new ArgumentNullException(nameof(element));
                }
                return Contains(_aabb2, element.Position);
            }

            /// <summary>
            /// 判断四叉树结点是否包含指定的位置。
            /// </summary>
            /// <param name="elementPosition">位置。</param>
            /// <returns>如果四叉树结点包含 <paramref name="elementPosition"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TElementPosition elementPosition)
            {
                return Contains(_aabb2, elementPosition);
            }

            /// <summary>
            /// 判断指定的范围是否包含指定的位置。
            /// </summary>
            /// <param name="aabb2">范围。</param>
            /// <param name="elementPosition">位置。</param>
            /// <returns>如果 <paramref name="aabb2"/> 包含 <paramref name="elementPosition"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
            protected abstract bool Contains(Aabb2 aabb2, TElementPosition elementPosition);

            /// <summary>
            /// 获取直接持有的子元素。
            /// </summary>
            /// <param name="results">用于存放结果的列表。</param>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElements(List<IQuadtreeElement<TElementPosition>> results)
            {
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                results.AddRange(_elements);
            }

            /// <summary>
            /// 获取直接持有的属于指定类型的子元素。
            /// </summary>
            /// <param name="results">用于存放结果的列表。</param>
            /// <typeparam name="T">子元素的类型。</typeparam>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElements<T>(List<T> results) where T : IQuadtreeElement<TElementPosition>
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
            /// 获取 4 个子结点。
            /// </summary>
            /// <param name="results">用于存放结果的数组。</param>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
            /// <exception cref="ArgumentException"><paramref name="results"/> 的长度小于 4，无法将结果存放到其中。</exception>
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
                if (results.Length < 4)
                {
                    throw new ArgumentException("数组长度不足", nameof(results));
                }
                Array.Copy(_children, 0, results, 0, 4);
            }

            /// <summary>
            /// 获取四叉树结点中被指定的圆包含的所有元素。
            /// </summary>
            /// <param name="center">圆心。</param>
            /// <param name="radius">圆半径</param>
            /// <param name="results">用于存放结果的列表。</param>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="center"/> 的任何分量为非数字或无穷大，或者 <paramref name="radius"/> 为非数字或负数或正无穷大。</exception>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElementsInCircle(
                Vector2                                  center,
                float                                    radius,
                List<IQuadtreeElement<TElementPosition>> results)
            {
                if (float.IsNaN(center.x) || float.IsInfinity(center.x) || float.IsNaN(center.y) ||
                    float.IsInfinity(center.y))
                {
                    throw new ArgumentOutOfRangeException(nameof(center), center, null);
                }
                if (float.IsNaN(radius) || radius < 0 || float.IsPositiveInfinity(radius))
                {
                    throw new ArgumentOutOfRangeException(nameof(radius), radius, null);
                }
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                var squareRadius = radius * radius;
                GetElementsInCircleCore(center, squareRadius, results);
            }

            private void GetElementsInCircleCore(
                Vector2                                         center,
                float                                           squareRadius,
                ICollection<IQuadtreeElement<TElementPosition>> results)
            {
                var squareDistance = QuadtreeHelper.GetSquareDistance(_aabb2, center);
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
                for (var i = 0; i < 4; i++)
                {
                    _children[i].GetElementsInCircleCore(center, squareRadius, results);
                }
            }

            /// <summary>
            /// 获取四叉树结点中被指定的范围包含的所有元素。
            /// </summary>
            /// <param name="aabb2">范围。</param>
            /// <param name="results">用于存放结果的列表。</param>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="aabb2"/> 的任何分量为非数字或无穷大。</exception>
            /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetElementsInAabb2(Aabb2 aabb2, List<IQuadtreeElement<TElementPosition>> results)
            {
                if (float.IsNaN(aabb2.MinX) || float.IsInfinity(aabb2.MinX) || float.IsNaN(aabb2.MaxX) ||
                    float.IsInfinity(aabb2.MaxX) || float.IsNaN(aabb2.MinY) || float.IsInfinity(aabb2.MinY) ||
                    float.IsNaN(aabb2.MaxY) || float.IsInfinity(aabb2.MaxY))
                {
                    throw new ArgumentOutOfRangeException(nameof(aabb2), aabb2, null);
                }
                if (results is null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                GetElementsInAabb2Core(aabb2, results);
            }

            private void GetElementsInAabb2Core(Aabb2 aabb2, List<IQuadtreeElement<TElementPosition>> results)
            {
                if (!_aabb2.Overlaps(aabb2))
                {
                    return;
                }
                foreach (var element in _elements)
                {
                    if (Contains(aabb2, element.Position))
                    {
                        results.Add(element);
                    }
                }
                if (_children is null)
                {
                    return;
                }
                for (var i = 0; i < 4; i++)
                {
                    _children[i].GetElementsInAabb2Core(aabb2, results);
                }
            }

            /// <summary>
            /// 获取指定的位置与指定的点之间的距离的平方。
            /// </summary>
            /// <param name="elementPosition">位置。</param>
            /// <param name="point">点。</param>
            /// <returns><paramref name="elementPosition"/> 与 <paramref name="point"/> 之间的距离的平方。</returns>
            protected abstract float GetSquareDistance(TElementPosition elementPosition, Vector2 point);

            internal void Add(IQuadtreeElement<TElementPosition> element)
            {
                // 此结点不是叶子结点，可以分裂
                if (_level < _tree._levels - 1)
                {
                    // 没有子结点
                    if (_children is null)
                    {
                        // 先设为自己的直接元素，然后再检查是否需要分裂
                        _elements.Add(element);
                        element.SetOwner(this);
                        TrySplitIfExceeded();
                    }
                    // 有子结点
                    // 应该尽量先往子结点里放
                    // 实在不行再设为自己的直接元素
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
                // 此结点是叶子结点，没有什么选择，只能设为自己的直接元素
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
                // 元素数超过了最大值
                // 尝试分裂，但不一定会分裂，因为如果这些元素都不能放到子结点里，那么分裂是没有意义的
                for (var i = 0; i < _elements.Count;)
                {
                    var element    = _elements[i];
                    var childIndex = GetOnlyContainsElementChildIndex(element);
                    // 这个元素应该放到子结点里
                    if (childIndex >= 0)
                    {
                        _elements.RemoveAt(i);
                        if (_children is null)
                        {
                            // 创建子结点
                            // 调用此方法的方法已经判断过 _level < _tree._levels - 1，这里不用再判断了，可以创建子结点
                            _children = PredefinedPools<Node>.ArrayLength4.Get();
                            for (var j = 0; j < 4; j++)
                            {
                                var childLevel = _level + 1;
                                var childAabb2 = QuadtreeHelper.GetChildAabb2(_aabb2, j);
                                _children[j] = _tree._createNodeHandler.CreateNode(_tree, this, childLevel, childAabb2);
                            }
                        }
                        _children[childIndex].Add(element);
                        // 从 _elements 移除元素后，后面的元素都会向前移动，因此 i 不递增
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            private int GetOnlyContainsElementChildIndex(IQuadtreeElement<TElementPosition> element)
            {
                var containsElementChildIndex = -1;
                for (var i = 0; i < 4; i++)
                {
                    if (!Contains(QuadtreeHelper.GetChildAabb2(_aabb2, i), element.Position))
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
            /// 从四叉树结点中移除指定的元素。
            /// </summary>
            /// <param name="element">元素。</param>
            /// <exception cref="ArgumentNullException"><paramref name="element"/> 为 <see langword="null"/>。</exception>
            /// <exception cref="ArgumentException">
            /// <paramref name="element"/> 不是此四叉树结点直接持有的元素。
            /// <br/>
            /// 请使用最新的在 <see cref="IQuadtreeElement{TPosition}.SetOwner"/> 执行时参数的值。
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(IQuadtreeElement<TElementPosition> element)
            {
                if (element is null)
                {
                    throw new ArgumentNullException(nameof(element));
                }
                if (!_elements.Remove(element))
                {
                    throw new ArgumentException("此结点不直接持有该元素", nameof(element));
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
                for (var i = 0; i < 4; i++)
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
                PredefinedPools<Node>.ArrayLength4.Return(_children);
                _children = null;
            }
        }

        /// <summary>
        /// 定义创建四叉树节点的方法。
        /// </summary>
        public interface ICreateNodeHandler
        {
            /// <summary>
            /// 创建一个四叉树结点。
            /// </summary>
            /// <param name="tree">结点所在的四叉树。</param>
            /// <param name="parent">结点的父结点。如果结点为根结点，则为 <see langword="null"/>。</param>
            /// <param name="level">结点的层次（根结点的层次为 0）。</param>
            /// <param name="aabb2">结点的范围。</param>
            /// <returns>创建出来的四叉树结点。</returns>
            /// <remarks>这些参数应该原样传递给 <see cref="Node"/> 的构造函数。</remarks>
            Node CreateNode(Quadtree<TElementPosition> tree, Node parent, int level, Aabb2 aabb2);
        }
    }
}
