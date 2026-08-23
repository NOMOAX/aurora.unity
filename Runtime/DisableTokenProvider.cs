using System.Runtime.CompilerServices;
using System.Threading;
using Aurora.Unity.Diagnostics;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a cancellation token tied to the activation state of a game object.
    /// </summary>
    /// <remarks>Do not manually modify the <see cref="Behaviour.enabled"/> property of this instance.</remarks>
    /// <seealso cref="GameObjectExtensions.GetDisableToken"/>
    [DisallowMultipleComponent]
    internal sealed class DisableTokenProvider : MonoBehaviour
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the cancellation token tied to the activation state of the game object.
        /// </summary>
        internal CancellationToken CancellationToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_cancellationTokenSource != null)
                {
                    return _cancellationTokenSource.Token;
                }

                if (!UnityEnvironment.IsPlaying || !this || !InlineAssertNoThrow.IsTrue(
                        enabled,
                        "Never disable this " + nameof(UnityEngine) + "." + nameof(Behaviour) + " instance directly!"
                    ) || !gameObject.activeInHierarchy)
                {
                    return new CancellationToken(true);
                }
                _cancellationTokenSource = new CancellationTokenSource();
                return _cancellationTokenSource.Token;
            }
        }

        private void Awake()
        {
            hideFlags |= HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable |
                         HideFlags.DontSaveInBuild;
        }

        private void OnEnable()
        {
            if (_cancellationTokenSource != null)
            {
                return;
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }
            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void OnApplicationQuit()
        {
            OnDisable();
        }
    }
}
