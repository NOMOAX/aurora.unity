using System;
using System.Linq;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    /// <summary>
    /// Draws the editor view for the <see cref="EnhancedButton"/> class.
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
            _interactableContent = new GUIContent("Interactable", "Whether it is interactable");
            _isOnContent         = new GUIContent("IsOn",         "On/Off");
            _buttonGroupContent  = new GUIContent("Group",        "Group");
            _rightClickContent   = new GUIContent("Right Click",  "Whether right-click is allowed");
            _doubleClickContent  = new GUIContent("Double Click", "Whether left double-click is allowed");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GeneralInformationGUI();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws basic information.
        /// </summary>
        protected void GeneralInformationGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var rect = EditorGUI.PrefixLabel(
                    EditorGUILayout.GetControlRect(),
                    EditorGUIUtility.TrTempContent(nameof(EnhancedButton.State))
                );
                EditorGUI.LabelField(rect, ((EnhancedButton)target).State.ToString());
            }

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

        // @formatter:max_line_length 10000

        [CustomPropertyDrawer(typeof(EnhancedButton.ColorBlock), true)]
        internal sealed class ColorBlockDrawer : PropertyDrawer
        {
            private static readonly GenericMenu.MenuFunction2 Reset = state =>
            {
                var property = (SerializedProperty)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                defaultColor.colorValue                = EnhancedButton.ColorBlock.DefaultDefaultColor;
                hoveredColor.colorValue                = EnhancedButton.ColorBlock.DefaultHoveredColor;
                pressedColor.colorValue                = EnhancedButton.ColorBlock.DefaultPressedColor;
                nonInteractableDefaultColor.colorValue = EnhancedButton.ColorBlock.DefaultNonInteractableDefaultColor;
                nonInteractableHoveredColor.colorValue = EnhancedButton.ColorBlock.DefaultNonInteractableHoveredColor;
                nonInteractablePressedColor.colorValue = EnhancedButton.ColorBlock.DefaultNonInteractablePressedColor;

                property.serializedObject.ApplyModifiedProperties();
            };

            private static readonly GenericMenu.MenuFunction2 Copy = state =>
            {
                var property = (SerializedProperty)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                var colorBlock = new EnhancedButton.ColorBlock
                {
                    defaultColor                = defaultColor.colorValue,
                    hoveredColor                = hoveredColor.colorValue,
                    pressedColor                = pressedColor.colorValue,
                    nonInteractableDefaultColor = nonInteractableDefaultColor.colorValue,
                    nonInteractableHoveredColor = nonInteractableHoveredColor.colorValue,
                    nonInteractablePressedColor = nonInteractablePressedColor.colorValue
                };

                WriteToClipboard(colorBlock);
            };

            private static readonly GenericMenu.MenuFunction2 Paste = state =>
            {
                var (property, colorBlock) = (Tuple<SerializedProperty, EnhancedButton.ColorBlock>)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                defaultColor.colorValue                = colorBlock.defaultColor;
                hoveredColor.colorValue                = colorBlock.hoveredColor;
                pressedColor.colorValue                = colorBlock.pressedColor;
                nonInteractableDefaultColor.colorValue = colorBlock.nonInteractableDefaultColor;
                nonInteractableHoveredColor.colorValue = colorBlock.nonInteractableHoveredColor;
                nonInteractablePressedColor.colorValue = colorBlock.nonInteractablePressedColor;

                property.serializedObject.ApplyModifiedProperties();
            };

            private static readonly GenericMenu.MenuFunction2 GenerateColors = state =>
            {
                var property = (SerializedProperty)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                var hoveredColorValue = hoveredColor.colorValue;

                defaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.DefaultMultiplier);
                pressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.PressedMultiplier);

                var nonInteractableHoveredColorValue = EnhancedButton.ColorBlock.RgbaMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.NonInteractableMultiplier);

                nonInteractableDefaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractableDefaultMultiplier);
                nonInteractableHoveredColor.colorValue = nonInteractableHoveredColorValue;
                nonInteractablePressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractablePressedMultiplier);

                property.serializedObject.ApplyModifiedProperties();
            };

            private static readonly GenericMenu.MenuFunction2 GenerateColorsWithGrayscale = state =>
            {
                var property = (SerializedProperty)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                var hoveredColorValue = hoveredColor.colorValue;

                defaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.DefaultMultiplier);
                pressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.PressedMultiplier);

                var nonInteractableHoveredColorValue = MakeGrayscale(EnhancedButton.ColorBlock.RgbaMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.NonInteractableMultiplier));

                nonInteractableDefaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractableDefaultMultiplier);
                nonInteractableHoveredColor.colorValue = nonInteractableHoveredColorValue;
                nonInteractablePressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractablePressedMultiplier);

                property.serializedObject.ApplyModifiedProperties();

                static Color MakeGrayscale(Color color)
                {
                    var grayscale = color.grayscale;
                    return new Color(grayscale, grayscale, grayscale, color.a);
                }
            };

            private static readonly GenericMenu.MenuFunction2 GenerateInteractableColors = state =>
            {
                var property = (SerializedProperty)state;

                var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));

                var hoveredColorValue = hoveredColor.colorValue;

                defaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.DefaultMultiplier);
                pressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(hoveredColorValue, EnhancedButton.ColorBlock.PressedMultiplier);

                property.serializedObject.ApplyModifiedProperties();
            };

            private static readonly GenericMenu.MenuFunction2 GenerateNonInteractableColors = state =>
            {
                var property = (SerializedProperty)state;

                var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                var nonInteractableHoveredColorValue = nonInteractableHoveredColor.colorValue;

                nonInteractableDefaultColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractableDefaultMultiplier);
                nonInteractablePressedColor.colorValue = EnhancedButton.ColorBlock.RgbMultiplied(nonInteractableHoveredColorValue, EnhancedButton.ColorBlock.NonInteractablePressedMultiplier);

                property.serializedObject.ApplyModifiedProperties();
            };

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                position.height = EditorGUIUtility.singleLineHeight;

                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, EditorGUIUtility.TrTempContent(property.displayName));

                // Right-clicking the Foldout row shows a context menu implementing several complex features
                if (GUI.Button(position, GUIContent.none, GUIStyle.none) && Event.current.button == 1)
                {
                    var genericMenu = new GenericMenu();
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Reset"), false, Reset, property);
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Copy"), false, Copy, property);
                    if (TryReadFromClipboard(out var colorBlock))
                    {
                        genericMenu.AddItem(EditorGUIUtility.TrTextContent("Paste"), false, Paste, Tuple.Create(property, colorBlock));
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(EditorGUIUtility.TrTextContent("Paste"), false);
                    }
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Generate Colors/Default"), false, GenerateColors, property);
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Generate Colors/Grayscale for Non-Interactable Colors"), false, GenerateColorsWithGrayscale, property);
                    genericMenu.AddSeparator("Generate Colors");
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Generate Colors/Interactable Colors Only"), false, GenerateInteractableColors, property);
                    genericMenu.AddItem(EditorGUIUtility.TrTextContent("Generate Colors/Non-Interactable Colors Only"), false, GenerateNonInteractableColors, property);
                    genericMenu.ShowAsContext();
                }

                if (property.isExpanded)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var defaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.defaultColor));
                        var hoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.hoveredColor));
                        var pressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.pressedColor));
                        var nonInteractableDefaultColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableDefaultColor));
                        var nonInteractableHoveredColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractableHoveredColor));
                        var nonInteractablePressedColor = property.FindPropertyRelative(nameof(EnhancedButton.ColorBlock.nonInteractablePressedColor));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, defaultColor, EditorGUIUtility.TrTempContent("Default"));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, hoveredColor, EditorGUIUtility.TrTempContent("Hovered"));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, pressedColor, EditorGUIUtility.TrTempContent("Pressed"));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, nonInteractableDefaultColor, EditorGUIUtility.TrTempContent("Non-Interactable Default"));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, nonInteractableHoveredColor, EditorGUIUtility.TrTempContent("Non-Interactable Hovered"));

                        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(position, nonInteractablePressedColor, EditorGUIUtility.TrTempContent("Non-Interactable Pressed"));
                    }
                }
            }

            private static void WriteToClipboard(EnhancedButton.ColorBlock colorBlock)
            {
                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(colorBlock);
            }

            private static bool TryReadFromClipboard(out EnhancedButton.ColorBlock colorBlock)
            {
                var clipboard = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboard))
                {
                    colorBlock = default;
                    return false;
                }
                try
                {
                    colorBlock = JsonUtility.FromJson<EnhancedButton.ColorBlock>(clipboard);
                    return true;
                }
                catch (Exception)
                {
                    colorBlock = default;
                    return false;
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var lineCount = 1;
                if (property.isExpanded)
                {
                    lineCount += 6; // 6 serialized properties
                }
                return lineCount * EditorGUIUtility.singleLineHeight + (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
            }
        }

        // @formatter:max_line_length restore
    }
}
