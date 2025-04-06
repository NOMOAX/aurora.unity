using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aurora.Collections;
using Aurora.Diagnostics;
using UnityEngine;

namespace Aurora.Unity.PlayerLoop
{
    [DebuggerDisplay(nameof(PlayerLoopRunner) + " ({_playerLoopPhase})")]
    internal sealed class PlayerLoopRunner
    {
        private static readonly Predicate<Bucket> PredicateBucketWillBeRemoved = BucketWillBeRemoved;

        private static readonly Func<Bucket, IPlayerLoopItem, bool> FuncBucketRemovableAndEqualToItem =
            BucketRemovableAndEqualToItem;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly List<Bucket> _buckets = new List<Bucket>();

        internal PlayerLoopRunner(PlayerLoopPhase playerLoopPhase)
        {
            _playerLoopPhase = playerLoopPhase;
        }

        internal void Add(IPlayerLoopItem item)
        {
            lock (_buckets)
            {
                _buckets.Add(new Bucket(item));
            }
        }

        internal void Remove(IPlayerLoopItem item)
        {
            lock (_buckets)
            {
                var bucket = _buckets.FindLast(FuncBucketRemovableAndEqualToItem, item);
                // 允许在 Run 中调用 Remove，因此这里不执行移除，实际在 Run 中移除
                if (bucket != null)
                {
                    bucket.MarkAsWillBeRemoved();
                }
                else
                {
                    Log.W($"找不到要移除的 {nameof(IPlayerLoopItem)}");
                }
            }
        }

#if UNITY_EDITOR
        internal void Clear()
        {
            lock (_buckets)
            {
                foreach (var disposable in _buckets.Select(GetItem).OfType<IDisposable>())
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
                _buckets.Clear();
            }

            static IPlayerLoopItem GetItem(Bucket bucket)
            {
                return bucket.Item;
            }
        }

#endif

        internal void Run()
        {
            PlayerLoopUtility.CurrentPlayerLoopPhase = _playerLoopPhase;
            try
            {
                lock (_buckets)
                {
                    // 列表的长度可能会增加，不要改为 foreach 语句
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < _buckets.Count; i++)
                    {
                        var bucket = _buckets[i];
                        if (bucket.WillBeRemoved)
                        {
                            continue;
                        }
                        try
                        {
                            bucket.Item.Run(_playerLoopPhase);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }
                    // 删除被标记为将要被删除的元素
                    _buckets.RemoveAll(PredicateBucketWillBeRemoved);

                    if (Time.frameCount % 4096 == 0)
                    {
                        _buckets.TrimExcess();
                    }
                }
            }
            finally
            {
                PlayerLoopUtility.CurrentPlayerLoopPhase = null;
            }
        }

        private static bool BucketWillBeRemoved(Bucket e)
        {
            return e.WillBeRemoved;
        }

        private static bool BucketRemovableAndEqualToItem(Bucket bucket, IPlayerLoopItem item)
        {
            return !bucket.WillBeRemoved && bucket.Item == item;
        }

        private sealed class Bucket
        {
            internal readonly IPlayerLoopItem Item;

            internal bool WillBeRemoved { get; private set; }

            internal Bucket(IPlayerLoopItem item)
            {
                Item = item;
            }

            internal void MarkAsWillBeRemoved()
            {
                WillBeRemoved = true;
            }
        }
    }
}
