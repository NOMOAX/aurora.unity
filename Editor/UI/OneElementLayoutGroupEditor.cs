using System.Linq;
using Aurora.Unity.UI;
using UnityEditor;

namespace Aurora.UnityEditor.UI
{
    /// <summary>
    /// 为 <see cref="OneElementLayoutGroup"/> 类的派生类的自定义编辑器提供基类。
    /// </summary>
    public abstract class OneElementLayoutGroupEditor : Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            if (targets.Cast<OneElementLayoutGroup>().All(ContainsExactlyOneElement))
            {
                return;
            }
            EditorGUILayout.HelpBox(
                "This layout group won't work properly because it doesn't contain exactly one layout element.",
                MessageType.Warning
            );

            static bool ContainsExactlyOneElement(OneElementLayoutGroup oneElementLayoutGroup)
            {
                return oneElementLayoutGroup.ContainsExactlyOneElement();
            }
        }
    }
}
