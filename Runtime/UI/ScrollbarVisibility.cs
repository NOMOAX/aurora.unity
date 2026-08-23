namespace Aurora.Unity.UI
{
    /// <summary>
    /// Represents the visibility of a scrollbar.
    /// </summary>
    public enum ScrollbarVisibility
    {
        /// <summary>
        /// Always invisible.
        /// </summary>
        Never,

        /// <summary>
        /// Visible only when the content size is larger than the viewport size.
        /// </summary>
        /// <seealso cref="ScrollView.OverflowedContentSize"/>
        OnlyIfNeeded,

        /// <summary>
        /// Always visible.
        /// </summary>
        Always
    }
}
