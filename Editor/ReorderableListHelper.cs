using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// Provides constants and helper methods needed when using <see cref="ReorderableList"/>.
    /// </summary>
    public static class ReorderableListHelper
    {
        /// <summary>
        /// Half of <see cref="VerticalSpacing"/>, used to shift all elements down so that each element is centered in its highlighted blue background when selected.
        /// </summary>
        private const float ElementContentTopPadding = 1f;

        /// <summary>
        /// This value is both the spacing between adjacent elements (added by <see cref="ReorderableList"/>, not controllable) and the spacing between adjacent rows within an element (specified by me, accepted without question).
        /// </summary>
        /// <remarks>The original value is <see cref="ReorderableList.Defaults"/><c>.elementPadding</c>.</remarks>
        public const float VerticalSpacing = 2f;

        /// <summary>
        /// Gets the height of an element with the specified number of rows, to be used as the return value of the <see cref="ReorderableList.elementHeightCallback"/> callback method.
        /// </summary>
        /// <param name="lineCount">The number of rows of content in the element.</param>
        /// <returns>The height of an element with <paramref name="lineCount"/> rows.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lineCount"/> is less than or equal to 0.</exception>
        public static float GetElementHeight(int lineCount)
        {
            if (lineCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineCount));
            }
            return EditorGUIUtility.singleLineHeight * lineCount + VerticalSpacing * (lineCount - 1);
        }

        /// <summary>
        /// Initializes the <see cref="Rect.y">y</see> of <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The 1st argument of <see cref="ReorderableList.drawElementCallback"/> passed by reference.</param>
        /// <remarks>Call at the beginning of the <see cref="ReorderableList.drawElementCallback"/> callback method.</remarks>
        public static void InitializeY(ref Rect rect)
        {
            rect.y += ElementContentTopPadding;
        }

        /// <summary>
        /// Sets the <see cref="Rect.height">height</see> of <paramref name="rect"/> to <see cref="EditorGUIUtility.singleLineHeight"/>.
        /// </summary>
        /// <param name="rect">The 1st argument of <see cref="ReorderableList.drawElementCallback"/> passed by reference.</param>
        public static void SetSingleLineHeight(ref Rect rect)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Sets the <see cref="Rect.y">y</see> of <paramref name="rect"/> to the starting position of the next row of content.
        /// </summary>
        /// <param name="rect">The 1st argument of <see cref="ReorderableList.drawElementCallback"/> passed by reference.</param>
        /// <remarks>Call before drawing the next row in the <see cref="ReorderableList.drawElementCallback"/> callback method.</remarks>
        public static void NextLine(ref Rect rect)
        {
            rect.y = rect.yMax + VerticalSpacing;
        }

        /// <summary>
        /// Sets the correct <see cref="ReorderableList.footerHeight"/> for a nested <see cref="ReorderableList"/> so that it is centered in its highlighted blue background when selected.
        /// </summary>
        /// <param name="nestedReorderableList">The nested <see cref="ReorderableList"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="nestedReorderableList"/> is <see langword="null"/>.</exception>
        public static void SetFooterHeightForNestedReorderableList(ReorderableList nestedReorderableList)
        {
            if (nestedReorderableList == null)
            {
                throw new ArgumentNullException(nameof(nestedReorderableList));
            }
            nestedReorderableList.footerHeight -= 1f;
        }
    }
}
