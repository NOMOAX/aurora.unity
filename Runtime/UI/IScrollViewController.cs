namespace Aurora.Unity.UI
{
    /// <summary>
    /// <see cref="ScrollView"/> 的控制器。
    /// </summary>
    public interface IScrollViewController
    {
        /// <summary>
        /// 获取项的数量。
        /// </summary>
        /// <param name="scrollView">滚动视图。</param>
        /// <returns>项的数量。</returns>
        int GetItemCount(ScrollView scrollView);

        /// <summary>
        /// 获取指定索引处的项的大小。
        /// </summary>
        /// <param name="scrollView">滚动视图。</param>
        /// <param name="index">索引。</param>
        /// <returns>索引为 <paramref name="index"/> 的项的大小。</returns>
        float GetItemSize(ScrollView scrollView, int index);

        /// <summary>
        /// 获取指定索引处的项（通过调用 <see cref="ScrollView.GetRecycledOrCreateNewItem"/> 以获取，还可以将需要使用的数据传递给项）。
        /// </summary>
        /// <param name="scrollView">滚动视图。</param>
        /// <param name="index">索引。</param>
        /// <param name="isNewCreated">项是否是新创建的。</param>
        /// <returns>项。</returns>
        ScrollViewItem GetItem(ScrollView scrollView, int index, out bool isNewCreated);

#if UNITY_EDITOR
        /// <summary>
        /// （仅编辑器环境下）获取指定项的名称，用于在编辑器环境中设置项的名称，以便于调试。
        /// </summary>
        /// <param name="item">项。</param>
        /// <returns>项的名称。</returns>
        /// <remarks>若不需要支持此行为，可返回 <see langword="null"/> 或抛出异常，方法的调用者会妥善处理。</remarks>
        string GetItemName(ScrollViewItem item);
#endif
    }
}
