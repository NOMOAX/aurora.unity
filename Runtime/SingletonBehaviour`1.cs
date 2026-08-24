using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a base class for a singleton behaviour that uses the singleton pattern.
    /// </summary>
    /// <typeparam name="T">The type of the singleton behaviour that uses the singleton pattern.</typeparam>
    /// <remarks>You can add the <see cref="DoNotDestroyOnLoadAttribute"/> attribute to the <typeparamref name="T"/> type to execute <see cref="UnityEngine.Object.DontDestroyOnLoad"/> on the single instance when <see cref="Instance"/> is first retrieved or created.</remarks>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        /// <summary>
        /// Gets the single instance of <typeparamref name="T"/>, or finds or creates a new one if it does not exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">The application is about to quit, or in the editor environment and not in play mode.</exception>
        public static T Instance => InstanceAlreadyExists
                                        ? InstanceAlreadyExists
                                        : InstanceAlreadyExists = FetchOrCreateInstance();

        /// <summary>
        /// Gets the single instance of <typeparamref name="T"/>, or returns <see langword="null"/> if it does not exist.
        /// </summary>
        public static T InstanceAlreadyExists { get; private set; }

        /// <summary>
        /// Ensures the single instance exists.
        /// </summary>
        /// <exception cref="InvalidOperationException">The application is about to quit, or in the editor environment and not in play mode.</exception>
        public static void EnsureInstanceExists()
        {
            if (!InstanceAlreadyExists)
            {
                InstanceAlreadyExists = FetchOrCreateInstance();
            }
        }

        private static T FetchOrCreateInstance()
        {
            if (!UnityEnvironment.IsPlaying)
            {
#if UNITY_EDITOR
                throw new InvalidOperationException(
                    "This operation cannot be performed in the editor environment while not in play mode"
                );
#else
                throw new InvalidOperationException(
                    $"The application has ended; this operation should not be performed. To avoid this exception, ensure that the value of {nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} is {bool.TrueString}"
                );
#endif
            }
            var        type  = typeof(T);
            var        found = FindObjectOfType<T>();
            GameObject gameObject;
            T          instance;
            if (found)
            {
                gameObject = found.gameObject;
                instance   = found;
            }
            else
            {
                gameObject = new GameObject(type.Name);
                instance   = gameObject.AddComponent<T>();
            }
            if (Attribute.GetCustomAttribute(type, typeof(DoNotDestroyOnLoadAttribute)) != null)
            {
                DontDestroyOnLoad(gameObject);
            }
            var withHideFlagsAttribute =
                (WithHideFlagsAttribute)Attribute.GetCustomAttribute(type, typeof(WithHideFlagsAttribute));
            if (withHideFlagsAttribute != null)
            {
                gameObject.hideFlags |= withHideFlagsAttribute.HideFlags;
            }
            return instance;
        }
    }
}
