namespace Aurora.Unity.Collections
{
    /// <summary>
    /// 四叉树的元素。
    /// </summary>
    /// <typeparam name="TPosition">元素的位置。</typeparam>
    public interface IQuadtreeElement<TPosition>
    {
        /// <summary>
        /// 获取位置。
        /// </summary>
        TPosition Position { get; }

        /// <summary>
        /// 设置直接持有此元素的四叉树结点。
        /// </summary>
        /// <param name="owner">如果此元素在四叉树中，则为直接持有此元素的四叉树结点；否则为 <see langword="null"/>。</param>
        /// <remarks>此方法被设计为仅由 <see cref="Quadtree{TElementPosition}.Node"/> 调用，开发者在实现中只需要更新记录这个值的引用即可，以便可以正确地调用 <see cref="Quadtree{TElementPosition}.Node.Remove"/>。</remarks>
        void SetOwner(Quadtree<TPosition>.Node owner);
    }
}
