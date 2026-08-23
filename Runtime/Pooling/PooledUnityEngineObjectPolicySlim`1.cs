using System;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// The lightweight policy for managing pooled Unity objects.
    /// </summary>
    /// <typeparam name="T">The type of the Unity object.</typeparam>
    public sealed class PooledUnityEngineObjectPolicySlim<T> : IPooledObjectPolicy<T> where T : Object
    {
        private readonly T _original;

        private readonly Transform _container;

        private readonly bool _optimizeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledUnityEngineObjectPolicySlim{T}"/> class.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="container">The container.</param>
        /// <param name="optimizeName">Whether to optimize the name of the copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="original"/> is <see langword="null"/>.</exception>
        public PooledUnityEngineObjectPolicySlim(T original, Transform container, bool optimizeName)
        {
            if (!original)
            {
                throw new ArgumentNullException(nameof(original));
            }
            _original     = original;
            _container    = container;
            _optimizeName = optimizeName;
        }

        /// <summary>
        /// The original.
        /// </summary>
        public T Original => _original;

        /// <summary>
        /// The container.
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
            return UnityEnvironment.IsPlaying && obj;
        }

        /// <inheritdoc />
        public void Dispose(T obj)
        {
            if (!UnityEnvironment.IsPlaying)
            {
                return;
            }
            if (!obj)
            {
                return;
            }
            Object.Destroy(obj);
        }
    }
}
