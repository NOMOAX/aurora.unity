using System;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// The lightweight policy for managing pooled components.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    public class PooledComponentPolicySlim<T> : IPooledObjectPolicy<T> where T : Component
    {
        private readonly T _original;

        private readonly Transform _container;

        private readonly bool _optimizeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledComponentPolicySlim{T}"/> class.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="container">The container.</param>
        /// <param name="optimizeName">Whether to optimize the name of the copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="original"/> is <see langword="null"/>.</exception>
        public PooledComponentPolicySlim(T original, Transform container, bool optimizeName)
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
            if (!UnityEnvironment.IsPlaying)
            {
                return false;
            }
            if (!obj)
            {
                return false;
            }
            OnReturning(obj);
            obj.transform.SetParent(_container, false);
            OnReturn(obj);
            return true;
        }

        /// <summary>
        /// Executed before the component is put into the pool.
        /// <br/>
        /// Override this method to perform extra cleanup operations.
        /// </summary>
        /// <param name="obj">The component.</param>
        protected virtual void OnReturning(T obj)
        {
        }

        /// <summary>
        /// Executed after the component is put into the pool.
        /// <br/>
        /// Override this method to perform extra cleanup operations.
        /// </summary>
        /// <param name="obj">The component.</param>
        protected virtual void OnReturn(T obj)
        {
        }

        /// <inheritdoc />
        /// <remarks>Destroys the game object associated with the component rather than only the component.</remarks>
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
            Object.Destroy(obj.gameObject);
        }
    }
}
