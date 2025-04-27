using System;
using System.Text;
using Aurora.Pooling;
using Aurora.Unity;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.UI.ViewSystem;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    internal static class EditorInitialization
    {
        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.AppendLine(
                    $"{nameof(Aurora)}.{nameof(UnityEditor)}.{nameof(EditorInitialization)}.{nameof(Initialize)}();"
                );
                InternalInitialize(stringBuilder);
                Debug.Log(stringBuilder.ToString());
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }

        private static void InternalInitialize(StringBuilder stringBuilder)
        {
            RuntimeInitialization.SetIsSingleThreadEnvironment(false, stringBuilder);
            RuntimeInitialization.SetUnityConsoleLogger(stringBuilder);
            RuntimeInitialization.SetUnityMainThreadId(stringBuilder);
            RuntimeInitialization.SetUnitySynchronizationContext(stringBuilder);
            RuntimeInitialization.SetUnitySynchronizationContextTaskScheduler(stringBuilder);
            RuntimeInitialization.SetIsProSkin(EditorGUIUtility.isProSkin, stringBuilder);
            RuntimeInitialization.PlayerLoopUtilityInitialize(stringBuilder);
            RuntimeInitialization.RegisterPrefabLessViewHandler(stringBuilder);

            EditorApplication.playModeStateChanged -= OnEditorApplicationPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnEditorApplicationPlayModeStateChanged;

            EditorApplication.update -= OnEditorApplicationUpdate;
            EditorApplication.update += OnEditorApplicationUpdate;
        }

        private static void OnEditorApplicationPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    PlayerLoopUtility.Clear();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    RuntimeInitialization.ExitPlayMode(null);
                    PlayerLoopUtility.Clear();
                    View.ClearContainers();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playModeStateChange), playModeStateChange, null);
            }
        }

        private static void OnEditorApplicationUpdate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling)
            {
                PlayerLoopUtility.Run();
            }
        }
    }
}
