#if UNITY_6000_3_OR_NEWER && !UNITY_6000_3_0 && !UNITY_6000_3_1 && !UNITY_6000_3_2 && !UNITY_6000_3_3
#define UNITY_6000_3_4_OR_NEWER
#endif
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    internal static class OptionMenus
    {
        private const int CreateScrollViewPriority = 19950321;

#if UNITY_6000_3_4_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Scroll View (Aurora Unity)", priority = CreateScrollViewPriority)]
#else
        [MenuItem("GameObject/UI/Scroll View (Aurora Unity)", priority = CreateScrollViewPriority)]
#endif
        private static void CreateScrollView(MenuCommand menuCommand)
        {
            CreateNewScrollViewWindow.OpenWindow(menuCommand.context as GameObject);
        }
    }
}
