namespace Aurora.Unity.UI
{
    /// <summary>
    /// 表示滚动条的可见性。
    /// </summary>
    public enum ScrollbarVisibility
    {
        /// <summary>
        /// 总是不可见。
        /// </summary>
        Never,

        /// <summary>
        /// 仅当内容大小大于视口大小时可见。
        /// </summary>
        /// <seealso cref="ScrollView.OverflowedContentSize"/>
        OnlyIfNeeded,

        /// <summary>
        /// 总是可见。
        /// </summary>
        Always
    }
}
