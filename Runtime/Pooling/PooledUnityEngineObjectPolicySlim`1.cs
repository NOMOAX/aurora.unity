using System;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// 管理池中的 Unity 对象的轻量级策略。
    /// </summary>
    /// <typeparam name="T">Unity 对象的类型。</typeparam>
    public sealed class PooledUnityEngineObjectPolicySlim<T> : IPooledObjectPolicy<T> where T : Object
    {
        private readonly T _original;

        private readonly Transform _container;

        private readonly bool _optimizeName;

        /// <summary>
        /// 初始化 <see cref="PooledUnityEngineObjectPolicySlim{T}"/> 类的新实例。
        /// </summary>
        /// <param name="original">原本。</param>
        /// <param name="container">容器。</param>
        /// <param name="optimizeName">是否要优化副本的名称。</param>
        /// <exception cref="ArgumentNullException"><paramref name="original"/> 为 <see langword="null"/>。</exception>
        public PooledUnityEngineObjectPolicySlim(T original, Transform container, bool optimizeName)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }
            _original     = original;
            _container    = container;
            _optimizeName = optimizeName;
        }

        /// <summary>
        /// 原本。
        /// </summary>
        public T Original => _original;

        /// <summary>
        /// 容器。
        /// </summary>
        public Transform Container => _container;

        /// <inheritdoc />
        public T Create()
        {
            if (!UnityEnvironment.IsPlaying)
            {
                throw new InvalidOperationException();
            }
            var obj = Object.Instantiate(_original, _container);
            if (_optimizeName)
            {
                UnityUtility.OptimizeName(obj);
            }
            return obj;
        }

        /// <inheritdoc />
        public void Get(T obj)
        {
        }

        /// <inheritdoc />
        public bool Return(T obj)
        {
            return UnityEnvironment.IsPlaying && obj != null;
        }

        /// <inheritdoc />
        public void Dispose(T obj)
        {
            if (!UnityEnvironment.IsPlaying)
            {
                return;
            }
            if (obj == null)
            {
                return;
            }
            Object.Destroy(obj);
        }
    }
}
