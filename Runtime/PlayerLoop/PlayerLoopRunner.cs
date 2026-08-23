using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Aurora.Diagnostics;
using Aurora.Interpolations;
using UnityEngine;

namespace Aurora.Unity.PlayerLoop
{
    [DebuggerDisplay(nameof(PlayerLoopRunner) + " ({_phase})")]
    internal sealed class PlayerLoopRunner
    {
        private static readonly Predicate<object> IsNull = obj => obj is null;

        private readonly PlayerLoopPhase _phase;

        private readonly List<IPlayerLoopItem> _items = new();

        private double _trimTime;

        internal PlayerLoopRunner(PlayerLoopPhase phase)
        {
            _phase = phase;

            _trimTime = GetNextTrimTime(Time.realtimeSinceStartupAsDouble);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetNextTrimTime(double currentTime)
        {
            return currentTime + InterpolationUtility.LinearInterpolate(60, 120, RandomUtility.Shared.NextDouble());
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
                if (_items.LastIndexOf(item) is var index and >= 0)
                {
                    _items[index] = null;
                }
                else
#if UNITY_EDITOR
                if (UnityEnvironment.IsPlaying)
#endif
                {
                    Log.W($"The {nameof(IPlayerLoopItem)} to remove was not found");
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
                    // The list's length may increase; do not change this to a foreach statement
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < _items.Count; i++)
                    {
                        if (_items[i] is var item && item is null)
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

                    if (Time.realtimeSinceStartupAsDouble is var currentTime && currentTime > _trimTime)
                    {
                        _trimTime = GetNextTrimTime(currentTime);
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
