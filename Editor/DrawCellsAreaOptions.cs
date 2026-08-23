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
        /// 轴名称（"X" "Y"）的样式。
        /// </summary>
        public GUIStyle AxisLabelStyle { get; set; }

        /// <summary>
        /// 列/行索引（"0" "1" "2" 等）的样式。
        /// </summary>
        public GUIStyle IndexLabelStyle { get; set; }

        /// <summary>
        /// 轴标签和索引标签远离单元格的偏移量。
        /// <br/>
        /// <see cref="Vector2.x"/> 分量控制 "Y" 轴标签和行索引标签在水平方向远离 grid 的距离（正值向左推移）；
        /// <br/>
        /// <see cref="Vector2.y"/> 分量控制 "X" 轴标签和列索引标签在垂直方向远离 grid 的距离（<see cref="CellRowOrigin.Bottom"/> 模式向下，<see cref="CellRowOrigin.Top"/> 模式向上）。
        /// </summary>
        public Vector2 LabelOffset { get; set; }
    }
}
