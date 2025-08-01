using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Aurora.Diagnostics;
using Aurora.Pooling;
using Aurora.Unity;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using RectTransformUtility = Aurora.Unity.UI.RectTransformUtility;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 编辑器工具集。
    /// </summary>
    public static class UnityEditorUtility
    {
        internal static class MenuItems
        {
            /// <summary>
            /// 相邻两个 <see cref="MenuItem"/> 的 <see cref="MenuItem.priority"/> 相差等于或大于这个值时，会出现一条分割线。
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
                    return (Action) Delegate.CreateDelegate(typeof(Action), clearMethodInfo);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }

            /// <summary>
            /// 移除 Console 窗口中的所有日志条目。
            /// </summary>
            [MenuItem(DisplayName + "/" + ClearLogEntriesName, priority = ClearLogEntriesPriority)]
            private static void ClearLogEntries()
            {
                if (ActionClear == null)
                {
                    Log.E("找不到 UnityEditor.LogEntries.Clear 方法");
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
                var rectTransform = (RectTransform) Selection.activeTransform;
                var layoutRoot    = RectTransformUtility.GetLayoutRoot(rectTransform);
                if (!layoutRoot)
                {
                    Log.W("选中的矩形变换不参与布局。");
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
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) Selection.activeTransform);
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
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) Selection.activeTransform);
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
                    System.IO.Path.Combine(
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
                var persistentDataPath = Application.persistentDataPath.ReplaceBackslashWithSlash();
                Process.Start(persistentDataPath);
            }

            [MenuItem(DisplayName + "/" + OpenPersistentDataPathName, true)]
            private static bool ValidateOpenPersistentDataPath()
            {
                var persistentDataPath = Application.persistentDataPath.ReplaceBackslashWithSlash();
                return Directory.Exists(persistentDataPath);
            }

            #endregion
        }

        /// <summary>
        /// 存放 Unity 编辑器中的各个标准窗口的类型。
        /// </summary>
        public static class EditorWindowTypes
        {
            /// <summary>
            /// 控制台窗口类型。
            /// </summary>
            public static readonly Type Console = UnityEditorAssembly.GetType("UnityEditor.ConsoleWindow");

            /// <summary>
            /// 游戏窗口类型。
            /// </summary>
            public static readonly Type Game = UnityEditorAssembly.GetType("UnityEditor.GameView");

            /// <summary>
            /// 层级窗口类型。
            /// </summary>
            public static readonly Type Hierarchy = UnityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

            /// <summary>
            /// 检视器窗口类型。
            /// </summary>
            public static readonly Type Inspector = UnityEditorAssembly.GetType("UnityEditor.InspectorWindow");

            /// <summary>
            /// 项目窗口类型。
            /// </summary>
            public static readonly Type Project = UnityEditorAssembly.GetType("UnityEditor.ProjectBrowser");

            /// <summary>
            /// 场景窗口类型。
            /// </summary>
            public static readonly Type Scene = UnityEditorAssembly.GetType("UnityEditor.SceneView");
        }

        internal const string DisplayName = "Aurora Unity";

        /// <summary>
        /// Unity 编辑器程序集。
        /// </summary>
        public static readonly Assembly UnityEditorAssembly = typeof(EditorApplication).Assembly;

        /// <summary>
        /// <c>UnityEditor.LogEntries</c> 类型。
        /// </summary>
        public static readonly Type LogEntriesType = UnityEditorAssembly.GetType("UnityEditor.LogEntries");

        /// <summary>
        /// 项目路径。
        /// </summary>
        public static readonly string ProjectPath =
            new DirectoryInfo(Application.dataPath).Parent!.FullName.ReplaceBackslashWithSlash();

        /// <summary>
        /// 路径。
        /// </summary>
        public static readonly string Path = PackageInfo.FindForAssembly(typeof(UnityUtility).Assembly).assetPath;

        private static readonly Regex SymbolRegex = new(@"\A[A-Za-z_][A-Za-z0-9_]*\z", RegexOptions.Compiled);

        /// <seealso cref="EditorGUILayout.IsChildrenIncluded"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IncludeChildren(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Generic => true,
                SerializedPropertyType.Vector4 => true,
                _                              => false
            };
        }

        /// <summary>
        /// 如果预编译符号非法，则抛出异常。
        /// </summary>
        /// <param name="symbol">预编译符号。</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> 不是一个合法的预编译符号。</exception>
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
        /// 判断指定的 Unity 全局唯一标识符是否有效。
        /// </summary>
        /// <param name="guid">Unity 全局唯一标识符。</param>
        /// <returns>如果 <paramref name="guid"/> 是有效的 Unity 全局唯一标识符，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuidValid(string guid)
        {
            return GUID.TryParse(guid, out _);
        }

        /// <summary>
        /// 判断指定的 Unity 全局唯一标识符是否有效。
        /// </summary>
        /// <param name="guid">Unity 全局唯一标识符。</param>
        /// <returns>如果 <paramref name="guid"/> 是有效的 Unity 全局唯一标识符，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuidValid(GUID guid)
        {
            return !guid.Empty();
        }
    }
}
