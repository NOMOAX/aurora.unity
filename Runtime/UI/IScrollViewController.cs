namespace Aurora.Unity.UI
{
    /// <summary>
    /// The controller of a scroll view.
    /// </summary>
    public interface IScrollViewController
    {
        /// <summary>
        /// Gets the number of items.
        /// </summary>
        /// <param name="scrollView">The scroll view.</param>
        /// <returns>The number of items.</returns>
        int GetItemCount(ScrollView scrollView);

        /// <summary>
        /// Gets the size of the item at the specified index.
        /// </summary>
        /// <param name="scrollView">The scroll view.</param>
        /// <param name="index">The index.</param>
        /// <returns>The size of the item at index <paramref name="index"/>.</returns>
        float GetItemSize(ScrollView scrollView, int index);

        /// <summary>
        /// Gets the item at the specified index (obtained by calling <see cref="ScrollView.GetRecycledOrCreateNewItem"/>; you can also pass the data the item needs).
        /// </summary>
        /// <param name="scrollView">The scroll view.</param>
        /// <param name="index">The index.</param>
        /// <param name="isNewCreated">Whether the item is newly created.</param>
        /// <returns>The item.</returns>
        ScrollViewItem GetItem(ScrollView scrollView, int index, out bool isNewCreated);

#if UNITY_EDITOR
        /// <summary>
        /// (Editor only) Gets the name of the specified item, used to set the item's name in the editor environment for debugging.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The name of the item.</returns>
        /// <remarks>If you do not need to support this behavior, you can return <see langword="null"/> or throw an exception; the caller handles it appropriately.</remarks>
        string GetItemName(ScrollViewItem item);
#endif
    }
}
