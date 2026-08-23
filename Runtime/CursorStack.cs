using System.Collections.Generic;
using Aurora.Diagnostics;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides methods to set and restore the cursor.
    /// </summary>
    /// <remarks>If you set a default cursor in PlayerSettings, you should call <see cref="set_InitialCursorInfo"/> after the program starts.</remarks>
    public static class CursorStack
    {
        /// <summary>
        /// Sets the initial cursor info.
        /// </summary>
        /// <remarks>If you set a default cursor in PlayerSettings, you should call <see cref="set_InitialCursorInfo"/> after the program starts.</remarks>
        public static CursorInfo InitialCursorInfo { set => _initialCursorInfo = value; }

        private static readonly Stack<CursorInfo> CursorInfos = new(16);

        private static CursorInfo _initialCursorInfo;

        /// <summary>
        /// Gets the number of temporarily stored cursor infos.
        /// </summary>
        public static int Count => CursorInfos.Count;

        /// <summary>
        /// Sets the cursor.
        /// </summary>
        /// <param name="cursorInfo">The cursor info.</param>
        public static void Push(CursorInfo cursorInfo)
        {
            CursorInfos.Push(cursorInfo);
            SetCursor(cursorInfo);
        }

        /// <summary>
        /// Restores the cursor to the state before the most recent call to <see cref="Push"/>.
        /// </summary>
        public static void Pop()
        {
            if (CursorInfos.Count == 0)
            {
                Log.E($"The number of calls to {nameof(Pop)} does not match the number of calls to {nameof(Push)}");
                return;
            }
            CursorInfos.Pop();
            SetCursor(CursorInfos.Count != 0 ? CursorInfos.Peek() : _initialCursorInfo);
        }

        private static void SetCursor(CursorInfo cursorInfo)
        {
            var texture    = cursorInfo.texture;
            var hotspot    = cursorInfo.hotspot;
            var cursorMode = cursorInfo.cursorMode;
            Cursor.SetCursor(texture, hotspot, cursorMode);
        }

#if UNITY_EDITOR
        internal static void Reset()
        {
            if (CursorInfos.Count != 0)
            {
                CursorInfos.Clear();
                SetCursor(_initialCursorInfo);
            }
            _initialCursorInfo = default;
        }
#endif
    }
}
