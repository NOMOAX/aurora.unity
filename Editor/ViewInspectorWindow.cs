using Aurora.Collections;
using Aurora.Pooling;
using Aurora.Unity.UI.ViewSystem;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    [EditorWindowTitle(title = Title)]
    internal sealed class ViewInspectorWindow : EditorWindow
    {
        private const string Title = "View Inspector";

        [MenuItem("Window" + "/" + UnityEditorUtility.DisplayName + "/" + Title)]
        public static void OpenWindow()
        {
            GetWindow<ViewInspectorWindow>();
        }

        private void OnInspectorUpdate()
        {
            if (View.Dirty)
            {
                View.Dirty = false;
                Repaint();
            }
        }

        private void OnGUI()
        {
            var viewContainerCount = View.ContainerCount;
            if (viewContainerCount == 0)
            {
                EditorGUILayout.HelpBox("There is nothing here.", MessageType.Info);
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    for (var i = 0; i < viewContainerCount; i++)
                    {
                        var viewContainer = View.GetContainer(i);
                        HandleViewContainer(viewContainer);
                    }
                }
            }
        }

        private static void HandleViewContainer(View.ViewContainer viewContainer)
        {
            EditorGUILayout.ObjectField(viewContainer.RectTransform, typeof(RectTransform), true);
            var views = PredefinedPools<View>.List.Get();
            try
            {
                viewContainer.GetViewsFromContainer(TreeEnumOrder.Default, views);
                if (views.Count == 0)
                {
                    return;
                }
                using (new EditorGUI.IndentLevelScope())
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

        private static void HandleView(View view)
        {
            EditorGUILayout.ObjectField(view, typeof(View), true);
            var children = PredefinedPools<View>.List.Get();
            try
            {
                view.GetViewsFromThis(TreeEnumOrder.Default, children);
                if (children.Count == 0)
                {
                    return;
                }
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
    }
}
