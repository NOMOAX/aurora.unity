using System;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Pooling
{
    /// <summary>
    /// 管理池中的组件的轻量级策略。
    /// </summary>
    /// <typeparam name="T">组件的类型。</typeparam>
    public class PooledComponentPolicySlim<T> : IPooledObjectPolicy<T> where T : Component
    {
        private readonly T _original;

        private readonly Transform _container;

        private readonly bool _optimizeName;

        /// <summary>
        /// 初始化 <see cref="PooledComponentPolicySlim{T}"/> 类的新实例。
        /// </summary>
        /// <param name="original">原本。</param>
        /// <param name="container">容器。</param>
        /// <param name="optimizeName">是否要优化副本的名称。</param>
        /// <exception cref="ArgumentNullException"><paramref name="original"/> 为 <see langword="null"/>。</exception>
        public PooledComponentPolicySlim(T original, Transform container, bool optimizeName)
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
            if (!UnityEnvironment.IsPlaying)
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }
            OnSettingTransformParent(obj);
            obj.transform.SetParent(_container, false);
            OnSetTransformParent(obj);
            return true;
        }

        /// <summary>
        /// 在将组件放入池时，将与组件关联的变换的父变换设置为在构造函数中传入的容器之前执行此方法。
        /// <br/>
        /// 你可以重写此方法，以执行一些初始化操作。
        /// </summary>
        /// <param name="obj">组件。</param>
        protected virtual void OnSettingTransformParent(T obj)
        {
        }

        /// <summary>
        /// 在将组件放入池时，将与组件关联的变换的父变换设置为在构造函数中传入的容器之后执行此方法。
        /// <br/>
        /// 你可以重写此方法，以执行一些初始化操作。
        /// </summary>
        /// <param name="obj">组件。</param>
        protected virtual void OnSetTransformParent(T obj)
        {
        }

        /// <inheritdoc />
        /// <remarks>会销毁与组件关联的游戏物体，而不是仅销毁组件。</remarks>
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
            Object.Destroy(obj.gameObject);
        }
    }
}
