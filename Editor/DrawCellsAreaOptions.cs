using System;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 单元格区域绘制配置。
    /// </summary>
    public sealed class DrawCellsAreaOptions
    {
        /// <summary>
        /// 行索引起始方向。
        /// </summary>
        public CellRowOrigin RowOrigin { get; set; }

        /// <summary>
        /// 绘制整体背景。
        /// </summary>
        public Action<Rect, object> OnDrawBackground { get; set; }

        /// <summary>
        /// 绘制单元格区域背景。
        /// </summary>
        public Action<Rect, object> OnDrawCellsAreaBackground { get; set; }

        /// <summary>
        /// 绘制单个单元格。
        /// </summary>
        public Action<Rect, Vector2Int, object> OnDrawCell { get; set; }

        /// <summary>
        /// 绘制删除列按钮。
        /// </summary>
        public Action<Rect, int, object> OnDrawDeleteColumnButton { get; set; }

        /// <summary>
        /// 绘制新增列按钮。
        /// </summary>
        public Action<Rect, int, object> OnDrawAddColumnButton { get; set; }

        /// <summary>
        /// 绘制删除行按钮。
        /// </summary>
        public Action<Rect, int, object> OnDrawDeleteRowButton { get; set; }

        /// <summary>
        /// 绘制新增行按钮。
        /// </summary>
        public Action<Rect, int, object> OnDrawAddRowButton { get; set; }

        /// <summary>
        /// 轴名称（“X”“Y”）的样式。
        /// </summary>
        public GUIStyle AxisLabelStyle { get; set; }

        /// <summary>
        /// 列/行索引（“0”“1”“2”等）的样式。
        /// </summary>
        public GUIStyle IndexLabelStyle { get; set; }
    }
}
