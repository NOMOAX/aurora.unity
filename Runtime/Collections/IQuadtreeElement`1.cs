namespace Aurora.Unity.Collections
{
    /// <summary>
    /// A quadtree element.
    /// </summary>
    /// <typeparam name="TPosition">The position of the element.</typeparam>
    public interface IQuadtreeElement<TPosition>
    {
        /// <summary>
        /// Gets the position.
        /// </summary>
        TPosition Position { get; }

        /// <summary>
        /// Sets the quadtree node that directly holds this element.
        /// </summary>
        /// <param name="owner">If this element is in the quadtree, the quadtree node that directly holds this element; otherwise <see langword="null"/>.</param>
        /// <remarks>This method is designed to be called only by <see cref="Quadtree{TElementPosition}.Node"/>; developers only need to update the reference that records this value in their implementation so that <see cref="Quadtree{TElementPosition}.Node.Remove"/> can be called correctly.</remarks>
        void SetOwner(Quadtree<TPosition>.Node owner);
    }
}
