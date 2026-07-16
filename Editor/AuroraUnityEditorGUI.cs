using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    public static class AuroraUnityEditorGUI
    {
        public static void DrawOuterBorder(Rect rect, Color color, float thickness)
        {
            float x0 = rect.x - thickness;
            float y0 = rect.y - thickness;
            float x1 = rect.x + rect.width;
            float y1 = rect.y + rect.height;
            float w  = rect.width + thickness * 2;
            float h  = rect.height;

            // 上边
            EditorGUI.DrawRect(new Rect(x0, y0, w, thickness), color);
            // 下边
            EditorGUI.DrawRect(new Rect(x0, y1, w, thickness), color);
            // 左边（不与上边/下边重叠，从 rect.y 起）
            EditorGUI.DrawRect(new Rect(x0, rect.y, thickness, h), color);
            // 右边
            EditorGUI.DrawRect(new Rect(x1, rect.y, thickness, h), color);
        }

        /// <summary>
        /// 绘制内边框 — 边框从 rect 边界向内延伸。
        /// 交界处的重叠部分由水平矩形（上边/下边）负责，垂直矩形两端向内缩让出角。
        /// </summary>
        /// <param name="rect">原始矩形</param>
        /// <param name="color">边框颜色</param>
        /// <param name="thickness">边框厚度（像素）</param>
        public static void DrawInnerBorder(Rect rect, Color color, float thickness)
        {
            float x0 = rect.x;
            float y0 = rect.y;
            float x1 = rect.x + rect.width - thickness;
            float y1 = rect.y + rect.height - thickness;
            float w  = rect.width;
            float h  = rect.height - thickness * 2;

            // 上边 — 覆盖左上角 + 右上角
            EditorGUI.DrawRect(new Rect(x0, y0, w, thickness), color);
            // 下边 — 覆盖左下角 + 右下角
            EditorGUI.DrawRect(new Rect(x0, y1, w, thickness), color);
            // 左边 — y 缩进 thickness，上下角让给水平边
            EditorGUI.DrawRect(new Rect(x0, y0 + thickness, thickness, h), color);
            // 右边 — 同上
            EditorGUI.DrawRect(new Rect(x1, y0 + thickness, thickness, h), color);
        }
    }
}
