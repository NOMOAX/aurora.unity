using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// A mask view.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaskView : PrefabLessView, IPointerClickHandler
    {
        private Block _block;

        /// <summary>
        /// Gets the mask.
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

        protected override void OnEnable()
        {
            base.OnEnable();

            _block = gameObject.AddComponent<Block>();
            _block.color = State switch
            {
                Args args       => args.MaskColor,
                Color color     => color,
                Color32 color32 => color32,
                _               => Color.clear
            };
        }

        protected override void OnDisable()
        {
            Destroy(_block);
            _block = null;

            base.OnDisable();
        }

        /// <summary>
        /// The mask view arguments.
        /// </summary>
        public sealed class Args
        {
            /// <summary>
            /// The mask color.
            /// </summary>
            public Color MaskColor { get; set; }

            /// <summary>
            /// Closes the view when clicked.
            /// </summary>
            /// <remarks>Closes the view before executing <see cref="InvocationOnClick"/>.</remarks>
            public bool CloseOnClick { get; set; }

            /// <summary>
            /// The invocation executed when clicked.
            /// </summary>
            public Invocation InvocationOnClick { get; set; }
        }
    }
}
