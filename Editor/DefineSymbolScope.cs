using System;
using System.Collections.Generic;
using System.Threading;
using Aurora.Pooling;
using UnityEditor;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// A scope for defining preprocessor symbols.
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
        /// Adds a preprocessor symbol.
        /// </summary>
        /// <param name="symbol">The preprocessor symbol to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> is not a valid preprocessor symbol.</exception>
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
        /// Removes a preprocessor symbol.
        /// </summary>
        /// <param name="symbol">The preprocessor symbol to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> is not a valid preprocessor symbol.</exception>
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
        /// Gets a value indicating whether the specified preprocessor symbol is defined.
        /// </summary>
        /// <param name="symbol">The preprocessor symbol.</param>
        /// <returns><see langword="true"/> if <paramref name="symbol"/> is defined; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="symbol"/> is not a valid preprocessor symbol.</exception>
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
