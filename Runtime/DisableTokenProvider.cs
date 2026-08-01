using System.Runtime.CompilerServices;
using System.Threading;
using Aurora.Unity.Diagnostics;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供与游戏物体的激活状态关联的取消令牌。
    /// </summary>
    /// <remarks>不要手动修改此实例的 <see cref="Behaviour.enabled"/> 属性。</remarks>
    /// <seealso cref="GameObjectExtensions.GetDisableToken"/>
    [DisallowMultipleComponent]
    internal sealed class DisableTokenProvider : MonoBehaviour
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 获取与游戏物体的激活状态关联的取消令牌。
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

                if (!UnityEnvironment.IsPlaying || !this || !InlineAssert.IsTrue(
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
