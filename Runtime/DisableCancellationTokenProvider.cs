using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供与游戏物体的激活状态关联的取消令牌。
    /// </summary>
    /// <remarks>不要手动修改此实例的 <see cref="Behaviour.enabled"/> 属性。</remarks>
    /// <seealso cref="GameObjectExtensions.GetDisableCancellationToken"/>
    [DisallowMultipleComponent]
    internal sealed class DisableCancellationTokenProvider : MonoBehaviour
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 获取与游戏物体的激活状态关联的取消令牌。
        /// </summary>
        internal CancellationToken CancellationToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _cancellationTokenSource?.Token ??
                   (UnityEnvironment.IsPlaying && this != null && enabled && gameObject.activeInHierarchy
                        ? (_cancellationTokenSource = new CancellationTokenSource()).Token
                        : new CancellationToken(true));
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

        private void OnEnable()
        {
            /*
             * 这里检查了 _cancellationTokenSource 是否为 null，仅在为 null 时才初始化
             *
             * 假设游戏物体上有另一个叫做 other 的脚本，
             * 并且假设 other.OnEnable 先于 this.OnEnable 执行，
             * 如果在 other.OnEnable 中调用了 this.CancellationToken（通过 GameObjectExtensions.GetDisableCancellationToken 方法），则 _cancellationTokenSource 已经初始化
             */
            _cancellationTokenSource ??= new CancellationTokenSource();
        }

        private void OnDisable()
        {
            CancelAndDisposeCancellationTokenSource();
        }

        private void OnApplicationQuit()
        {
            CancelAndDisposeCancellationTokenSource();
        }
    }
}
