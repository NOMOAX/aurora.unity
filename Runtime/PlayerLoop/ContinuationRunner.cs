using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Aurora.Diagnostics;

namespace Aurora.Unity.PlayerLoop
{
    [DebuggerDisplay(nameof(ContinuationRunner) + " ({_playerLoopPhase})")]
    internal sealed class ContinuationRunner
    {
        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly ConcurrentQueue<Invocation> _continuations;

        internal ContinuationRunner(PlayerLoopPhase playerLoopPhase)
        {
            _playerLoopPhase = playerLoopPhase;
            _continuations   = new ConcurrentQueue<Invocation>();
        }

        internal void Add(Action continuation)
        {
            _continuations.Enqueue(new InvocationAction(continuation));
        }

        internal void Add(Action<object> continuation, object state)
        {
            _continuations.Enqueue(new InvocationActionWithState(continuation, state));
        }

        internal void Add(Invocation continuationInvocation)
        {
            _continuations.Enqueue(continuationInvocation);
        }

#if UNITY_EDITOR
        internal void Clear()
        {
            while (_continuations.TryDequeue(out _))
            {
            }
        }
#endif

        internal void Run()
        {
            PlayerLoopUtility.CurrentPlayerLoopPhase = _playerLoopPhase;
            try
            {
                while (_continuations.TryDequeue(out var invocation))
                {
                    try
                    {
                        invocation.Invoke();
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
            }
            finally
            {
                PlayerLoopUtility.CurrentPlayerLoopPhase = null;
            }
        }
    }
}
