using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供与游戏物体的生存状态关联的取消令牌。
    /// </summary>
    /// <seealso cref="GameObjectExtensions.GetDestroyCancellationToken"/>
    [DisallowMultipleComponent]
    internal sealed class DestroyCancellationTokenProvider : MonoBehaviour
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 获取与游戏物体的生存状态关联的取消令牌。
        /// </summary>
        internal CancellationToken CancellationToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cancellationTokenSource?.Token ?? new CancellationToken(true);
        }

        private DestroyCancellationTokenProvider()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void CancelAndDisposeCancellationTokenSource()
        {
            if (_cancellationTokenSource is null)
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

        private void Awake()
        {
            hideFlags |= HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable |
                         HideFlags.DontSaveInBuild;
        }

        private void OnDestroy()
        {
            CancelAndDisposeCancellationTokenSource();
        }

        private void OnApplicationQuit()
        {
            CancelAndDisposeCancellationTokenSource();
        }
    }
}
