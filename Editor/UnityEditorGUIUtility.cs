// @formatter:max_line_length 10000

using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// Provides a set of utilities to assist IMGUI drawing.
    /// </summary>
    public static class UnityEditorGUIUtility
    {
        private const float AxisLabelPositionOffset = 0.7f;

        private static readonly GUILayoutOption[] WithNoExpand = { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };

        /// <summary>
        /// Draws an outer border.
        /// </summary>
        /// <param name="rect">The position and size of the border.</param>
        /// <param name="color">The color of the border.</param>
        /// <param name="thickness">The thickness of the border.</param>
        public static void DrawOuterBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMin - thickness, rect.xMax + thickness, rect.yMin), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMax, rect.xMax + thickness, rect.yMax + thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMin, rect.xMin, rect.yMax), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMax, rect.yMin, rect.xMax + thickness, rect.yMax), color);
        }

        /// <summary>
        /// Draws an inner border.
        /// </summary>
        /// <param name="rect">The position and size of the border.</param>
        /// <param name="color">The color of the border.</param>
        /// <param name="thickness">The thickness of the border.</param>
        public static void DrawInnerBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMax - thickness, rect.xMax, rect.yMax), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMin + thickness, rect.xMin + thickness, rect.yMax - thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMax - thickness, rect.yMin + thickness, rect.xMax, rect.yMax - thickness), color);
        }

        /// <summary>
        /// Draws a cell area.
        /// </summary>
        /// <param name="padding">The outer margin of the whole area (x = left, y = bottom, z = right, w = top).</param>
        /// <param name="cellsAreaPadding">The outer margin of the cell area (x = left, y = bottom, z = right, w = top).</param>
        /// <param name="cellDimensions">The column and row counts of the cells (x = columns, y = rows).</param>
        /// <param name="cellSize">The size of a cell. The tool will also use this value as the size.</param>
        /// <param name="cellSpacing">The spacing between cells. The tool will also use this value as the spacing.</param>
        /// <param name="options">The drawing configuration (callbacks and styles).</param>
        /// <param name="state">The caller-defined state, passed as the last argument to each drawing delegate.</param>
        public static void DrawCellsArea(Vector4 padding, Vector4 cellsAreaPadding, Vector2Int cellDimensions, Vector2 cellSize, Vector2 cellSpacing, DrawCellsAreaOptions options, object state)
        {
            if (float.IsNaN(padding.x) || float.IsNaN(padding.y) || float.IsNaN(padding.z) || float.IsNaN(padding.w) || padding.x < 0 || padding.y < 0 || padding.z < 0 || padding.w < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(padding));
            }
            if (float.IsNaN(cellsAreaPadding.x) || float.IsNaN(cellsAreaPadding.y) || float.IsNaN(cellsAreaPadding.z) || float.IsNaN(cellsAreaPadding.w) || cellsAreaPadding.x < 0 || cellsAreaPadding.y < 0 || cellsAreaPadding.z < 0 || cellsAreaPadding.w < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellsAreaPadding));
            }
            if (cellDimensions.x < 0 || cellDimensions.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellDimensions));
            }
            if (float.IsNaN(cellSize.x) || float.IsNaN(cellSize.y) || cellSize.x < 0 || cellSize.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize));
            }
            if (float.IsNaN(cellSpacing.x) || float.IsNaN(cellSpacing.y) || cellSpacing.x < 0 || cellSpacing.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSpacing));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            switch (options.RowOrigin)
            {
                case CellRowOrigin.Bottom:
                    DrawCellsAreaBottom(padding, cellsAreaPadding, cellDimensions, cellSize, cellSpacing, options, state);
                    break;
                case CellRowOrigin.Top:
                    DrawCellsAreaTop(padding, cellsAreaPadding, cellDimensions, cellSize, cellSpacing, options, state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawCellsAreaBottom(Vector4 padding, Vector4 cellsAreaPadding, Vector2Int cellDimensions, Vector2 cellSize, Vector2 cellSpacing, DrawCellsAreaOptions options, object state)
        {
            #region Calculate the sizes of each region

            var columns = cellDimensions.x;
            var rows    = cellDimensions.y;
            var stepX   = cellSize.x + cellSpacing.x;
            var stepY   = cellSize.y + cellSpacing.y;

            // Button overflow amount
            var buttonOverflowX = (cellSize.x + cellSpacing.x) * 0.5f;
            var buttonOverflowY = (cellSize.y + cellSpacing.y) * 0.5f;

            // Effective margins: left/bottom push the button area away; right/top take the max with the overflow amount
            var effectiveLeft   = cellsAreaPadding.x;                             // x = left
            var effectiveBottom = cellsAreaPadding.y;                             // y = bottom
            var effectiveRight  = Mathf.Max(cellsAreaPadding.z, buttonOverflowX); // z = right
            var effectiveTop    = Mathf.Max(cellsAreaPadding.w, buttonOverflowY); // w = top

            var rowButtonsWidth     = stepX + cellSize.x;
            var columnButtonsHeight = stepY + cellSize.y;
            var gridWidth           = columns * cellSize.x + Mathf.Max(0, columns - 1) * cellSpacing.x;
            var gridHeight          = rows * cellSize.y + Mathf.Max(0,    rows - 1) * cellSpacing.y;
            var innerWidth          = rowButtonsWidth + effectiveLeft + gridWidth + effectiveRight;
            var innerHeight         = effectiveTop + gridHeight + effectiveBottom + columnButtonsHeight;
            var totalWidth          = padding.x + innerWidth + padding.z;
            var totalHeight         = padding.y + innerHeight + padding.w;

            #endregion

            // Get totalRect and split sub-regions
            var totalRect = GUILayoutUtility.GetRect(totalWidth, totalHeight, WithNoExpand);
            var innerRect = new Rect(totalRect.xMin + padding.x, totalRect.yMin + padding.w, innerWidth, innerHeight);
            var gridRect = new Rect(innerRect.xMin + rowButtonsWidth + effectiveLeft, innerRect.yMin + effectiveTop, gridWidth, gridHeight);
            var columnButtonsRect = new Rect(gridRect.xMin, gridRect.yMax + effectiveBottom, gridWidth + effectiveRight, columnButtonsHeight);
            var rowButtonsRect = new Rect(innerRect.xMin, innerRect.yMin + effectiveTop - buttonOverflowY, rowButtonsWidth, gridHeight + buttonOverflowY * 2f);

            // Background
            options.OnDrawBackground?.Invoke(totalRect, state);

            // Cell area background = gridRect + cellsAreaPadding
            if (options.OnDrawCellsAreaBackground != null)
            {
                var cellsAreaBackgroundRect = new Rect(gridRect.xMin - cellsAreaPadding.x, gridRect.yMin - cellsAreaPadding.w, cellsAreaPadding.x + gridRect.width + cellsAreaPadding.z, cellsAreaPadding.y + gridRect.height + cellsAreaPadding.w);
                options.OnDrawCellsAreaBackground(cellsAreaBackgroundRect, state);
            }

            // Delete column button
            if (options.OnDrawDeleteColumnButton != null)
            {
                var deleteColumnY = columnButtonsRect.yMin;
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var deleteColumnX        = gridRect.xMin + columnIndex * stepX;
                    var deleteColumnPosition = new Vector2(deleteColumnX, deleteColumnY);
                    var deleteColumnRect     = new Rect(deleteColumnPosition, cellSize);
                    options.OnDrawDeleteColumnButton(deleteColumnRect, columnIndex, state);
                }
            }
            // Add column button
            if (options.OnDrawAddColumnButton != null)
            {
                var addColumnY = columnButtonsRect.yMin + stepY;
                for (var columnInsertIndex = 0; columnInsertIndex <= columns; columnInsertIndex++)
                {
                    var addColumnX        = gridRect.xMin + (columnInsertIndex - 0.5f) * stepX;
                    var addColumnPosition = new Vector2(addColumnX, addColumnY);
                    var addColumnRect     = new Rect(addColumnPosition, cellSize);
                    options.OnDrawAddColumnButton(addColumnRect, columnInsertIndex, state);
                }
            }

            // Delete row button
            if (options.OnDrawDeleteRowButton != null)
            {
                var deleteRowX = rowButtonsRect.xMin + stepX;
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var deleteRowY        = gridRect.yMax - (rowIndex + 1) * cellSize.y - rowIndex * cellSpacing.y;
                    var deleteRowPosition = new Vector2(deleteRowX, deleteRowY);
                    var deleteRowRect     = new Rect(deleteRowPosition, cellSize);
                    options.OnDrawDeleteRowButton(deleteRowRect, rowIndex, state);
                }
            }
            // Add row button
            if (options.OnDrawAddRowButton != null)
            {
                var addRowX = rowButtonsRect.xMin;
                for (var rowInsertIndex = 0; rowInsertIndex <= rows; rowInsertIndex++)
                {
                    var addRowY = gridRect.yMax - (rowInsertIndex + 0.5f) * cellSize.y - (rowInsertIndex - 0.5f) * cellSpacing.y;
                    var addRowPosition = new Vector2(addRowX, addRowY);
                    var addRowRect = new Rect(addRowPosition, cellSize);
                    options.OnDrawAddRowButton(addRowRect, rowInsertIndex, state);
                }
            }

            // Each cell
            if (options.OnDrawCell != null)
            {
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var cellX        = gridRect.xMin + columnIndex * stepX;
                    var cellY        = gridRect.yMax - (rowIndex + 1) * cellSize.y - rowIndex * cellSpacing.y;
                    var cellPosition = new Vector2(cellX, cellY);
                    var cellRect     = new Rect(cellPosition, cellSize);
                    var cellCoord    = new Vector2Int(columnIndex, rowIndex);
                    options.OnDrawCell(cellRect, cellCoord, state);
                }
            }

            if (options.IndexLabelStyle != null)
            {
                // Column index label: the top midpoint is adjacent to the bottom midpoint of the corresponding column cell
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var content = new GUIContent(columnIndex.ToString(NumberFormatInfo.InvariantInfo));
                    var anchorX = gridRect.xMin + columnIndex * stepX + cellSize.x * 0.5f;
                    var labelSize = options.IndexLabelStyle.CalcSize(content);
                    var labelPosition = new Vector2(anchorX - labelSize.x * 0.5f, gridRect.yMax + options.LabelOffset.y);
                    var labelRect = new Rect(labelPosition, labelSize);
                    GUI.Label(labelRect, content, options.IndexLabelStyle);
                }
                // Row index label: the right midpoint is adjacent to the left midpoint of the corresponding row cell
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var content = new GUIContent(rowIndex.ToString(NumberFormatInfo.InvariantInfo));
                    var cellY = gridRect.yMax - (rowIndex + 1) * cellSize.y - rowIndex * cellSpacing.y;
                    var anchorY = cellY + cellSize.y * 0.5f;
                    var labelSize = options.IndexLabelStyle.CalcSize(content);
                    var labelPosition = new Vector2(gridRect.xMin - labelSize.x - options.LabelOffset.x, anchorY - labelSize.y * 0.5f);
                    var labelRect = new Rect(labelPosition, labelSize);
                    GUI.Label(labelRect, content, options.IndexLabelStyle);
                }
            }

            if (options.AxisLabelStyle != null)
            {
                // "X" axis label: the top-right corner is aligned with the bottom-left corner of the (0,0) cell, then shifted right
                {
                    var content = new GUIContent("X");
                    var xLabelSize = options.AxisLabelStyle.CalcSize(content);
                    var xLabelPosition = new Vector2(gridRect.xMin + xLabelSize.x * AxisLabelPositionOffset - xLabelSize.x, gridRect.yMax + options.LabelOffset.y);
                    var xLabelRect = new Rect(xLabelPosition, xLabelSize);
                    GUI.Label(xLabelRect, content, options.AxisLabelStyle);
                }
                // "Y" axis label: the top-right corner is aligned with the bottom-left corner of the (0,0) cell, then shifted up
                {
                    var content = new GUIContent("Y");
                    var yLabelSize = options.AxisLabelStyle.CalcSize(content);
                    var yLabelPosition = new Vector2(gridRect.xMin - yLabelSize.x - options.LabelOffset.x, gridRect.yMax - yLabelSize.y * AxisLabelPositionOffset);
                    var yLabelRect = new Rect(yLabelPosition, yLabelSize);
                    GUI.Label(yLabelRect, content, options.AxisLabelStyle);
                }
            }
        }

        private static void DrawCellsAreaTop(Vector4 padding, Vector4 cellsAreaPadding, Vector2Int cellDimensions, Vector2 cellSize, Vector2 cellSpacing, DrawCellsAreaOptions options, object state)
        {
            #region Calculate the sizes of each region

            var columns = cellDimensions.x;
            var rows    = cellDimensions.y;
            var stepX   = cellSize.x + cellSpacing.x;
            var stepY   = cellSize.y + cellSpacing.y;

            // Button overflow amount
            var buttonOverflowX = (cellSize.x + cellSpacing.x) * 0.5f;
            var buttonOverflowY = (cellSize.y + cellSpacing.y) * 0.5f;

            // Effective margins: left/top push the button area away; right/bottom take the max with the overflow amount
            var effectiveLeft   = cellsAreaPadding.x;                             // x = left
            var effectiveBottom = Mathf.Max(cellsAreaPadding.y, buttonOverflowY); // y = bottom
            var effectiveRight  = Mathf.Max(cellsAreaPadding.z, buttonOverflowX); // z = right
            var effectiveTop    = cellsAreaPadding.w;                             // w = top

            var rowButtonsWidth     = stepX + cellSize.x;
            var columnButtonsHeight = stepY + cellSize.y;
            var gridWidth           = columns * cellSize.x + Mathf.Max(0, columns - 1) * cellSpacing.x;
            var gridHeight          = rows * cellSize.y + Mathf.Max(0,    rows - 1) * cellSpacing.y;
            var innerWidth          = rowButtonsWidth + effectiveLeft + gridWidth + effectiveRight;
            var innerHeight         = columnButtonsHeight + effectiveTop + gridHeight + effectiveBottom;
            var totalWidth          = padding.x + innerWidth + padding.z;
            var totalHeight         = padding.y + innerHeight + padding.w;

            #endregion

            // Get totalRect and split sub-regions
            var totalRect = GUILayoutUtility.GetRect(totalWidth, totalHeight, WithNoExpand);
            var innerRect = new Rect(totalRect.xMin + padding.x, totalRect.yMin + padding.w, innerWidth, innerHeight);
            var gridRect = new Rect(innerRect.xMin + rowButtonsWidth + effectiveLeft, innerRect.yMin + columnButtonsHeight + effectiveTop, gridWidth, gridHeight);
            var columnButtonsRect = new Rect(innerRect.xMin + rowButtonsWidth + effectiveLeft, innerRect.yMin, gridWidth + effectiveRight, columnButtonsHeight);
            var rowButtonsRect = new Rect(innerRect.xMin, gridRect.yMin - buttonOverflowY, rowButtonsWidth, gridHeight + buttonOverflowY * 2f);

            // Background
            options.OnDrawBackground?.Invoke(totalRect, state);

            // Cell area background = gridRect + cellsAreaPadding
            if (options.OnDrawCellsAreaBackground != null)
            {
                var cellsAreaBackgroundRect = new Rect(gridRect.xMin - cellsAreaPadding.x, gridRect.yMin - cellsAreaPadding.w, cellsAreaPadding.x + gridRect.width + cellsAreaPadding.z, cellsAreaPadding.y + gridRect.height + cellsAreaPadding.w);
                options.OnDrawCellsAreaBackground(cellsAreaBackgroundRect, state);
            }

            // Delete column button
            if (options.OnDrawDeleteColumnButton != null)
            {
                var deleteColumnY = columnButtonsRect.yMin + stepY;
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var deleteColumnX        = gridRect.xMin + columnIndex * stepX;
                    var deleteColumnPosition = new Vector2(deleteColumnX, deleteColumnY);
                    var deleteColumnRect     = new Rect(deleteColumnPosition, cellSize);
                    options.OnDrawDeleteColumnButton(deleteColumnRect, columnIndex, state);
                }
            }
            // Add column button
            if (options.OnDrawAddColumnButton != null)
            {
                var addColumnY = columnButtonsRect.yMin;
                for (var columnInsertIndex = 0; columnInsertIndex <= columns; columnInsertIndex++)
                {
                    var addColumnX        = gridRect.xMin + (columnInsertIndex - 0.5f) * stepX;
                    var addColumnPosition = new Vector2(addColumnX, addColumnY);
                    var addColumnRect     = new Rect(addColumnPosition, cellSize);
                    options.OnDrawAddColumnButton(addColumnRect, columnInsertIndex, state);
                }
            }

            // Delete row button
            if (options.OnDrawDeleteRowButton != null)
            {
                var deleteRowX = rowButtonsRect.xMin + stepX;
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var deleteRowY        = gridRect.yMin + rowIndex * stepY;
                    var deleteRowPosition = new Vector2(deleteRowX, deleteRowY);
                    var deleteRowRect     = new Rect(deleteRowPosition, cellSize);
                    options.OnDrawDeleteRowButton(deleteRowRect, rowIndex, state);
                }
            }
            // Add row button
            if (options.OnDrawAddRowButton != null)
            {
                var addRowX = rowButtonsRect.xMin;
                for (var rowInsertIndex = 0; rowInsertIndex <= rows; rowInsertIndex++)
                {
                    var addRowY        = gridRect.yMin + (rowInsertIndex - 0.5f) * stepY;
                    var addRowPosition = new Vector2(addRowX, addRowY);
                    var addRowRect     = new Rect(addRowPosition, cellSize);
                    options.OnDrawAddRowButton(addRowRect, rowInsertIndex, state);
                }
            }

            // Each cell
            if (options.OnDrawCell != null)
            {
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var cellX        = gridRect.xMin + columnIndex * stepX;
                    var cellY        = gridRect.yMin + rowIndex * stepY;
                    var cellPosition = new Vector2(cellX, cellY);
                    var cellRect     = new Rect(cellPosition, cellSize);
                    var cellCoord    = new Vector2Int(columnIndex, rowIndex);
                    options.OnDrawCell(cellRect, cellCoord, state);
                }
            }

            if (options.IndexLabelStyle != null)
            {
                // Column index label: the top midpoint is adjacent to the top midpoint of the corresponding column cell
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    var content = new GUIContent(columnIndex.ToString(NumberFormatInfo.InvariantInfo));
                    var anchorX = gridRect.xMin + columnIndex * stepX + cellSize.x * 0.5f;
                    var labelSize = options.IndexLabelStyle.CalcSize(content);
                    var labelPosition = new Vector2(anchorX - labelSize.x * 0.5f, gridRect.yMin - labelSize.y - options.LabelOffset.y);
                    var labelRect = new Rect(labelPosition, labelSize);
                    GUI.Label(labelRect, content, options.IndexLabelStyle);
                }
                // Row index label: the right midpoint is adjacent to the left midpoint of the corresponding row cell
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    var content = new GUIContent(rowIndex.ToString(NumberFormatInfo.InvariantInfo));
                    var cellY = gridRect.yMin + rowIndex * stepY;
                    var anchorY = cellY + cellSize.y * 0.5f;
                    var labelSize = options.IndexLabelStyle.CalcSize(content);
                    var labelPosition = new Vector2(gridRect.xMin - labelSize.x - options.LabelOffset.x, anchorY - labelSize.y * 0.5f);
                    var labelRect = new Rect(labelPosition, labelSize);
                    GUI.Label(labelRect, content, options.IndexLabelStyle);
                }
            }

            if (options.AxisLabelStyle != null)
            {
                // "X" axis label: the bottom-right corner is aligned with the top-left corner of the (0,0) cell, then shifted right
                {
                    var content = new GUIContent("X");
                    var xLabelSize = options.AxisLabelStyle.CalcSize(content);
                    var xLabelPosition = new Vector2(gridRect.xMin + xLabelSize.x * AxisLabelPositionOffset - xLabelSize.x, gridRect.yMin - xLabelSize.y - options.LabelOffset.y);
                    var xLabelRect = new Rect(xLabelPosition, xLabelSize);
                    GUI.Label(xLabelRect, content, options.AxisLabelStyle);
                }
                // "Y" axis label: the bottom-right corner is aligned with the top-left corner of the (0,0) cell, then shifted down
                {
                    var content = new GUIContent("Y");
                    var yLabelSize = options.AxisLabelStyle.CalcSize(content);
                    var yLabelPosition = new Vector2(gridRect.xMin - yLabelSize.x - options.LabelOffset.x, gridRect.yMin - yLabelSize.y * (1f - AxisLabelPositionOffset));
                    var yLabelRect = new Rect(yLabelPosition, yLabelSize);
                    GUI.Label(yLabelRect, content, options.AxisLabelStyle);
                }
            }
        }
    }
}
// @formatter:max_line_length restore
