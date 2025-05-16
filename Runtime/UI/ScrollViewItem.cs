using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// <see cref="UI.ScrollView"/> 的项。
    /// </summary>
    /// <remarks>不同类型的 <see cref="ScrollViewItem"/> 使用 <see cref="identifier"/> 来区分。</remarks>
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
        /// 标识符，用于区分不同类型的 <see cref="ScrollViewItem"/>。
        /// </summary>
        /// <remarks>别忘了在编辑时赋值，并且不要在运行时修改。</remarks>
        public string identifier = string.Empty;

        /// <summary>
        /// 滚动视图。
        /// </summary>
        /// <returns>在首次执行 <see cref="OnGet"/> 之前赋值。</returns>
        public ScrollView ScrollView => scrollView;

        /// <summary>
        /// 在 <see cref="ScrollView"/> 中的索引。
        /// </summary>
        public int Index => index;

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 是否已进入 <see cref="ScrollView"/> 的视口。
        /// </summary>
        public bool Visible => visible;

        /// <summary>
        /// 在使用此 <see cref="ScrollViewItem"/> 时执行。
        /// </summary>
        /// <param name="isNewCreated">如果是新创建的，则为 <see langword="true"/>；否则为 <see langword="false"/>。暴露此行为有助于对新创建的项做一些额外操作。</param>
        protected internal virtual void OnGet(bool isNewCreated)
        {
        }

        /// <summary>
        /// 在将此 <see cref="ScrollViewItem"/> 放入缓存时执行。
        /// </summary>
        protected internal virtual void OnReturn()
        {
        }

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 进入 <see cref="ScrollView"/> 的视口。
        /// </summary>
        protected internal virtual void OnVisible()
        {
        }

        /// <summary>
        /// 此 <see cref="ScrollViewItem"/> 离开 <see cref="ScrollView"/> 的视口。
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
