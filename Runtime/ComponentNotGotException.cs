using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// The exception thrown when a component of the specified type cannot be obtained.
    /// </summary>
    public class ComponentNotGotException : UnityException
    {
        /// <summary>
        /// The game object.
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// The method used to get a component from the game object.
        /// </summary>
        public GetComponentMethod Method { get; }

        /// <summary>
        /// The component type.
        /// </summary>
        public Type ComponentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectInactiveException"/> class with the specified game object, the method to get a component from the game object, and the component type.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="method">The method used to get a component from the game object.</param>
        /// <param name="componentType">The component type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> or <paramref name="componentType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="componentType"/> is neither an interface type nor a <seealso cref="Component"/> type or its subtype.</exception>
        public ComponentNotGotException(GameObject gameObject, GetComponentMethod method, Type componentType)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            if (componentType is null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }
            if (!componentType.IsInterface || componentType != typeof(Component) ||
                componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
            GameObject    = gameObject;
            Method        = method;
            ComponentType = componentType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectInactiveException"/> class with the specified game object, the method to get a component from the game object, the component type, and an error message.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="method">The method used to get a component from the game object.</param>
        /// <param name="message">The message describing the error.</param>
        /// <param name="componentType">The component type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> or <paramref name="componentType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="componentType"/> is neither an interface type nor a <seealso cref="Component"/> type or its subtype.</exception>
        public ComponentNotGotException(
            GameObject         gameObject,
            GetComponentMethod method,
            string             message,
            Type               componentType) : base(message)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            if (componentType is null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }
            if (!componentType.IsInterface || componentType != typeof(Component) ||
                componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
            GameObject    = gameObject;
            Method        = method;
            ComponentType = componentType;
        }
    }
}
