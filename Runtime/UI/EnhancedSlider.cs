using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A slider.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class EnhancedSlider : UIBehaviour,
                                         ICanvasElement,
                                         IPointerDownHandler,
                                         IPointerUpHandler,
                                         IDragHandler,
                                         IInitializePotentialDragHandler
    {
        /// <summary>
        /// Represents the method executed when the slider's value changes.
        /// </summary>
        public delegate void ValueChangedEventHandler(EnhancedSlider slider, float value, float oldValue);

        [SerializeField]
        private bool interactable = true;

        [SerializeField]
        private RectTransform fill;

        [SerializeField]
        private RectTransform handle;

        [SerializeField]
        private Slider.Direction direction;

        [SerializeField]
        [Range(0, 1)]
        private float value;

        private Vector2 _offset;

        private bool _isOperating;

        private RectTransform _fillParent;

        private RectTransform _handleParent;

        private DrivenRectTransformTracker _tracker;

        private bool _delayUpdateVisuals;

        /// <summary>
        /// Whether it is interactable.
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
                if (_isOperating)
                {
                    _isOperating = false;
                    OnOperationEnded();
                }
            }
        }

        /// <summary>
        /// The fill.
        /// </summary>
        public RectTransform Fill
        {
            get => fill;
            set
            {
                if (fill == value)
                {
                    return;
                }
                fill = value;
                UpdateCachedReferences();
                UpdateVisuals();
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        public RectTransform Handle
        {
            get => handle;
            set
            {
                if (handle == value)
                {
                    return;
                }
                handle = value;
                UpdateCachedReferences();
                UpdateVisuals();
            }
        }

        /// <summary>
        /// The direction.
        /// </summary>
        public Slider.Direction Direction
        {
            get => direction;
            set
            {
                if (direction.Equals(value))
                {
                    return;
                }
                direction = value;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// The value.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number.</exception>
        public float Value
        {
            get => value;
            set
            {
                if (value is float.NaN)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                value = Mathf.Clamp01(value);
                SetValue(value, true);
            }
        }

        /// <summary>
        /// Whether it is currently being operated.
        /// </summary>
        public bool IsOperating => _isOperating;

        private bool MayDrag => IsActive() && interactable;

        private RectTransform.Axis Axis => (RectTransform.Axis)((int)direction / 2);

        private bool ReverseValue => (int)direction % 2 != 0;

        /// <summary>
        /// The value changed.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// The operation is about to begin.
        /// </summary>
        public event Action<EnhancedSlider> OperationBeginning;

        /// <summary>
        /// The operation ended.
        /// </summary>
        public event Action<EnhancedSlider> OperationEnded;

        private void OnValueChanged(float oldValue)
        {
            ValueChanged?.Invoke(this, value, oldValue);
        }

        private void OnOperationBeginning()
        {
            OperationBeginning?.Invoke(this);
        }

        private void OnOperationEnded()
        {
            OperationEnded?.Invoke(this);
        }

        private void UpdateDrag(PointerEventData eventData)
        {
            RectTransform rectTransform;
            if (_handleParent)
            {
                rectTransform = _handleParent;
            }
            else if (_fillParent)
            {
                rectTransform = _fillParent;
            }
            else
            {
                return;
            }
            var axisInt           = (int)Axis;
            var rectTransformSize = rectTransform.rect.size[axisInt];
            if (rectTransformSize <= 0)
            {
                return;
            }
            if (!UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint
                ))
            {
                return;
            }
            localPoint -= rectTransform.rect.position;
            var v = Mathf.Clamp01((localPoint - _offset)[axisInt] / rectTransformSize);
            if (ReverseValue)
            {
                v = 1 - v;
            }
            SetValue(v, true);
        }

        /// <summary>
        /// Sets the value without raising the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="v">The value.</param>
        /// <exception cref="ArgumentException"><paramref name="v"/> is <see cref="float.NaN"/>.</exception>
        public void SetValueWithoutNotify(float v)
        {
            if (v is float.NaN)
            {
                throw new ArgumentException(null, nameof(v));
            }
            v = Mathf.Clamp01(v);
            SetValue(v, false);
        }

        private void SetValue(float v, bool sendCallback)
        {
            if (value.Equals(v))
            {
                return;
            }
            var oldValue = value;
            value = v;
            UpdateVisuals();
            if (sendCallback)
            {
                OnValueChanged(oldValue);
            }
        }

        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateCachedReferences();
            }
#endif
            _tracker.Clear();
            var reverseValue = ReverseValue;
            var axisInt      = (int)Axis;
            if (_fillParent)
            {
                _tracker.Add(this, fill, DrivenTransformProperties.Anchors);
                var anchorMin = Vector2.zero;
                var anchorMax = Vector2.one;
                if (reverseValue)
                {
                    anchorMin[axisInt] = 1 - value;
                }
                else
                {
                    anchorMax[axisInt] = value;
                }
                fill.anchorMin = anchorMin;
                fill.anchorMax = anchorMax;
            }
            if (_handleParent)
            {
                _tracker.Add(this, handle, DrivenTransformProperties.Anchors);
                var anchorMin                           = Vector2.zero;
                var anchorMax                           = Vector2.one;
                anchorMin[axisInt] = anchorMax[axisInt] = reverseValue ? 1 - value : value;
                handle.anchorMin   = anchorMin;
                handle.anchorMax   = anchorMax;
            }
        }

        private void UpdateCachedReferences()
        {
            if (fill && fill != transform)
            {
                _fillParent = fill.parent as RectTransform;
            }
            else
            {
                fill        = null;
                _fillParent = null;
            }
            if (handle && handle != transform)
            {
                _handleParent = handle.parent as RectTransform;
            }
            else
            {
                handle        = null;
                _handleParent = null;
            }
        }

        private void HandleDelayUpdateVisuals()
        {
            if (!_delayUpdateVisuals)
            {
                return;
            }
            _delayUpdateVisuals = false;
            SetValue(value, false);
            UpdateVisuals();
        }

        void ICanvasElement.Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
            {
                OnValueChanged(value);
            }
#endif
        }

        void ICanvasElement.LayoutComplete()
        {
        }

        void ICanvasElement.GraphicUpdateComplete()
        {
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (!MayDrag)
            {
                return;
            }
            OnOperationBeginning();
            if (!MayDrag)
            {
                OnOperationEnded();
                return;
            }
            _isOperating = true;
            _offset      = Vector2.zero;
            if (_handleParent && UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(
                    handle,
                    eventData.pointerPressRaycast.screenPosition,
                    eventData.enterEventCamera
                ))
            {
                UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    handle,
                    eventData.pointerPressRaycast.screenPosition,
                    eventData.pressEventCamera,
                    out _offset
                );
            }
            else
            {
                UpdateDrag(eventData);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (!MayDrag)
            {
                return;
            }
            if (!_isOperating)
            {
                return;
            }
            _isOperating = false;
            OnOperationEnded();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (!MayDrag)
            {
                return;
            }
            if (!_isOperating)
            {
                return;
            }
            UpdateDrag(eventData);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (IsActive())
            {
                UpdateCachedReferences();
                _delayUpdateVisuals = true;
            }
            if (!PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
            {
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            }
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            SetValue(value, false);
            UpdateVisuals();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            if (_isOperating)
            {
                _isOperating = false;
                OnOperationEnded();
            }
            base.OnDisable();
        }

        private void Update()
        {
            HandleDelayUpdateVisuals();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (IsActive())
            {
                UpdateVisuals();
            }
        }
    }
}
