// @formatter:max_line_length 10000

using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 可传入用户自定义状态的 <see cref="ReorderableList"/>。
    /// </summary>
    /// <remarks>虽然依然没能在此类内部避免闭包，但此类将闭包行为限制在了此类的内部。</remarks>
    public sealed class ReorderableListWithState : ReorderableList
    {
        private HeaderCallbackDelegate _drawHeaderCallback;

        private FooterCallbackDelegate _drawFooterCallback;

        private ElementCallbackDelegate _drawElementCallback;

        private ElementCallbackDelegate _drawElementBackgroundCallback;

        private DrawNoneElementCallback _drawNoneElementCallback;

        private ElementHeightCallbackDelegate _elementHeightCallback;

        private ReorderCallbackDelegateWithDetails _onReorderCallbackWithDetails;

        private ReorderCallbackDelegate _onReorderCallback;

        private SelectCallbackDelegate _onSelectCallback;

        private AddCallbackDelegate _onAddCallback;

        private AddDropdownCallbackDelegate _onAddDropdownCallback;

        private RemoveCallbackDelegate _onRemoveCallback;

        private DragCallbackDelegate _onMouseDragCallback;

        private SelectCallbackDelegate _onMouseUpCallback;

        private CanRemoveCallbackDelegate _onCanRemoveCallback;

        private CanAddCallbackDelegate _onCanAddCallback;

        private ChangedCallbackDelegate _onChangedCallback;

        /// <seealso cref="ReorderableList.drawHeaderCallback"/>
        public new HeaderCallbackDelegate
            // ReSharper disable InconsistentNaming
            drawHeaderCallback
            // ReSharper restore InconsistentNaming
        {
            get => _drawHeaderCallback;
            set
            {
                if (value is null)
                {
                    _drawHeaderCallback     = null;
                    base.drawHeaderCallback = null;
                }
                else
                {
                    _drawHeaderCallback     = value;
                    base.drawHeaderCallback = DrawHeader;

                    void DrawHeader(Rect rect)
                    {
                        value(rect, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.drawFooterCallback"/>
        public new FooterCallbackDelegate
            // ReSharper disable InconsistentNaming
            drawFooterCallback
            // ReSharper restore InconsistentNaming
        {
            get => _drawFooterCallback;
            set
            {
                if (value is null)
                {
                    _drawFooterCallback     = null;
                    base.drawFooterCallback = null;
                }
                else
                {
                    _drawFooterCallback     = value;
                    base.drawFooterCallback = DrawFooter;

                    void DrawFooter(Rect rect)
                    {
                        value(rect, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.drawElementCallback"/>
        public new ElementCallbackDelegate
            // ReSharper disable InconsistentNaming
            drawElementCallback
            // ReSharper restore InconsistentNaming
        {
            get => _drawElementCallback;
            set
            {
                if (value is null)
                {
                    _drawElementCallback     = null;
                    base.drawElementCallback = null;
                }
                else
                {
                    _drawElementCallback     = value;
                    base.drawElementCallback = DrawElement;

                    void DrawElement(Rect rect, int index1, bool isActive, bool isFocused)
                    {
                        value(rect, index1, isActive, isFocused, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.drawElementBackgroundCallback"/>
        public new ElementCallbackDelegate
            // ReSharper disable InconsistentNaming
            drawElementBackgroundCallback
            // ReSharper restore InconsistentNaming
        {
            get => _drawElementBackgroundCallback;
            set
            {
                if (value is null)
                {
                    _drawElementBackgroundCallback     = null;
                    base.drawElementBackgroundCallback = null;
                }
                else
                {
                    _drawElementBackgroundCallback     = value;
                    base.drawElementBackgroundCallback = DrawElementBackground;

                    void DrawElementBackground(Rect rect, int index1, bool isActive, bool isFocused)
                    {
                        value(rect, index1, isActive, isFocused, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.drawNoneElementCallback"/>
        public new DrawNoneElementCallback
            // ReSharper disable InconsistentNaming
            drawNoneElementCallback
            // ReSharper restore InconsistentNaming
        {
            get => _drawNoneElementCallback;
            set
            {
                if (value is null)
                {
                    _drawNoneElementCallback     = null;
                    base.drawNoneElementCallback = null;
                }
                else
                {
                    _drawNoneElementCallback     = value;
                    base.drawNoneElementCallback = DrawNoneElement;

                    void DrawNoneElement(Rect rect)
                    {
                        value(rect, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.elementHeightCallback"/>
        public new ElementHeightCallbackDelegate
            // ReSharper disable InconsistentNaming
            elementHeightCallback
            // ReSharper restore InconsistentNaming
        {
            get => _elementHeightCallback;
            set
            {
                if (value is null)
                {
                    _elementHeightCallback     = null;
                    base.elementHeightCallback = null;
                }
                else
                {
                    _elementHeightCallback     = value;
                    base.elementHeightCallback = ElementHeight;

                    float ElementHeight(int index1)
                    {
                        return value(index1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onReorderCallbackWithDetails"/>
        public new ReorderCallbackDelegateWithDetails
            // ReSharper disable InconsistentNaming
            onReorderCallbackWithDetails
            // ReSharper restore InconsistentNaming
        {
            get => _onReorderCallbackWithDetails;
            set
            {
                if (value is null)
                {
                    _onReorderCallbackWithDetails     = null;
                    base.onReorderCallbackWithDetails = null;
                }
                else
                {
                    _onReorderCallbackWithDetails     = value;
                    base.onReorderCallbackWithDetails = OnReorderWithDetails;

                    void OnReorderWithDetails(ReorderableList list1, int oldIndex, int newIndex)
                    {
                        value((ReorderableListWithState)list1, oldIndex, newIndex, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onReorderCallback"/>
        public new ReorderCallbackDelegate
            // ReSharper disable InconsistentNaming
            onReorderCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onReorderCallback;
            set
            {
                if (value is null)
                {
                    _onReorderCallback     = null;
                    base.onReorderCallback = null;
                }
                else
                {
                    _onReorderCallback     = value;
                    base.onReorderCallback = OnReorder;

                    void OnReorder(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onSelectCallback"/>
        public new SelectCallbackDelegate
            // ReSharper disable InconsistentNaming
            onSelectCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onSelectCallback;
            set
            {
                if (value is null)
                {
                    _onSelectCallback     = null;
                    base.onSelectCallback = null;
                }
                else
                {
                    _onSelectCallback     = value;
                    base.onSelectCallback = OnSelect;

                    void OnSelect(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onAddCallback"/>
        public new AddCallbackDelegate
            // ReSharper disable InconsistentNaming
            onAddCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onAddCallback;
            set
            {
                if (value is null)
                {
                    _onAddCallback     = null;
                    base.onAddCallback = null;
                }
                else
                {
                    _onAddCallback     = value;
                    base.onAddCallback = OnAdd;

                    void OnAdd(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onAddDropdownCallback"/>
        public new AddDropdownCallbackDelegate
            // ReSharper disable InconsistentNaming
            onAddDropdownCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onAddDropdownCallback;
            set
            {
                if (value is null)
                {
                    _onAddDropdownCallback     = null;
                    base.onAddDropdownCallback = null;
                }
                else
                {
                    _onAddDropdownCallback     = value;
                    base.onAddDropdownCallback = OnAddDropdown;

                    void OnAddDropdown(Rect buttonRect, ReorderableList list1)
                    {
                        value(buttonRect, (ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onRemoveCallback"/>
        public new RemoveCallbackDelegate
            // ReSharper disable InconsistentNaming
            onRemoveCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onRemoveCallback;
            set
            {
                if (value is null)
                {
                    _onRemoveCallback     = null;
                    base.onRemoveCallback = null;
                }
                else
                {
                    _onRemoveCallback     = value;
                    base.onRemoveCallback = OnRemove;

                    void OnRemove(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onMouseDragCallback"/>
        public new DragCallbackDelegate
            // ReSharper disable InconsistentNaming
            onMouseDragCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onMouseDragCallback;
            set
            {
                if (value is null)
                {
                    _onMouseDragCallback     = null;
                    base.onMouseDragCallback = null;
                }
                else
                {
                    _onMouseDragCallback     = value;
                    base.onMouseDragCallback = OnMouseDrag;

                    void OnMouseDrag(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onMouseUpCallback"/>
        public new SelectCallbackDelegate
            // ReSharper disable InconsistentNaming
            onMouseUpCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onMouseUpCallback;
            set
            {
                if (value is null)
                {
                    _onMouseUpCallback     = null;
                    base.onMouseUpCallback = null;
                }
                else
                {
                    _onMouseUpCallback     = value;
                    base.onMouseUpCallback = OnMouseUp;

                    void OnMouseUp(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onCanRemoveCallback"/>
        public new CanRemoveCallbackDelegate
            // ReSharper disable InconsistentNaming
            onCanRemoveCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onCanRemoveCallback;
            set
            {
                if (value is null)
                {
                    _onCanRemoveCallback     = null;
                    base.onCanRemoveCallback = null;
                }
                else
                {
                    _onCanRemoveCallback     = value;
                    base.onCanRemoveCallback = OnCanRemove;

                    bool OnCanRemove(ReorderableList list1)
                    {
                        return value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onCanAddCallback"/>
        public new CanAddCallbackDelegate
            // ReSharper disable InconsistentNaming
            onCanAddCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onCanAddCallback;
            set
            {
                if (value is null)
                {
                    _onCanAddCallback     = null;
                    base.onCanAddCallback = null;
                }
                else
                {
                    _onCanAddCallback     = value;
                    base.onCanAddCallback = OnCanAdd;

                    bool OnCanAdd(ReorderableList list1)
                    {
                        return value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        /// <seealso cref="ReorderableList.onChangedCallback"/>
        public new ChangedCallbackDelegate
            // ReSharper disable InconsistentNaming
            onChangedCallback
            // ReSharper restore InconsistentNaming
        {
            get => _onChangedCallback;
            set
            {
                if (value is null)
                {
                    _onChangedCallback     = null;
                    base.onChangedCallback = null;
                }
                else
                {
                    _onChangedCallback     = value;
                    base.onChangedCallback = OnChanged;

                    void OnChanged(ReorderableList list1)
                    {
                        value((ReorderableListWithState)list1, _state);
                    }
                }
            }
        }

        private readonly object _state;

        /// <summary>
        /// 初始化 <see cref="ReorderableListWithState"/> 时，传入的自定义状态。
        /// </summary>
        public object State => _state;

        /// <seealso cref="ReorderableList(IList, Type)"/>
        public ReorderableListWithState(IList elements, Type elementType, object state) : base(elements, elementType)
        {
            _state = state;
        }

        /// <seealso cref="ReorderableList(IList, Type, bool, bool, bool, bool)"/>
        public ReorderableListWithState(IList elements, Type elementType, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton, object state) : base(elements, elementType, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            _state = state;
        }

        /// <seealso cref="ReorderableList(SerializedObject, SerializedProperty)"/>
        public ReorderableListWithState(SerializedObject serializedObject, SerializedProperty elements, object state) : base(serializedObject, elements)
        {
            _state = state;
        }

        /// <seealso cref="ReorderableList(SerializedObject, SerializedProperty, bool, bool, bool, bool)"/>
        public ReorderableListWithState(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton, object state) : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            _state = state;
        }

        /// <seealso cref="ReorderableList.HeaderCallbackDelegate"/>
        public new delegate void HeaderCallbackDelegate(Rect rect, object state);

        /// <seealso cref="ReorderableList.FooterCallbackDelegate"/>
        public new delegate void FooterCallbackDelegate(Rect rect, object state);

        /// <seealso cref="ReorderableList.ElementCallbackDelegate"/>
        public new delegate void ElementCallbackDelegate(Rect rect, int index, bool isActive, bool isFocused, object state);

        /// <seealso cref="ReorderableList.ElementHeightCallbackDelegate"/>
        public new delegate float ElementHeightCallbackDelegate(int index, object state);

        /// <seealso cref="ReorderableList.DrawNoneElementCallback"/>
        public new delegate void DrawNoneElementCallback(Rect rect, object state);

        /// <seealso cref="ReorderableList.ReorderCallbackDelegateWithDetails"/>
        public new delegate void ReorderCallbackDelegateWithDetails(ReorderableListWithState list, int oldIndex, int newIndex, object state);

        /// <seealso cref="ReorderableList.ReorderCallbackDelegate"/>
        public new delegate void ReorderCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.SelectCallbackDelegate"/>
        public new delegate void SelectCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.AddCallbackDelegate"/>
        public new delegate void AddCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.AddDropdownCallbackDelegate"/>
        public new delegate void AddDropdownCallbackDelegate(Rect buttonRect, ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.RemoveCallbackDelegate"/>
        public new delegate void RemoveCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.ChangedCallbackDelegate"/>
        public new delegate void ChangedCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.CanRemoveCallbackDelegate"/>
        public new delegate bool CanRemoveCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.CanAddCallbackDelegate"/>
        public new delegate bool CanAddCallbackDelegate(ReorderableListWithState list, object state);

        /// <seealso cref="ReorderableList.DragCallbackDelegate"/>
        public new delegate void DragCallbackDelegate(ReorderableListWithState list, object state);
    }
}
// @formatter:max_line_length restore
