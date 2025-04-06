using Aurora.Collections;
using Aurora.Pooling;
using Aurora.Unity.UI.ViewSystem;
using UnityEditor;

namespace Aurora.UnityEditor
{
    internal sealed class ViewInspectorWindow : EditorWindow
    {
        [MenuItem("Window/Aurora Unity/View Inspector")]
        public static void OpenWindow()
        {
            GetWindow<ViewInspectorWindow>("View Inspector").ShowUtility();
        }

        private static void HandleView(View view)
        {
            if (view == null)
            {
                return;
            }
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(view, typeof(View), true);
            }
            var children = PredefinedPools<View>.List.Get();
            try
            {
                View.GetViews(view, TreeEnumOrder.Default, children);
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var child in children)
                    {
                        HandleView(child);
                    }
                }
            }
            finally
            {
                PredefinedPools<View>.List.Return(children);
            }
        }

        private void OnGUI()
        {
            var views = PredefinedPools<View>.List.Get();
            try
            {
                View.GetViews(null, TreeEnumOrder.Default, views);
                if (views.Count == 0)
                {
                    EditorGUILayout.HelpBox("No views.", MessageType.Info);
                }
                else
                {
                    foreach (var view in views)
                    {
                        HandleView(view);
                    }
                }
            }
            finally
            {
                PredefinedPools<View>.List.Return(views);
            }
        }
    }
}
