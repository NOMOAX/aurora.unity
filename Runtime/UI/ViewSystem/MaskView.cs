using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 遮罩界面。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaskView : PrefabLessView, IPointerClickHandler
    {
        private Block _block;

        /// <summary>
        /// 获取遮罩。
        /// </summary>
        public Graphic MaskGraphic => _block;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (State is Args args)
                {
                    var invocationOnClick = args.InvocationOnClick;
                    if (args.CloseOnClick)
                    {
                        Close();
                    }
                    invocationOnClick?.Invoke();
                }
            }
        }

        private void OnEnable()
        {
            _block = gameObject.AddComponent<Block>();
            _block.color = State switch
            {
                Args args       => args.MaskColor,
                Color color     => color,
                Color32 color32 => color32,
                _               => Color.clear
            };
        }

        private void OnDisable()
        {
            Destroy(_block);
            _block = null;
        }

        /// <summary>
        /// 遮罩界面参数。
        /// </summary>
        public sealed class Args
        {
            /// <summary>
            /// 遮罩颜色。
            /// </summary>
            public Color MaskColor { get; set; }

            /// <summary>
            /// 点击时关闭界面。
            /// </summary>
            /// <remarks>在执行 <see cref="InvocationOnClick"/> 之前关闭界面。</remarks>
            public bool CloseOnClick { get; set; }

            /// <summary>
            /// 点击时执行的调用。
            /// </summary>
            public Invocation InvocationOnClick { get; set; }
        }
    }
}
