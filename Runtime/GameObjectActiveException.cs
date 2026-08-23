using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// The exception thrown when a game object is active.
    /// </summary>
    public class GameObjectActiveException : UnityException
    {
        /// <summary>
        /// The game object.
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectActiveException"/> class with the specified game object.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        public GameObjectActiveException(GameObject gameObject)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectActiveException"/> class with the specified game object and error message.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="message">The message describing the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        public GameObjectActiveException(GameObject gameObject, string message) : base(message)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectActiveException"/> class with the specified game object, error message, and inner exception.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        public GameObjectActiveException(GameObject gameObject, string message, Exception innerException) : base(
            message,
            innerException
        )
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }
    }
}
