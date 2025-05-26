using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    internal static class OptionMenus
    {
        private const int CreateScrollViewPriority = 19950321;

        [MenuItem("GameObject/UI/Scroll View - Aurora Unity", priority = CreateScrollViewPriority)]
        public static void CreateScrollView(MenuCommand menuCommand)
        {
            CreateNewScrollViewWindow.OpenWindow(menuCommand.context as GameObject);
        }
    }
}
