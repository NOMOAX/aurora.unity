using System;
using Aurora.Unity;
using Aurora.Unity.PlayerLoop;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    internal static class EditorInitialization
    {
        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            Debug.Log($"开始执行 {nameof(EditorInitialization)}.{nameof(Initialize)}");
            try
            {
                InternalInitialize();
            }
            finally
            {
                Debug.Log($"结束执行 {nameof(EditorInitialization)}.{nameof(Initialize)}");
            }
        }

        private static void InternalInitialize()
        {
            RuntimeInitialization.SetIsSingleThreadEnvironment(false);
            RuntimeInitialization.SetUnityConsoleLogger();
            RuntimeInitialization.SetUnityMainThreadId();
            RuntimeInitialization.SetUnitySynchronizationContext();
            RuntimeInitialization.SetUnitySynchronizationContextTaskScheduler();
            RuntimeInitialization.SetIsProSkin(EditorGUIUtility.isProSkin);
            RuntimeInitialization.PlayerLoopUtilityInitialize();
            RuntimeInitialization.RegisterPrefabLessViewHandler();

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
                    RuntimeInitialization.SetIsPlaying(false);
                    PlayerLoopUtility.Clear();
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
