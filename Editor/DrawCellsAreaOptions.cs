using System;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// Drawing configuration for a cell region.
    /// </summary>
    public sealed class DrawCellsAreaOptions
    {
        /// <summary>
        /// The starting direction of the row index.
        /// </summary>
        public CellRowOrigin RowOrigin { get; set; }

        /// <summary>
        /// Draws the overall background.
        /// </summary>
        public Action<Rect, object> OnDrawBackground { get; set; }

        /// <summary>
        /// Draws the cell region background.
        /// </summary>
        public Action<Rect, object> OnDrawCellsAreaBackground { get; set; }

        /// <summary>
        /// Draws a single cell.
        /// </summary>
        public Action<Rect, Vector2Int, object> OnDrawCell { get; set; }

        /// <summary>
        /// Draws the delete-column button.
        /// </summary>
        public Action<Rect, int, object> OnDrawDeleteColumnButton { get; set; }

        /// <summary>
        /// Draws the add-column button.
        /// </summary>
        public Action<Rect, int, object> OnDrawAddColumnButton { get; set; }

        /// <summary>
        /// Draws the delete-row button.
        /// </summary>
        public Action<Rect, int, object> OnDrawDeleteRowButton { get; set; }

        /// <summary>
        /// Draws the add-row button.
        /// </summary>
        public Action<Rect, int, object> OnDrawAddRowButton { get; set; }

        /// <summary>
        /// The style of the axis names ("X" "Y").
        /// </summary>
        public GUIStyle AxisLabelStyle { get; set; }

        /// <summary>
        /// The style of the column/row indices ("0" "1" "2" etc.).
        /// </summary>
        public GUIStyle IndexLabelStyle { get; set; }

        /// <summary>
        /// The offset by which axis labels and index labels are moved away from the cells.
        /// <br/>
        /// The <see cref="Vector2.x"/> component controls how far the "Y" axis label and row index labels are moved horizontally away from the grid (positive values push left);
        /// <br/>
        /// The <see cref="Vector2.y"/> component controls how far the "X" axis label and column index labels are moved vertically away from the grid (<see cref="CellRowOrigin.Bottom"/> mode moves down, <see cref="CellRowOrigin.Top"/> mode moves up).
        /// </summary>
        public Vector2 LabelOffset { get; set; }
    }
}
