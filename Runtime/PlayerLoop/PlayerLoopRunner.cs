using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aurora.Diagnostics;
using UnityEngine;

namespace Aurora.Unity.PlayerLoop
{
    [DebuggerDisplay(nameof(PlayerLoopRunner) + " ({_phase})")]
    internal sealed class PlayerLoopRunner
    {
        private static readonly Predicate<object> IsNull = obj => obj is null;

        private readonly PlayerLoopPhase _phase;

        private readonly List<IPlayerLoopItem> _items = new();

        internal PlayerLoopRunner(PlayerLoopPhase phase)
        {
            _phase = phase;
        }

        internal void Add(IPlayerLoopItem item)
        {
            lock (_items)
            {
                _items.Add(item);
            }
        }

        internal void Remove(IPlayerLoopItem item)
        {
            lock (_items)
            {
                var index = _items.LastIndexOf(item);
                if (index >= 0)
                {
                    _items[index] = null;
                }
                else
#if UNITY_EDITOR
                if (UnityEnvironment.IsPlaying)
#endif
                {
                    Log.W($"找不到要移除的 {nameof(IPlayerLoopItem)}");
                }
            }
        }

#if UNITY_EDITOR
        internal void Clear()
        {
            lock (_items)
            {
                foreach (var disposable in _items.OfType<IDisposable>())
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
                _items.Clear();
            }
        }
#endif

        internal void Run()
        {
            PlayerLoopUtility.CurrentPhase = _phase;
            try
            {
                lock (_items)
                {
                    // 列表的长度可能会增加，不要改为 foreach 语句
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < _items.Count; i++)
                    {
                        var item = _items[i];
                        if (item == null)
                        {
                            continue;
                        }
                        try
                        {
                            item.Run(_phase);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }
                    _items.RemoveAll(IsNull);

                    if (Time.frameCount % 4096 == 0)
                    {
                        _items.TrimExcess();
                    }
                }
            }
            finally
            {
                PlayerLoopUtility.CurrentPhase = null;
            }
        }
    }
}
