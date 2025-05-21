using System.Collections.Generic;
using Aurora.Diagnostics;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供设置与恢复光标的方法。
    /// </summary>
    /// <remarks>如果你在 PlayerSettings 中设置了默认光标，则应在程序启动后调用 <see cref="set_InitialCursorInfo"/>。</remarks>
    public static class CursorStack
    {
        /// <summary>
        /// 设置初始光标信息。
        /// </summary>
        /// <remarks>如果你在 PlayerSettings 中设置了默认光标，则应在程序启动后调用 <see cref="set_InitialCursorInfo"/>。</remarks>
        public static CursorInfo InitialCursorInfo { set => _initialCursorInfo = value; }

        private static readonly Stack<CursorInfo> CursorInfos = new(16);

        private static CursorInfo _initialCursorInfo;

        /// <summary>
        /// 获取暂存的光标信息的数量。
        /// </summary>
        public static int Count => CursorInfos.Count;

        /// <summary>
        /// 设置光标。
        /// </summary>
        /// <param name="cursorInfo">光标信息。</param>
        public static void Push(CursorInfo cursorInfo)
        {
            CursorInfos.Push(cursorInfo);
            SetCursor(cursorInfo);
        }

        /// <summary>
        /// 恢复光标到上一次调用 <see cref="Push"/> 之前的状态。
        /// </summary>
        public static void Pop()
        {
            if (CursorInfos.Count == 0)
            {
                Log.E($"调用 {nameof(Pop)} 与调用 {nameof(Push)} 的次数不匹配");
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
