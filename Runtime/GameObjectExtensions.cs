using System;
using System.Threading;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="GameObject"/> 类提供扩展方法。
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 设置当前 <see cref="GameObject"/> 以及它的递归子游戏物体所在的层。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <param name="layer">层。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            var list = PredefinedPools<Transform>.List.Get();
            try
            {
                gameObject.GetComponentsInChildren(true, list);
                foreach (var transform in list)
                {
                    transform.gameObject.layer = layer;
                }
            }
            finally
            {
                PredefinedPools<Transform>.List.Return(list);
            }
        }

        /// <summary>
        /// 设置当前 <see cref="GameObject"/> 以及它的递归子游戏物体的标签。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <param name="tag">标签。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public static void SetTagRecursively(this GameObject gameObject, string tag)
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            var list = PredefinedPools<Transform>.List.Get();
            try
            {
                gameObject.GetComponentsInChildren(true, list);
                foreach (var transform in list)
                {
                    transform.gameObject.tag = tag;
                }
            }
            finally
            {
                PredefinedPools<Transform>.List.Return(list);
            }
        }

        /// <summary>
        /// 获取或添加组件。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <typeparam name="T">要获取或添加的组件的类型。</typeparam>
        /// <returns>获取到的或添加的组件。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            return InternalGetOrAddComponent<T>(gameObject);
        }

        private static T InternalGetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out var result) ? result : gameObject.AddComponent<T>();
        }

        /// <summary>
        /// 移除组件。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <typeparam name="T">要移除的组件的类型。</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public static void RemoveComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            if (gameObject.TryGetComponent<T>(out var result))
            {
                Object.Destroy(result);
            }
        }

        /// <summary>
        /// 获取与当前 <see cref="GameObject"/> 的激活状态关联的取消令牌。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <returns>与当前游戏物体的激活状态关联的取消令牌。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public static CancellationToken GetDisableToken(this GameObject gameObject)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            return !gameObject
                       ? new CancellationToken(true)
                       : InternalGetOrAddComponent<DisableTokenProvider>(gameObject).CancellationToken;
        }

        /// <summary>
        /// 销毁当前 <see cref="GameObject"/> 的所有子游戏物体。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        /// <remarks>销毁按倒序进行，即首先销毁最后的子游戏物体，最后销毁第一个子游戏物体。</remarks>
        public static void DestroyChildren(this GameObject gameObject)
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            var transform = gameObject.transform;
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var childTransform  = transform.GetChild(i);
                var childGameObject = childTransform.gameObject;
                Object.Destroy(childGameObject);
            }
        }

        /// <summary>
        /// 立即销毁当前 <see cref="GameObject"/> 的所有子游戏物体。
        /// </summary>
        /// <param name="gameObject">此游戏物体。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        /// <remarks>销毁按倒序进行，即首先销毁最后的子游戏物体，最后销毁第一个子游戏物体。</remarks>
        public static void DestroyChildrenImmediate(this GameObject gameObject)
        {
            if (!gameObject)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            var transform = gameObject.transform;
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var childTransform  = transform.GetChild(i);
                var childGameObject = childTransform.gameObject;
                Object.DestroyImmediate(childGameObject);
            }
        }
    }
}
