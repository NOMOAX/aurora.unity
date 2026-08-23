using System;
using System.Threading;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="GameObject"/> class.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Sets the layer of the current <see cref="GameObject"/> and its recursive child game objects.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <param name="layer">The layer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
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
        /// Sets the tag of the current <see cref="GameObject"/> and its recursive child game objects.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
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
        /// Gets or adds a component.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <typeparam name="T">The type of the component to get or add.</typeparam>
        /// <returns>The component that was gotten or added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
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
        /// Removes a component.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <typeparam name="T">The type of the component to remove.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
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
        /// Gets a cancellation token tied to the activation state of the current <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <returns>The cancellation token tied to the activation state of the current game object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
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
        /// Destroys all child game objects of the current <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        /// <remarks>Destruction is in reverse order, i.e. the last child game object is destroyed first and the first child game object is destroyed last.</remarks>
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
        /// Immediately destroys all child game objects of the current <see cref="GameObject"/>.
        /// </summary>
        /// <param name="gameObject">This game object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        /// <remarks>Destruction is in reverse order, i.e. the last child game object is destroyed first and the first child game object is destroyed last.</remarks>
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
