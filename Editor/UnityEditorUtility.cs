using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Aurora.Diagnostics;
using Aurora.IO;
using Aurora.Pooling;
using Aurora.Unity;
using Aurora.Unity.UI.ViewSystem;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using RectTransformUtility = Aurora.Unity.UI.RectTransformUtility;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// Editor utility set.
    /// </summary>
    public static class UnityEditorUtility
    {
        internal static class MenuItems
        {
            /// <summary>
            /// When the <see cref="MenuItem.priority"/> difference of two adjacent <see cref="MenuItem"/> is greater than or equal to this value, a separator appears.
            /// </summary>
            private const int Separator = 11;

            #region Initialize

            private const string InitializeName = "Initialize";

            private const int InitializePriority = 0;

            [MenuItem(DisplayName + "/" + InitializeName, priority = InitializePriority)]
            private static void Initialize()
            {
                EditorInitialization.Initialize();
            }

            [MenuItem(DisplayName + "/" + InitializeName, true)]
            private static bool ValidateInitialize()
            {
                return !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                       !EditorApplication.isPlayingOrWillChangePlaymode;
            }

            #endregion

            #region Allow Unsafe Code

            private const string AllowUnsafeCodeName = "Allow Unsafe Code";

            private const int AllowUnsafeCodePriority = InitializePriority + Separator;

            [MenuItem(DisplayName + "/" + AllowUnsafeCodeName, priority = AllowUnsafeCodePriority)]
            private static void AllowUnsafeCode()
            {
                PlayerSettings.allowUnsafeCode = !PlayerSettings.allowUnsafeCode;
                AssetDatabase.SaveAssets();
            }

            [MenuItem(DisplayName + "/" + AllowUnsafeCodeName, true)]
            private static bool ValidateAllowUnsafeCode()
            {
                Menu.SetChecked(DisplayName + "/" + AllowUnsafeCodeName, PlayerSettings.allowUnsafeCode);
                return !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                       !EditorApplication.isPlayingOrWillChangePlaymode;
            }

            #endregion

            #region Clear Log Entries

            private const string ClearLogEntriesName = "Clear Log Entries";

            private const int ClearLogEntriesPriority = AllowUnsafeCodePriority + 1;

            private static readonly Action ActionClear = GetActionClear();

            private static Action GetActionClear()
            {
                var logEntriesType = LogEntriesType;
                if (logEntriesType == null)
                {
                    return null;
                }
                var clearMethodInfo = logEntriesType.GetMethod(
                    "Clear",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    CallingConventions.Standard,
                    Type.EmptyTypes,
                    null
                );
                if (clearMethodInfo == null)
                {
                    return null;
                }
                try
                {
                    return (Action)Delegate.CreateDelegate(typeof(Action), clearMethodInfo);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }

            /// <summary>
            /// Removes all log entries from the Console window.
            /// </summary>
            [MenuItem(DisplayName + "/" + ClearLogEntriesName, priority = ClearLogEntriesPriority)]
            private static void ClearLogEntries()
            {
                if (ActionClear == null)
                {
                    Log.E("The UnityEditor.LogEntries.Clear method was not found");
                }
                else
                {
                    ActionClear();
                }
            }

            #endregion

            #region Request Script Compilation

            private const string RequestScriptCompilationName = "Request Script Compilation";

            private const int RequestScriptCompilationPriority = ClearLogEntriesPriority + 1;

            [MenuItem(DisplayName + "/" + RequestScriptCompilationName, priority = RequestScriptCompilationPriority)]
            private static void RequestScriptCompilation()
            {
                CompilationPipeline.RequestScriptCompilation();
            }

            [MenuItem(DisplayName + "/" + RequestScriptCompilationName, true)]
            private static bool ValidateRequestScriptCompilation()
            {
                return !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                       !EditorApplication.isPlayingOrWillChangePlaymode;
            }

            #endregion

            #region Layout

            #region Ping Layout Root

            private const string PingLayoutRootName = "Layout/Ping Layout Root";

            private const int PingLayoutRootPriority = RequestScriptCompilationPriority + 1;

            [MenuItem(DisplayName + "/" + PingLayoutRootName, priority = PingLayoutRootPriority)]
            private static void PingLayoutRoot()
            {
                var rectTransform = (RectTransform)Selection.activeTransform;
                var layoutRoot    = RectTransformUtility.GetLayoutRoot(rectTransform);
                if (!layoutRoot)
                {
                    Log.W("The selected RectTransform does not participate in layout.");
                    return;
                }
                EditorGUIUtility.PingObject(layoutRoot);
            }

            [MenuItem(DisplayName + "/" + PingLayoutRootName, true)]
            private static bool ValidatePingLayoutRoot()
            {
                return Selection.activeTransform is RectTransform;
            }

            #endregion

            #region Mark Layout For Rebuild

            private const string MarkRebuildLayoutName = "Layout/Mark Layout For Rebuild";

            private const int MarkRebuildLayoutPriority = PingLayoutRootPriority + 1;

            [MenuItem(DisplayName + "/" + MarkRebuildLayoutName, priority = MarkRebuildLayoutPriority)]
            private static void MarkLayoutForRebuild()
            {
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)Selection.activeTransform);
            }

            [MenuItem(DisplayName + "/" + MarkRebuildLayoutName, true)]
            private static bool ValidateMarkLayoutForRebuild()
            {
                return Selection.activeTransform is RectTransform;
            }

            #endregion

            #region Force Rebuild Layout Immediate

            private const string RebuildLayoutName = "Layout/Force Rebuild Layout Immediate";

            private const int RebuildLayoutPriority = MarkRebuildLayoutPriority + 1;

            [MenuItem(DisplayName + "/" + RebuildLayoutName, priority = RebuildLayoutPriority)]
            private static void ForceRebuildLayoutImmediate()
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Selection.activeTransform);
            }

            [MenuItem(DisplayName + "/" + RebuildLayoutName, true)]
            private static bool ValidateForceRebuildLayoutImmediate()
            {
                return Selection.activeTransform is RectTransform;
            }

            #endregion

            #endregion

            #region Log Graphic Raycast Target

            private const string LogGraphicRaycastTargetName = "Log Graphic Raycast Target";

            private const int LogGraphicRaycastTargetPriority = RebuildLayoutPriority + 1;

            [MenuItem(DisplayName + "/" + LogGraphicRaycastTargetName, priority = LogGraphicRaycastTargetPriority)]
            private static void LogGraphicRaycastTarget()
            {
                foreach (var graphic in Selection.GetFiltered<Graphic>(SelectionMode.Unfiltered))
                {
                    var stringBuilder = PredefinedPools.StringBuilder.Get();
                    try
                    {
                        stringBuilder.Append(graphic.name);
                        stringBuilder.Append(' ');
                        stringBuilder.Append('(');
                        stringBuilder.Append(graphic.GetType().Name);
                        stringBuilder.Append(')');
                        stringBuilder.Append(' ');
                        stringBuilder.Append(nameof(Graphic.raycastTarget));
                        stringBuilder.Append(' ');
                        stringBuilder.Append('=');
                        stringBuilder.Append(' ');
                        stringBuilder.Append(graphic.raycastTarget);

                        Debug.Log(stringBuilder.ToString());
                    }
                    finally
                    {
                        PredefinedPools.StringBuilder.Return(stringBuilder);
                    }
                }
            }

            [MenuItem(DisplayName + "/" + LogGraphicRaycastTargetName, true)]
            private static bool ValidateLogGraphicRaycastTarget()
            {
                return Selection.GetFiltered<Graphic>(SelectionMode.Unfiltered).Length > 0;
            }

            #endregion

            #region Log RectTransform

            private const string LogRectTransformName = "Log RectTransform";

            private const int LogRectTransformPriority = LogGraphicRaycastTargetPriority + 1;

            [MenuItem(DisplayName + "/" + LogRectTransformName, priority = LogRectTransformPriority)]
            private static void LogRectTransform()
            {
                foreach (var rectTransform in Selection.GetFiltered<RectTransform>(SelectionMode.Unfiltered))
                {
                    Debug.Log(RectTransformUtility.GetLogMessage(rectTransform));
                }
            }

            [MenuItem(DisplayName + "/" + LogRectTransformName, true)]
            private static bool ValidateLogRectTransform()
            {
                return Selection.GetFiltered<RectTransform>(SelectionMode.Unfiltered).Length > 0;
            }

            #endregion

            #region Optimize Object Name

            private const string OptimizeObjectNameName = "Optimize Object Name";

            private const int OptimizeObjectNamePriority = LogRectTransformPriority + 1;

            [MenuItem(DisplayName + "/" + OptimizeObjectNameName, priority = OptimizeObjectNamePriority)]
            private static void OptimizeObjectName()
            {
                foreach (var @object in Selection.objects)
                {
                    if (!@object)
                    {
                        continue;
                    }
                    UnityUtility.OptimizeName(@object);
                }
            }

            [MenuItem(DisplayName + "/" + OptimizeObjectNameName, true)]
            private static bool ValidateOptimizeObjectsNames()
            {
                return Selection.objects.Length > 0;
            }

            #endregion

            #region Clipboard

            #region Convert Path to GUID

            private const string ConvertPathToGuidName = "Clipboard/Convert Path to GUID";

            private const int ConvertPathToGuidPriority = OptimizeObjectNamePriority + 1;

            [MenuItem(DisplayName + "/" + ConvertPathToGuidName, priority = ConvertPathToGuidPriority)]
            private static void ConvertPathToGuid()
            {
                var path = GUIUtility.systemCopyBuffer;
                var guid = AssetDatabase.GUIDFromAssetPath(path);
                GUIUtility.systemCopyBuffer = guid.ToString();
            }

            [MenuItem(DisplayName + "/" + ConvertPathToGuidName, true)]
            private static bool ValidateConvertPathToGuid()
            {
                if (EditorApplication.isCompiling)
                {
                    return false;
                }
                var path = GUIUtility.systemCopyBuffer;
                var guid = AssetDatabase.GUIDFromAssetPath(path);
                return IsGuidValid(guid);
            }

            #endregion

            #region Ping Path

            private const string PingPathName = "Clipboard/Ping Path";

            private const int PingPathPriority = ConvertPathToGuidPriority + 1;

            [MenuItem(DisplayName + "/" + PingPathName, priority = PingPathPriority)]
            private static void PingPath()
            {
                var path  = GUIUtility.systemCopyBuffer;
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(asset);
            }

            [MenuItem(DisplayName + "/" + PingPathName, true)]
            private static bool ValidatePingPath()
            {
                if (EditorApplication.isCompiling)
                {
                    return false;
                }
                var path = GUIUtility.systemCopyBuffer;
                var guid = AssetDatabase.GUIDFromAssetPath(path);
                return IsGuidValid(guid);
            }

            #endregion

            #region Convert GUID to Path

            private const string ConvertGuidToPathName = "Clipboard/Convert GUID to Path";

            private const int ConvertGuidToPathPriority = PingPathPriority + 1;

            [MenuItem(DisplayName + "/" + ConvertGuidToPathName, priority = ConvertGuidToPathPriority)]
            private static void ConvertGuidToPath()
            {
                var guid = GUIUtility.systemCopyBuffer;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GUIUtility.systemCopyBuffer = path;
            }

            [MenuItem(DisplayName + "/" + ConvertGuidToPathName, true)]
            private static bool ValidateConvertGuidToPath()
            {
                if (EditorApplication.isCompiling)
                {
                    return false;
                }
                var guid = GUIUtility.systemCopyBuffer;
                if (!IsGuidValid(guid))
                {
                    return false;
                }
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return path.Length > 0;
            }

            #endregion

            #region Ping GUID

            private const string PingGuidName = "Clipboard/Ping GUID";

            private const int PingGuidPriority = ConvertGuidToPathPriority + 1;

            [MenuItem(DisplayName + "/" + PingGuidName, priority = PingGuidPriority)]
            private static void PingGuid()
            {
                var guid  = GUIUtility.systemCopyBuffer;
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(asset);
            }

            [MenuItem(DisplayName + "/" + PingGuidName, true)]
            private static bool ValidatePingGuid()
            {
                if (EditorApplication.isCompiling)
                {
                    return false;
                }
                var guid = GUIUtility.systemCopyBuffer;
                if (!IsGuidValid(guid))
                {
                    return false;
                }
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return path.Length > 0;
            }

            #endregion

            #endregion

            #region Capture Screenshot to Desktop

            private const string CaptureScreenshotToDesktopName = "Capture Screenshot to Desktop";

            private const int CaptureScreenshotToDesktopPriority = PingGuidPriority + 1;

            [MenuItem(
                DisplayName + "/" + CaptureScreenshotToDesktopName,
                priority = CaptureScreenshotToDesktopPriority
            )]
            private static void CaptureScreenshotToDesktop()
            {
                UnityUtility.BeginCaptureScreenshot(
                    Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                        $"{Application.productName}_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss}.png"
                    )
                );
            }

            [MenuItem(DisplayName + "/" + CaptureScreenshotToDesktopName, true)]
            private static bool ValidateCaptureScreenshotToDesktop()
            {
                return !EditorApplication.isCompiling;
            }

            #endregion

            #region Open Persistent Data Path

            private const string OpenPersistentDataPathName = "Open Persistent Data Path";

            private const int OpenPersistentDataPathPriority = CaptureScreenshotToDesktopPriority + 1;

            [MenuItem(DisplayName + "/" + OpenPersistentDataPathName, priority = OpenPersistentDataPathPriority)]
            private static void OpenPersistentDataPath()
            {
                var persistentDataPath = PathUtility.ReplaceBackslashWithForwardSlash(Application.persistentDataPath);
                Process.Start(persistentDataPath);
            }

            [MenuItem(DisplayName + "/" + OpenPersistentDataPathName, true)]
            private static bool ValidateOpenPersistentDataPath()
            {
                var persistentDataPath = PathUtility.ReplaceBackslashWithForwardSlash(Application.persistentDataPath);
                return Directory.Exists(persistentDataPath);
            }

            #endregion

            #region Validate View Prefabs

            private const string ValidateViewPrefabsName = "Validate View Prefabs";

            private const int ValidateViewPrefabsPriority = OpenPersistentDataPathPriority + 1;

            [MenuItem(DisplayName + "/" + ValidateViewPrefabsName, priority = ValidateViewPrefabsPriority)]
            private static void ValidateViewPrefabs()
            {
                var messages = PredefinedPools<string>.List.Get();
                var prefabs  = PredefinedPools<GameObject>.List.Get();
                try
                {
                    foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
                    {
                        if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        if (!prefab)
                        {
                            continue;
                        }
                        var views = PredefinedPools<View>.List.Get();
                        try
                        {
                            prefab.GetComponentsInChildren(true, views);
                            foreach (var view in views)
                            {
                                if (view.gameObject.activeSelf && view.enabled)
                                {
                                    messages.Add(
                                        $"- {assetPath}: {view.GetType().Name} on '{view.gameObject.name}' is active and enabled"
                                    );
                                    prefabs.Add(prefab);
                                }
                            }
                        }
                        finally
                        {
                            PredefinedPools<View>.List.Return(views);
                        }
                    }
                    var count = messages.Count;
                    if (count == 0)
                    {
                        Debug.Log("All view prefabs satisfy inactive or disabled.");
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"{count} view {EnglishUtility.Pluralize("prefab", "prefabs", count)} with an active and enabled view:"
                        );
                        for (var i = 0; i < count; i++)
                        {
                            Debug.LogWarning(messages[i], prefabs[i]);
                        }
                    }
                }
                finally
                {
                    PredefinedPools<string>.List.Return(messages);
                    PredefinedPools<GameObject>.List.Return(prefabs);
                }
            }

            [MenuItem(DisplayName + "/" + ValidateViewPrefabsName, true)]
            private static bool ValidateValidateViewPrefabs()
            {
                return !EditorApplication.isCompiling && !EditorApplication.isUpdating;
            }

            #endregion
        }

        /// <summary>
        /// Stores the types of the various standard windows in the Unity editor.
        /// </summary>
        public static class EditorWindowTypes
        {
            /// <summary>
            /// The console window type.
            /// </summary>
            public static readonly Type Console = UnityEditorAssembly.GetType("UnityEditor.ConsoleWindow");

            /// <summary>
            /// The game window type.
            /// </summary>
            public static readonly Type Game = UnityEditorAssembly.GetType("UnityEditor.GameView");

            /// <summary>
            /// The hierarchy window type.
            /// </summary>
            public static readonly Type Hierarchy = UnityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

            /// <summary>
            /// The inspector window type.
            /// </summary>
            public static readonly Type Inspector = UnityEditorAssembly.GetType("UnityEditor.InspectorWindow");

            /// <summary>
            /// The project window type.
            /// </summary>
            public static readonly Type Project = UnityEditorAssembly.GetType("UnityEditor.ProjectBrowser");

            /// <summary>
            /// The scene window type.
            /// </summary>
            public static readonly Type Scene = UnityEditorAssembly.GetType("UnityEditor.SceneView");
        }

        internal const string DisplayName = "Aurora Unity";

        /// <summary>
        /// The Unity editor assembly.
        /// </summary>
        public static readonly Assembly UnityEditorAssembly = typeof(EditorApplication).Assembly;

        /// <summary>
        /// The <c>UnityEditor.LogEntries</c> type.
        /// </summary>
        public static readonly Type LogEntriesType = UnityEditorAssembly.GetType("UnityEditor.LogEntries");

        /// <summary>
        /// The project path.
        /// </summary>
        public static readonly string ProjectPath =
            PathUtility.ReplaceBackslashWithForwardSlash(new DirectoryInfo(Application.dataPath).Parent!.FullName);

        private static readonly Regex SymbolRegex = new(@"\A[A-Za-z_][A-Za-z0-9_]*\z", RegexOptions.Compiled);

        /// <summary>
        /// Determines whether the specified serialized property contains child members.
        /// </summary>
        /// <param name="property">The serialized property.</param>
        /// <returns><see langword="true"/> if <paramref name="property"/> contains child members; otherwise <see langword="false"/>.</returns>
        /// <remarks>Can be used with <see cref="EditorGUI.PropertyField(Rect,SerializedProperty,GUIContent,bool)"/> and <see cref="EditorGUI.GetPropertyHeight(SerializedProperty,GUIContent,bool)"/></remarks>
        /// <seealso cref="EditorGUILayout.IsChildrenIncluded"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsChildrenIncluded(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Generic => true,
                SerializedPropertyType.Vector4 => true,
                _                              => false
            };
        }

        /// <summary>
        /// Throws an exception if a preprocessor symbol is invalid.
        /// </summary>
        /// <param name="symbol">The preprocessor symbol.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> is not a valid preprocessor symbol.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfSymbolInvalid(string symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }
            if (SymbolRegex.IsMatch(symbol))
            {
                return;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Determines whether the specified Unity global unique identifier is valid.
        /// </summary>
        /// <param name="guid">The Unity global unique identifier.</param>
        /// <returns><see langword="true"/> if <paramref name="guid"/> is a valid Unity global unique identifier; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuidValid(string guid)
        {
            return GUID.TryParse(guid, out _);
        }

        /// <summary>
        /// Determines whether the specified Unity global unique identifier is valid.
        /// </summary>
        /// <param name="guid">The Unity global unique identifier.</param>
        /// <returns><see langword="true"/> if <paramref name="guid"/> is a valid Unity global unique identifier; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuidValid(GUID guid)
        {
            return !guid.Empty();
        }
    }
}
