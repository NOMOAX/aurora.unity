using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A button.
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
        /// Represents the method executed when a button is clicked.
        /// </summary>
        public delegate void ClickedEventHandler(EnhancedButton button, PointerEventData eventData);

        /// <summary>
        /// Represents the method executed when the button's toggle state changes.
        /// </summary>
        public delegate void ToggledEventHandler(EnhancedButton button);

        /// <summary>
        /// Represents the method executed when the button's state, toggle state, or interactable state changes.
        /// </summary>
        public delegate void UpdatedEventHandler(EnhancedButton button);

        [SerializeField]
        internal bool interactable = true;

        [SerializeField]
        internal bool isOn;

        [SerializeField]
        internal EnhancedButtonGroup group;

        /// <summary>
        /// Whether right-click is allowed.
        /// </summary>
        public bool rightClick;

        /// <summary>
        /// Whether double-click is allowed.
        /// </summary>
        public bool doubleClick;

        private EnhancedButtonState _state;

        private bool _pointerInside;

        private bool _pointerDown;

        private bool _statusCanBePressed;

        private Coroutine _delaySingleClickCoroutine;

        /// <summary>
        /// Gets or sets whether the button is interactable.
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
        /// Gets or sets the button's toggle state.
        /// </summary>
        public bool IsOn { get => isOn; set => SetIsOn(value, true); }

        /// <summary>
        /// Gets the button's state.
        /// </summary>
        public EnhancedButtonState State => GetState();

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <remarks>Note that a button can exist in the group only when the button is <see cref="Behaviour.isActiveAndEnabled"/>.</remarks>
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
        /// The button's toggle state changed.
        /// </summary>
        /// <remarks>Unless <see cref="IsOn"/> is set manually or <see cref="SetIsOnWithoutNotify"/> is called, it is not raised when <see cref="Interactable"/> is <see langword="false"/>.</remarks>
        public event ToggledEventHandler Toggled;

        /// <summary>
        /// The button was clicked.
        /// </summary>
        /// <remarks>Still raised when <see cref="Interactable"/> is <see langword="false"/>.</remarks>
        public event ClickedEventHandler Clicked;

        /// <summary>
        /// The button was double-clicked.
        /// </summary>
        /// <remarks>Still raised when <see cref="Interactable"/> is <see langword="false"/>.</remarks>
        public event ClickedEventHandler DoubleClicked;

        /// <summary>
        /// The button's state, toggle state, or interactable state changed.
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
        /// Turns the button on or off, but does not raise <see cref="Toggled"/>.
        /// </summary>
        /// <param name="value"><see langword="true"/> means on, <see langword="false"/> means off.</param>
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
        /// Raises <see cref="Updated"/>.
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
        /// Defines several colors.
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
            /// The default color.
            /// </summary>
            public Color defaultColor;

            /// <summary>
            /// The hovered color.
            /// </summary>
            public Color hoveredColor;

            /// <summary>
            /// The pressed color.
            /// </summary>
            public Color pressedColor;

            /// <summary>
            /// The non-interactable default color.
            /// </summary>
            public Color nonInteractableDefaultColor;

            /// <summary>
            /// The non-interactable hovered color.
            /// </summary>
            public Color nonInteractableHoveredColor;

            /// <summary>
            /// The non-interactable pressed color.
            /// </summary>
            public Color nonInteractablePressedColor;

            /// <summary>
            /// Gets the color based on the button's state (<see cref="EnhancedButton.State"/>), whether it is interactable (<see cref="EnhancedButton.Interactable"/>).
            /// </summary>
            /// <param name="button">The button.</param>
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
