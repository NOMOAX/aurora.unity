using System;
using System.Collections.Generic;
using System.Threading;
using Aurora.Pooling;
using UnityEditor;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// 定义预编译符号范围。
    /// </summary>
    public sealed class DefineSymbolScope : IDisposable
    {
        private readonly BuildTargetGroup _buildTargetGroup;

        private List<string> _list;

        private bool _dirty;

        public DefineSymbolScope()
        {
            _buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            _list             = PredefinedPools<string>.List.Get();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup, out var symbols);
            _list.AddRange(symbols);
        }

        public DefineSymbolScope(BuildTargetGroup buildTargetGroup)
        {
            _buildTargetGroup = buildTargetGroup;
            _list             = PredefinedPools<string>.List.Get();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup, out var symbols);
            _list.AddRange(symbols);
        }

        /// <summary>
        /// 添加预编译符号。
        /// </summary>
        /// <param name="symbol">要添加的预编译符号。</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> 不是一个合法的预编译符号。</exception>
        public void Add(string symbol)
        {
            UnityEditorUtility.ThrowIfSymbolInvalid(symbol);
            if (_list.Contains(symbol))
            {
                return;
            }
            _list.Add(symbol);
            _dirty = true;
        }

        /// <summary>
        /// 移除预编译符号。
        /// </summary>
        /// <param name="symbol">要移除的预编译符号。</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> 不是一个合法的预编译符号。</exception>
        public void Remove(string symbol)
        {
            UnityEditorUtility.ThrowIfSymbolInvalid(symbol);
            var index = _list.LastIndexOf(symbol);
            if (index < 0)
            {
                return;
            }
            _list.RemoveAt(index);
            _dirty = true;
        }

        /// <summary>
        /// 获取一个值，这个值指示指定的预编译符号是否已定义。
        /// </summary>
        /// <param name="symbol">预编译符号。</param>
        /// <returns>如果 <paramref name="symbol"/> 已定义，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> 不是一个合法的预编译符号。</exception>
        public bool IsDefined(string symbol)
        {
            UnityEditorUtility.ThrowIfSymbolInvalid(symbol);
            return _list.Contains(symbol);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var list = _list;
            if (list == null || Interlocked.CompareExchange(ref _list, null, list) != list)
            {
                return;
            }
            if (_dirty)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, list.ToArray());
                _dirty = false;
            }
            PredefinedPools<string>.List.Return(list);
        }
    }
}
