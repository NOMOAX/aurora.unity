using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// An item of a scroll view.
    /// </summary>
    /// <remarks>Different types of <see cref="ScrollViewItem"/> are distinguished by <see cref="identifier"/>.</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollViewItem : UIBehaviour
    {
        [SerializeField]
        [GuiDisable]
        internal ScrollView scrollView;

        [SerializeField]
        [GuiDisable]
        internal int index = -1;

        [SerializeField]
        [GuiDisable]
        internal bool visible;

        /// <summary>
        /// The identifier used to distinguish different types of <see cref="ScrollViewItem"/>.
        /// </summary>
        /// <remarks>Remember to assign it at edit time and do not modify it at runtime.</remarks>
        public string identifier = string.Empty;

        /// <summary>
        /// The scroll view.
        /// </summary>
        /// <returns>Assign it before the first <see cref="OnGet"/> call.</returns>
        public ScrollView ScrollView => scrollView;

        /// <summary>
        /// The index within the <see cref="ScrollView"/>.
        /// </summary>
        public int Index => index;

        /// <summary>
        /// Whether this <see cref="ScrollViewItem"/> has entered the <see cref="ScrollView"/>'s viewport.
        /// </summary>
        public bool Visible => visible;

        /// <summary>
        /// Executed when this <see cref="ScrollViewItem"/> is used.
        /// </summary>
        /// <param name="isNewCreated"><see langword="true"/> if it is newly created; otherwise <see langword="false"/>. Exposing this behavior helps to perform extra operations on newly created items.</param>
        protected internal virtual void OnGet(bool isNewCreated)
        {
        }

        /// <summary>
        /// Executed when this <see cref="ScrollViewItem"/> is put into the cache and when the scroll view is being destroyed.
        /// </summary>
        /// <param name="isScrollViewBeingDestroyed"><see langword="true"/> if the scroll view is being destroyed; otherwise <see langword="false"/>.</param>
        protected internal virtual void OnReturn(bool isScrollViewBeingDestroyed)
        {
        }

        /// <summary>
        /// This <see cref="ScrollViewItem"/> enters the <see cref="ScrollView"/>'s viewport.
        /// </summary>
        protected internal virtual void OnVisible()
        {
        }

        /// <summary>
        /// This <see cref="ScrollViewItem"/> leaves the <see cref="ScrollView"/>'s viewport.
        /// </summary>
        protected internal virtual void OnInvisible()
        {
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = GetType().FullName;
            }
        }
#endif
    }
}
