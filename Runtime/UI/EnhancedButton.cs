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
        public delegate void ToggledEventHandler(EnhancedButton button, bool isOn);

        /// <summary>
        /// 表示当按钮的状态发生改变时执行的方法。
        /// </summary>
        public delegate void StatusChangedEventHandler(
            EnhancedButton       button,
            EnhancedButtonStatus status,
            EnhancedButtonStatus oldStatus);

        /// <summary>
        /// 表示当按钮的状态或者开关状态发生改变时执行的方法。
        /// </summary>
        public delegate void TransitedEventHandler(EnhancedButton button, EnhancedButtonStatus status, bool isOn);

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

        private EnhancedButtonStatus _status;

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
                UpdateStatus();
            }
        }

        /// <summary>
        /// 获取或设置按钮的开关状态。
        /// </summary>
        public bool IsOn { get => isOn; set => SetIsOn(value, true); }

        /// <summary>
        /// 获取按钮所处于的状态。
        /// </summary>
        public EnhancedButtonStatus Status => GetStatus();

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
                    if (group != null)
                    {
                        group.UnregisterButton(this);
                    }
                    if (value != null)
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
        public event ToggledEventHandler Toggled;

        /// <summary>
        /// 按钮被点击。
        /// </summary>
        public event ClickedEventHandler Clicked;

        /// <summary>
        /// 按钮被双击。
        /// </summary>
        public event ClickedEventHandler DoubleClicked;

        /// <summary>
        /// 按钮的状态发生改变。
        /// </summary>
        public event StatusChangedEventHandler StatusChanged;

        /// <summary>
        /// 按钮的状态或者开关状态发生改变。
        /// </summary>
        public event TransitedEventHandler Transited;

        private void OnToggled(EnhancedButton button, bool buttonIsOn)
        {
            Toggled?.Invoke(button, buttonIsOn);
        }

        private void OnClicked(EnhancedButton button, PointerEventData eventData)
        {
            Clicked?.Invoke(button, eventData);
        }

        private void OnDoubleClicked(EnhancedButton button, PointerEventData eventData)
        {
            DoubleClicked?.Invoke(button, eventData);
        }

        private void OnStatusChanged(EnhancedButton button, EnhancedButtonStatus status, EnhancedButtonStatus oldStatus)
        {
            StatusChanged?.Invoke(button, status, oldStatus);
        }

        private void OnTransited(EnhancedButton button, EnhancedButtonStatus status, bool buttonIsOn)
        {
            Transited?.Invoke(button, status, buttonIsOn);
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
            SetIsOn(!isOn, true);
            OnClicked(this, eventData);
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
        /// 开启或关闭按钮，但不要引发 <see cref="Transited"/>。
        /// </summary>
        /// <param name="value"><see langword="true"/> 表示开启，<see langword="false"/> 表示关闭。</param>
        public void SetIsOnWithoutNotify(bool value)
        {
            SetIsOn(value, false);
        }

        private void SetIsOn(bool value, bool sendCallback)
        {
            if (!interactable)
            {
                return;
            }
            if (isOn == value)
            {
                return;
            }
            isOn = value;
            if (sendCallback)
            {
                OnToggled(this, isOn);
            }
            OnTransited(this, _status, isOn);
        }

        private EnhancedButtonStatus GetStatus()
        {
            if (!isActiveAndEnabled)
            {
                return EnhancedButtonStatus.Inactive;
            }
            if (!interactable)
            {
                return EnhancedButtonStatus.NotInteractable;
            }
            if (_pointerDown && _statusCanBePressed)
            {
                return EnhancedButtonStatus.Pressed;
            }
            return _pointerInside ? EnhancedButtonStatus.Hovered : EnhancedButtonStatus.Default;
        }

        private void UpdateStatus()
        {
            var newStatus = GetStatus();
            if (_status == newStatus)
            {
                return;
            }
            var oldStatus = _status;
            _status = newStatus;
            OnStatusChanged(this, _status, oldStatus);
            OnTransited(this, _status, isOn);
        }

        /// <summary>
        /// 引发 <see cref="Transited"/>。
        /// </summary>
        public void InvokeTransited()
        {
            OnTransited(this, _status, isOn);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            UpdateStatus();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _pointerInside             = false;
            _statusCanBePressed        = false;
            eventData.eligibleForClick = false;
            UpdateStatus();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _pointerDown        = true;
            _statusCanBePressed = true;
            UpdateStatus();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _pointerDown = false;
            UpdateStatus();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    if (!doubleClick)
                    {
                        SetIsOn(!isOn, true);
                        OnClicked(this, eventData);
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
                                OnDoubleClicked(this, eventData);
                                break;
                        }
                    }
                    break;
                case PointerEventData.InputButton.Right:
                    if (rightClick)
                    {
                        OnClicked(this, eventData);
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
            if (group != null)
            {
                group.RegisterButton(this);
            }
            UpdateStatus();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            _pointerInside             = false;
            _pointerDown               = false;
            _delaySingleClickCoroutine = null;
            UpdateStatus();
            if (group != null)
            {
                group.UnregisterButton(this);
            }
            base.OnDisable();
        }

        /// <summary>
        /// 定义几个颜色。
        /// </summary>
        [Serializable]
        public sealed class ColorBlock
        {
            /// <summary>
            /// 默认颜色。
            /// </summary>
            public Color defaultColor = new Color(0.8f, 0.8f, 0.8f);

            /// <summary>
            /// 悬停颜色。
            /// </summary>
            public Color hoveredColor = Color.white;

            /// <summary>
            /// 按下颜色。
            /// </summary>
            public Color pressedColor = new Color(0.6f, 0.6f, 0.6f);

            /// <summary>
            /// 不可交互颜色。
            /// </summary>
            public Color notInteractableColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            /// <summary>
            /// 根据指定的 <see cref="EnhancedButtonStatus"/> 获取颜色。
            /// <list type="table">
            /// <listheader><term>按钮状态</term><description>返回值</description></listheader>
            /// <item><term><see cref="EnhancedButtonStatus.Default"/></term><description><see cref="defaultColor"/></description></item>
            /// <item><term><see cref="EnhancedButtonStatus.Hovered"/></term><description><see cref="hoveredColor"/></description></item>
            /// <item><term><see cref="EnhancedButtonStatus.Pressed"/></term><description><see cref="pressedColor"/></description></item>
            /// <item><term><see cref="EnhancedButtonStatus.NotInteractable"/></term><description><see cref="notInteractableColor"/></description></item>
            /// <item><term>其他值</term><description><see cref="Color.clear"/></description></item>
            /// </list>
            /// </summary>
            /// <param name="status">按钮状态。</param>
            /// <returns>如果有与 <paramref name="status"/> 对应的颜色，则为该颜色，否则为 <see cref="Color.clear"/>。</returns>
            public Color GetColor(EnhancedButtonStatus status)
            {
                return status switch
                {
                    EnhancedButtonStatus.Default         => defaultColor,
                    EnhancedButtonStatus.Hovered         => hoveredColor,
                    EnhancedButtonStatus.Pressed         => pressedColor,
                    EnhancedButtonStatus.NotInteractable => notInteractableColor,
                    _                                    => Color.clear
                };
            }
        }
    }
}
