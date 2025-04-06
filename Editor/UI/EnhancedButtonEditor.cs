using System.Linq;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    /// <summary>
    /// 为 <see cref="EnhancedButton"/> 类绘制编辑器视图。
    /// </summary>
    [CustomEditor(typeof(EnhancedButton))]
    [CanEditMultipleObjects]
    public class EnhancedButtonEditor : Editor
    {
        private SerializedProperty _interactable;

        private SerializedProperty _isOn;

        private SerializedProperty _buttonGroup;

        private SerializedProperty _rightClick;

        private SerializedProperty _doubleClick;

        private GUIContent _interactableContent;

        private GUIContent _isOnContent;

        private GUIContent _buttonGroupContent;

        private GUIContent _rightClickContent;

        private GUIContent _doubleClickContent;

        private const string InteractableName = nameof(EnhancedButton.interactable);

        private const string IsOnName = nameof(EnhancedButton.isOn);

        private const string GroupName = nameof(EnhancedButton.group);

        private const string RightClickName = nameof(EnhancedButton.rightClick);

        private const string DoubleClickName = nameof(EnhancedButton.doubleClick);

        private void OnEnable()
        {
            _interactable        = serializedObject.FindProperty(InteractableName);
            _isOn                = serializedObject.FindProperty(IsOnName);
            _buttonGroup         = serializedObject.FindProperty(GroupName);
            _rightClick          = serializedObject.FindProperty(RightClickName);
            _doubleClick         = serializedObject.FindProperty(DoubleClickName);
            _interactableContent = new GUIContent("Interactable", "是否可交互");
            _isOnContent         = new GUIContent("IsOn",         "开启/关闭");
            _buttonGroupContent  = new GUIContent("Group",        "组");
            _rightClickContent   = new GUIContent("Right Click",  "是否允许右键单击");
            _doubleClickContent  = new GUIContent("Double Click", "是否允许左键双击");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GeneralInformationGUI();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制基本信息。
        /// </summary>
        protected void GeneralInformationGUI()
        {
            EditorGUILayout.PropertyField(_interactable, _interactableContent);
            EditorGUILayout.PropertyField(_isOn,         _isOnContent);
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_buttonGroup, _buttonGroupContent);
                if (changeCheckScope.changed)
                {
                    foreach (var button in targets.Cast<EnhancedButton>())
                    {
                        Undo.RecordObject(button, "Set Button Group");
                        button.Group = _buttonGroup.objectReferenceValue as EnhancedButtonGroup;
                        EditorUtility.SetDirty(button);
                    }
                }
            }
            EditorGUILayout.PropertyField(_rightClick,  _rightClickContent);
            EditorGUILayout.PropertyField(_doubleClick, _doubleClickContent);
        }
    }
}
