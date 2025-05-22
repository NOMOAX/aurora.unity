using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 处理 <see cref="PrefabLessView"/> 的界面处理程序。
    /// </summary>
    public sealed class PrefabLessViewHandler : ViewHandler
    {
        /// <summary>
        /// 获取单一实例。
        /// </summary>
        public static PrefabLessViewHandler Instance { get; } = new();

        private PrefabLessViewHandler()
        {
        }

        /// <inheritdoc />
        public override Type HandleableLeastDerivedViewType => typeof(PrefabLessView);

        /// <inheritdoc />
        public override Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(cancellationToken);
            }
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.SetActive(false);
            var t = gameObject.AddComponent<T>();
            return Task.FromResult(t);
        }
    }
}
