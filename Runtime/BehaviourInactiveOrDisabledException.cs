using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// The exception thrown when the game object associated with a behaviour is inactive or the behaviour is disabled.
    /// </summary>
    public class BehaviourInactiveOrDisabledException : UnityException
    {
        /// <summary>
        /// The behaviour.
        /// </summary>
        public Behaviour Behaviour { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourInactiveOrDisabledException"/> class with the specified behaviour.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <exception cref="ArgumentNullException"><paramref name="behaviour"/> is <see langword="null"/>.</exception>
        public BehaviourInactiveOrDisabledException(Behaviour behaviour)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourInactiveOrDisabledException"/> class with the specified behaviour and error message.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <param name="message">The message describing the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="behaviour"/> is <see langword="null"/>.</exception>
        public BehaviourInactiveOrDisabledException(Behaviour behaviour, string message) : base(message)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourInactiveOrDisabledException"/> class with the specified behaviour, error message, and inner exception.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        public BehaviourInactiveOrDisabledException(Behaviour behaviour, string message, Exception innerException) :
            base(message, innerException)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }
    }
}
