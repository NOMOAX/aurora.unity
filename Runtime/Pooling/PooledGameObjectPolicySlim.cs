using System;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// The lightweight policy for managing pooled game objects.
    /// </summary>
    public sealed class PooledGameObjectPolicySlim : IPooledObjectPolicy<GameObject>
    {
        private readonly GameObject _original;

        private readonly Transform _container;

        private readonly bool _optimizeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledGameObjectPolicySlim"/> class.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="container">The container.</param>
        /// <param name="optimizeName">Whether to optimize the name of the copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="original"/> is <see langword="null"/>.</exception>
        public PooledGameObjectPolicySlim(GameObject original, Transform container, bool optimizeName)
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
        public GameObject Original => _original;

        /// <summary>
        /// The container.
        /// </summary>
        public Transform Container => _container;

        /// <inheritdoc />
        public GameObject Create()
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
        public void Get(GameObject obj)
        {
        }

        /// <inheritdoc />
        public bool Return(GameObject obj)
        {
            if (!UnityEnvironment.IsPlaying)
            {
                return false;
            }
            if (!obj)
            {
                return false;
            }
            obj.transform.SetParent(_container, false);
            return true;
        }

        /// <inheritdoc />
        public void Dispose(GameObject obj)
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
