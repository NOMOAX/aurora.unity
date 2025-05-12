using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// <see cref="UI.ScrollView"/> 的项。
    /// </summary>
    /// <remarks>不同类型的 <see cref="ScrollViewItem"/> 使用 <see cref="identifier"/> 来区分。</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollViewItem : MonoBehaviour
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
        /// 标识符，用于区分不同类型的 <see cref="ScrollViewItem"/>。
        /// </summary>
        /// <remarks>请在编辑时设置好，不要在运行时更改。</remarks>
        public string identifier;

        /// <summary>
        /// 滚动视图。
        /// </summary>
        public ScrollView ScrollView => scrollView;

        /// <summary>
        /// 在 <see cref="ScrollView"/> 中的索引。
        /// </summary>
        public int Index => index;

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 是否进入了 <see cref="ScrollView"/>。
        /// </summary>
        public bool Visible => visible;

        /// <summary>
        /// 在使用此 <see cref="ScrollViewItem"/> 时执行。
        /// </summary>
        /// <param name="enhancer">拥有此 <see cref="ScrollViewItem"/> 的 <see cref="ScrollView"/>。</param>
        /// <param name="isNewCreated">如果是新创建的，则为 <see langword="true"/>；否则为 <see langword="false"/>。暴露此行为有助于对新创建的项做一些额外操作。</param>
        protected internal virtual void OnGet(ScrollView enhancer, bool isNewCreated)
        {
        }

        /// <summary>
        /// 在将此 <see cref="ScrollViewItem"/> 放入缓存时执行。
        /// </summary>
        /// <param name="enhancer">拥有此 <see cref="ScrollViewItem"/> 的 <see cref="ScrollView"/>。</param>
        protected internal virtual void OnReturn(ScrollView enhancer)
        {
        }

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 进入 <see cref="ScrollView"/>。
        /// </summary>
        /// <param name="enhancer">拥有此 <see cref="ScrollViewItem"/> 的 <see cref="ScrollView"/>。</param>
        protected internal virtual void OnVisible(ScrollView enhancer)
        {
        }

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 离开 <see cref="ScrollView"/>。
        /// </summary>
        /// <param name="enhancer">拥有此 <see cref="ScrollViewItem"/> 的 <see cref="ScrollView"/>。</param>
        protected internal virtual void OnInvisible(ScrollView enhancer)
        {
        }
    }
}
