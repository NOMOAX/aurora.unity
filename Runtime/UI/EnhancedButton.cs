using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 按钮。
    /// </summary>
    [DisallowMultipleComponent]
    public class EnhancedButton : UIBehaviour,
                                  IPointerEnterHandler,
                                  IPointerExitHandler,
                                  IPointerDownHandler,
                                  IPointerUpHandler,
                                  IPointerClickHandler
    {
        /// <summary>
        /// 表示当按钮被点击时执行的方法。
        /// </summary>
        public delegate void ClickedEventHandler(EnhancedButton button, PointerEventData eventData);

        /// <summary>
        /// 表示当按钮的开关状态发生改变时执行的方法。
        /// </summary>
        public delegate void ToggledEventHandler(EnhancedButton button);

        /// <summary>
        /// 表示当按钮的状态、开关状态或者可交互状态发生更改时执行的方法。
        /// </summary>
        public delegate void UpdatedEventHandler(EnhancedButton button);

        [SerializeField]
        internal bool interactable = true;

        [SerializeField]
        internal bool isOn;

        [SerializeField]
        internal EnhancedButtonGroup group;

        /// <summary>
        /// 是否允许右击。
        /// </summary>
        public bool rightClick;

        /// <summary>
        /// 是否允许双击。
        /// </summary>
        public bool doubleClick;

        private EnhancedButtonState _state;

        private bool _pointerInside;

        private bool _pointerDown;

        private bool _statusCanBePressed;

        private Coroutine _delaySingleClickCoroutine;

        /// <summary>
        /// 获取或设置按钮是否可交互。
        /// </summary>
        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value)
                {
                    return;
                }
                interactable = value;
                OnUpdated();
            }
        }

        /// <summary>
        /// 获取或设置按钮的开关状态。
        /// </summary>
        public bool IsOn { get => isOn; set => SetIsOn(value, true); }

        /// <summary>
        /// 获取按钮的状态。
        /// </summary>
        public EnhancedButtonState State => GetState();

        /// <summary>
        /// 获取或设置组。
        /// </summary>
        /// <remarks>请注意，只有当按钮 <see cref="Behaviour.isActiveAndEnabled"/> 时，它才可能会在组中存在。</remarks>
        public EnhancedButtonGroup Group
        {
            get => group;
            set
            {
                if (group == value)
                {
                    return;
                }
                if (isActiveAndEnabled)
                {
                    if (group)
                    {
                        group.UnregisterButton(this);
                    }
                    if (value)
                    {
                        value.RegisterButton(this);
                    }
                }
                group = value;
            }
        }

        /// <summary>
        /// 按钮的开关状态发生改变。
        /// </summary>
        /// <remarks>除非手动设置 <see cref="IsOn"/> 或者调用 <see cref="SetIsOnWithoutNotify"/>，否则不会在 <see cref="Interactable"/> 为 <see langword="false"/> 时引发。</remarks>
        public event ToggledEventHandler Toggled;

        /// <summary>
        /// 按钮被点击。
        /// </summary>
        /// <remarks>当 <see cref="Interactable"/> 为 <see langword="false"/> 时仍然能够引发。</remarks>
        public event ClickedEventHandler Clicked;

        /// <summary>
        /// 按钮被双击。
        /// </summary>
        /// <remarks>当 <see cref="Interactable"/> 为 <see langword="false"/> 时仍然能够引发。</remarks>
        public event ClickedEventHandler DoubleClicked;

        /// <summary>
        /// 按钮的状态、开关状态或者可交互状态发生更改。
        /// </summary>
        public event UpdatedEventHandler Updated;

        private void OnToggled()
        {
            Toggled?.Invoke(this);
        }

        private void OnClicked(PointerEventData eventData)
        {
            Clicked?.Invoke(this, eventData);
        }

        private void OnDoubleClicked(PointerEventData eventData)
        {
            DoubleClicked?.Invoke(this, eventData);
        }

        private void OnUpdated()
        {
            Updated?.Invoke(this);
        }

        private IEnumerator DelaySingleClick(PointerEventData eventData)
        {
            var clickDelayTimer = 0f;
            do
            {
                yield return null;
                clickDelayTimer += Time.unscaledDeltaTime;
            } while (clickDelayTimer < UnityUtility.ClickDelayTime);
            _delaySingleClickCoroutine = null;
            if (interactable)
            {
                SetIsOn(!isOn, true);
            }
            if (!this)
            {
                yield break;
            }
            OnClicked(eventData);
        }

        private void ReleaseDelaySingleClickCoroutine()
        {
            if (_delaySingleClickCoroutine is null)
            {
                return;
            }
            StopCoroutine(_delaySingleClickCoroutine);
            _delaySingleClickCoroutine = null;
        }

        /// <summary>
        /// 开启或关闭按钮，但不要引发 <see cref="Toggled"/>。
        /// </summary>
        /// <param name="value"><see langword="true"/> 表示开启，<see langword="false"/> 表示关闭。</param>
        public void SetIsOnWithoutNotify(bool value)
        {
            SetIsOn(value, false);
        }

        private void SetIsOn(bool value, bool raiseEvent)
        {
            if (isOn == value)
            {
                return;
            }
            isOn = value;
            if (raiseEvent)
            {
                OnToggled();
            }
            if (!this)
            {
                return;
            }
            OnUpdated();
        }

        private EnhancedButtonState GetState()
        {
            return _pointerDown && _statusCanBePressed
                       ? EnhancedButtonState.Pressed
                       : _pointerInside
                           ? EnhancedButtonState.Hovered
                           : EnhancedButtonState.Default;
        }

        private void UpdateState()
        {
            var state = GetState();
            if (_state == state)
            {
                return;
            }
            _state = state;
            OnUpdated();
        }

        /// <summary>
        /// 引发 <see cref="Updated"/>。
        /// </summary>
        public void Refresh()
        {
            OnUpdated();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            UpdateState();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _pointerInside             = false;
            _statusCanBePressed        = false;
            eventData.eligibleForClick = false;
            UpdateState();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _pointerDown        = true;
            _statusCanBePressed = true;
            UpdateState();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _pointerDown = false;
            UpdateState();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    if (!doubleClick)
                    {
                        if (interactable)
                        {
                            SetIsOn(!isOn, true);
                        }
                        if (!this)
                        {
                            break;
                        }
                        OnClicked(eventData);
                    }
                    else
                    {
                        ReleaseDelaySingleClickCoroutine();
                        switch (eventData.clickCount)
                        {
                            case 1:
                                _delaySingleClickCoroutine = StartCoroutine(DelaySingleClick(eventData));
                                break;
                            case 2:
                                OnDoubleClicked(eventData);
                                break;
                        }
                    }
                    break;
                case PointerEventData.InputButton.Right:
                    if (rightClick)
                    {
                        OnClicked(eventData);
                    }
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            _pointerInside = false;
            _pointerDown   = false;
            if (group)
            {
                group.RegisterButton(this);
            }
            UpdateState();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            _pointerInside             = false;
            _pointerDown               = false;
            _delaySingleClickCoroutine = null;
            UpdateState();
            if (group)
            {
                group.UnregisterButton(this);
            }
            base.OnDisable();
        }

        /// <summary>
        /// 定义几个颜色。
        /// </summary>
        [Serializable]
        public struct ColorBlock
        {
            internal const float DefaultMultiplier = 0.8f;

            internal const float PressedMultiplier = 0.6f;

            internal const float NonInteractableMultiplier = 0.5f;

            internal const float NonInteractableDefaultMultiplier = 0.9f;

            internal const float NonInteractablePressedMultiplier = 0.8f;

            internal static readonly Color DefaultDefaultColor;

            internal static readonly Color DefaultHoveredColor;

            internal static readonly Color DefaultPressedColor;

            internal static readonly Color DefaultNonInteractableDefaultColor;

            internal static readonly Color DefaultNonInteractableHoveredColor;

            internal static readonly Color DefaultNonInteractablePressedColor;

            static ColorBlock()
            {
                // @formatter:max_line_length 10000

                var defaultHoveredColor = Color.white;

                DefaultDefaultColor = RgbMultiplied(defaultHoveredColor, DefaultMultiplier);
                DefaultHoveredColor = defaultHoveredColor;
                DefaultPressedColor = RgbMultiplied(defaultHoveredColor, PressedMultiplier);

                var defaultNonInteractableHoveredColor = RgbaMultiplied(defaultHoveredColor, NonInteractableMultiplier);

                DefaultNonInteractableDefaultColor = RgbMultiplied(defaultNonInteractableHoveredColor, NonInteractableDefaultMultiplier);
                DefaultNonInteractableHoveredColor = defaultNonInteractableHoveredColor;
                DefaultNonInteractablePressedColor = RgbMultiplied(defaultNonInteractableHoveredColor, NonInteractablePressedMultiplier);

                // @formatter:max_line_length restore
            }

            internal static Color RgbMultiplied(Color color, float multiplier)
            {
                var r = color.r * multiplier;
                var g = color.g * multiplier;
                var b = color.b * multiplier;
                var a = color.a;
                return new Color(r, g, b, a);
            }

            internal static Color RgbaMultiplied(Color color, float multiplier)
            {
                var r = color.r * multiplier;
                var g = color.g * multiplier;
                var b = color.b * multiplier;
                var a = color.a * multiplier;
                return new Color(r, g, b, a);
            }

            /// <summary>
            /// 默认颜色。
            /// </summary>
            public Color defaultColor;

            /// <summary>
            /// 悬停颜色。
            /// </summary>
            public Color hoveredColor;

            /// <summary>
            /// 按下颜色。
            /// </summary>
            public Color pressedColor;

            /// <summary>
            /// 不可交互时默认颜色。
            /// </summary>
            public Color nonInteractableDefaultColor;

            /// <summary>
            /// 不可交互时悬停颜色。
            /// </summary>
            public Color nonInteractableHoveredColor;

            /// <summary>
            /// 不可交互时按下颜色。
            /// </summary>
            public Color nonInteractablePressedColor;

            /// <summary>
            /// 根据按钮的状态（<see cref="EnhancedButton.State"/>）、是否可交互（<see cref="EnhancedButton.Interactable"/>）获取颜色。
            /// </summary>
            /// <param name="button">按钮。</param>
            public Color GetColor(EnhancedButton button)
            {
                if (!button)
                {
                    return Color.clear;
                }
                return (button.State, button.interactable) switch
                {
                    (EnhancedButtonState.Default, true)  => defaultColor,
                    (EnhancedButtonState.Hovered, true)  => hoveredColor,
                    (EnhancedButtonState.Pressed, true)  => pressedColor,
                    (EnhancedButtonState.Default, false) => nonInteractableDefaultColor,
                    (EnhancedButtonState.Hovered, false) => nonInteractableHoveredColor,
                    (EnhancedButtonState.Pressed, false) => nonInteractablePressedColor,
                    _                                    => Color.clear
                };
            }
        }
    }
}
