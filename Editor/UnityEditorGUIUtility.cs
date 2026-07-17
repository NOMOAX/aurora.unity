using System;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 提供一组辅助 IMGUI 绘制的工具。
    /// </summary>
    public static class UnityEditorGUIUtility
    {
        /// <summary>
        /// 绘制外边框。
        /// </summary>
        /// <param name="rect">边框的位置和大小。</param>
        /// <param name="color">边框的颜色。</param>
        /// <param name="thickness">边框的粗细。</param>
        public static void DrawOuterBorder(Rect rect, Color color, float thickness)
        {
            // @formatter:max_line_length 10000
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMin - thickness, rect.xMax + thickness, rect.yMin), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMax, rect.xMax + thickness, rect.yMax + thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin - thickness, rect.yMin, rect.xMin, rect.yMax), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMax, rect.yMin, rect.xMax + thickness, rect.yMax), color);
            // @formatter:max_line_length restore
        }

        /// <summary>
        /// 绘制内边框。
        /// </summary>
        /// <param name="rect">边框的位置和大小。</param>
        /// <param name="color">边框的颜色。</param>
        /// <param name="thickness">边框的粗细。</param>
        public static void DrawInnerBorder(Rect rect, Color color, float thickness)
        {
            // @formatter:max_line_length 10000
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMax - thickness, rect.xMax, rect.yMax), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMin, rect.yMin + thickness, rect.xMin + thickness, rect.yMax - thickness), color);
            EditorGUI.DrawRect(Rect.MinMaxRect(rect.xMax - thickness, rect.yMin + thickness, rect.xMax, rect.yMax - thickness), color);
            // @formatter:max_line_length restore
        }

        /// <summary>
        /// 绘制单元格区域。
        /// </summary>
        /// <param name="padding">整个区域的外边距。</param>
        /// <param name="cellsAreaPadding">单元格区域的外边距（与列/行工具按钮区之间的间距）。</param>
        /// <param name="cellDimensions">单元格的列数和行数（x = 列数，y = 行数）。</param>
        /// <param name="cellSize">单个单元格的大小。</param>
        /// <param name="cellSpacing">单元格之间的间距。</param>
        /// <param name="drawOptions">绘制配置（回调与样式）。</param>
        public static void DrawCellsArea(
            RectOffset           padding,
            RectOffset           cellsAreaPadding,
            Vector2Int           cellDimensions,
            Vector2              cellSize,
            Vector2              cellSpacing,
            CellsAreaDrawOptions drawOptions)
        {
            // 1. 按钮溢出量：新增按钮比最末单元格多出的空间
            //    buttonOverflow = cellSize * 0.5f - cellSpacing * 0.5f   (Vector2)
            //    effectiveMargin = max(cellsAreaPadding, buttonOverflow)

            // 2. 从整体区域 Rect 缩进 effectiveMargin，得到 cellsAreaRect

            // 3. 绘制整体背景 OnDrawBackground(totalRect)

            // 4. 计算子区域 Rect（列/行按钮位置待后续确定）：
            //    columnButtonsRect — 列工具区
            //    rowButtonsRect    — 行工具区
            //    gridRect          — 单元格区

            // 5. 绘制单元格区域背景 OnDrawCellsAreaBackground

            // 6. 根据 RowOrigin 分流：
            //    Bottom（默认）：列工具在下方，遵循 Unity 空间坐标系（Y 向上，行 0 在底）
            //    Top（反向）：   列工具在上方，对应 GUI Y 向下（行 0 在顶）
            //    if (drawOptions.RowOrigin == CellRowOrigin.Bottom) DrawCellsAreaBottom(...)
            //    else DrawCellsAreaTop(...)

            // 7. 遍历单元格（固定列优先：先遍历列再遍历行）：
            //    for col 0..cellDimensions.x-1:
            //      for row 0..cellDimensions.y-1:
            //        OnDrawCell(rectAt(col, row), new Vector2Int(col, row))
        }

        /// <summary>
        /// CellRowOrigin.Top：行 0 在顶部，Y 向下递增。
        /// </summary>
        private static void DrawCellsAreaTop(
            Rect                 gridRect,
            Vector2              cellSize,
            Vector2              cellSpacing,
            Vector2Int           cellDimensions,
            CellsAreaDrawOptions drawOptions)
        {
            // 遍历每行 i（0 到 cellDimensions.y）：
            //   cellY = gridRect.y + i * (cellSize.y + cellSpacing.y)
            //
            //   // 行删除按钮（左侧 rowButtonsRect 内）
            //   OnDrawDeleteRowButton(new Rect(..., cellY, ..., cellSize.y), i)
            //
            //   // 行新增按钮（居中于间距）
            //   addY = cellY + cellSize.y + cellSpacing.y * 0.5f
            //   OnDrawAddNewRowButton(new Rect(..., addY, ..., cellSize.y), i)
            //
            //   // 最后多一个新增按钮（i == cellDimensions.y 时无删除按钮）
            //
            // 遍历每个单元格（col, row）：
            //   x = gridRect.x + col * (cellSize.x + cellSpacing.x)
            //   y = gridRect.y + row * (cellSize.y + cellSpacing.y)
            //   OnDrawCell(new Rect(x, y, cellSize), new Vector2Int(col, row))

            throw new NotImplementedException();
        }

        /// <summary>
        /// CellRowOrigin.Bottom：行 0 在底部，Y 向上递减。
        /// </summary>
        private static void DrawCellsAreaBottom(
            Rect                 gridRect,
            Vector2              cellSize,
            Vector2              cellSpacing,
            Vector2Int           cellDimensions,
            CellsAreaDrawOptions drawOptions)
        {
            // 行方向翻转：以 gridRect 底部为基准向上计算
            // 遍历每行 i（0 到 cellDimensions.y）：
            //   cellY = gridRect.yMax - (i + 1) * cellSize.y - i * cellSpacing.y
            //
            //   // 行删除按钮
            //   OnDrawDeleteRowButton(new Rect(..., cellY, ..., cellSize.y), i)
            //
            //   // 行新增按钮（上方）
            //   addY = cellY - cellSpacing.y * 0.5f
            //   OnDrawAddNewRowButton(new Rect(..., addY, ..., cellSize.y), i)
            //
            //   // 最后多一个新增按钮（i == cellDimensions.y 时在顶部之上）
            //
            // 遍历每个单元格（col, row）：
            //   x = gridRect.x + col * (cellSize.x + cellSpacing.x)
            //   y = gridRect.yMax - (row + 1) * cellSize.y - row * cellSpacing.y
            //   OnDrawCell(new Rect(x, y, cellSize), new Vector2Int(col, row))

            throw new NotImplementedException();
        }
    }
}
