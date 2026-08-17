using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 提供在使用 <see cref="ReorderableList"/> 时需要用到的辅助方法。
    /// </summary>
    public static class ReorderableListHelper
    {
        /// <summary>
        /// <see cref="VerticalSpacing"/> 的一半，用于将所有元素向下移动，使得各元素在它被选中时的亮蓝色背景里居中。
        /// </summary>
        private const float ElementContentTopPadding = 1f;

        /// <summary>
        /// 这个值既是相邻元素的间距（由 <see cref="ReorderableList"/> 添加，不可控制），又是元素内部相邻行的间距（我规定的，不服来干我）。
        /// </summary>
        /// <remarks>原始值为 <see cref="ReorderableList.Defaults"/><c>.elementPadding</c>。</remarks>
        private const float VerticalSpacing = 2f;

        /// <summary>
        /// 获取具有指定行数的元素的高度，用于作为 <see cref="ReorderableList.elementHeightCallback"/> 回调方法的返回值。
        /// </summary>
        /// <param name="lineCount">元素内内容的行数。</param>
        /// <returns>具有 <paramref name="lineCount"/> 行的元素的高度。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lineCount"/> 小于或等于 0。</exception>
        public static float GetElementHeight(int lineCount)
        {
            if (lineCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineCount));
            }
            return EditorGUIUtility.singleLineHeight * lineCount + VerticalSpacing * (lineCount - 1);
        }

        /// <summary>
        /// 初始化 <paramref name="rect"/> 的 <see cref="Rect.y">y</see>。
        /// </summary>
        /// <param name="rect">按引用传入 <see cref="ReorderableList.drawElementCallback"/> 的第 1 个参数。</param>
        /// <remarks>在 <see cref="ReorderableList.drawElementCallback"/> 回调方法的一开始调用。</remarks>
        public static void InitializeY(ref Rect rect)
        {
            rect.y += ElementContentTopPadding;
        }

        /// <summary>
        /// 设置 <paramref name="rect"/> 的 <see cref="Rect.height">height</see> 为 <see cref="EditorGUIUtility.singleLineHeight"/>。
        /// </summary>
        /// <param name="rect">按引用传入 <see cref="ReorderableList.drawElementCallback"/> 的第 1 个参数。</param>
        public static void SetSingleLineHeight(ref Rect rect)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// 设置 <paramref name="rect"/> 的 <see cref="Rect.y">y</see> 为下一行内容的起始位置。
        /// </summary>
        /// <param name="rect">按引用传入 <see cref="ReorderableList.drawElementCallback"/> 的第 1 个参数。</param>
        /// <remarks>在 <see cref="ReorderableList.drawElementCallback"/> 回调方法中需要绘制下一行之前调用。</remarks>
        public static void NextLine(ref Rect rect)
        {
            rect.y = rect.yMax + VerticalSpacing;
        }
    }
}
