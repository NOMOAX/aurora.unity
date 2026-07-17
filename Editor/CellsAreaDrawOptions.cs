using System;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 单元格区域绘制配置。
    /// </summary>
    public sealed class CellsAreaDrawOptions
    {
        /// <summary>
        /// 行索引起始方向。
        /// </summary>
        public CellRowOrigin RowOrigin;

        /// <summary>
        /// 绘制整体背景。
        /// </summary>
        public Action<Rect> OnDrawBackground;

        /// <summary>
        /// 绘制单元格区域背景。
        /// </summary>
        public Action<Rect, int> OnDrawCellsAreaBackground;

        /// <summary>
        /// 绘制单个单元格。
        /// </summary>
        public Action<Rect, Vector2Int> OnDrawCell;

        /// <summary>
        /// 绘制删除列按钮。
        /// </summary>
        public Action<Rect, int> OnDrawDeleteColumnButton;

        /// <summary>
        /// 绘制新增列按钮。
        /// </summary>
        public Action<Rect, int> OnDrawAddNewColumnButton;

        /// <summary>
        /// 绘制删除行按钮。
        /// </summary>
        public Action<Rect, int> OnDrawDeleteRowButton;

        /// <summary>
        /// 绘制新增行按钮。
        /// </summary>
        public Action<Rect, int> OnDrawAddNewRowButton;

        /// <summary>
        /// 轴名称（"X"、"Y"）的标签样式。
        /// </summary>
        public GUIStyle AxisLabelStyle;

        /// <summary>
        /// 行/列索引（"0"、"1"、"2"…）的标签样式。
        /// </summary>
        public GUIStyle IndexLabelStyle;
    }
}
