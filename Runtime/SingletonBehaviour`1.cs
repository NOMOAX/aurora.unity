using System;
using System.Runtime.CompilerServices;
using Aurora.Diagnostics;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a base class for a <see cref="MonoBehaviour"/> that uses the singleton pattern.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="MonoBehaviour"/> that uses the singleton pattern.</typeparam>
    /// <remarks>
    /// The single instance is assigned when it awakes, or when it is found by <see cref="FindInstance"/>.
    /// <br/>
    /// You can add the <see cref="DoNotDestroyOnLoadAttribute"/> attribute to <typeparamref name="T"/> to execute <see cref="UnityEngine.Object.DontDestroyOnLoad"/> on the single instance when it is assigned, or add the <see cref="WithHideFlagsAttribute"/> attribute to set <see cref="UnityEngine.Object.hideFlags"/> on the single instance when it is assigned.
    /// </remarks>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;

        /// <summary>
        /// Gets the single instance of <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>The instance is assigned when it awakes, or when <see cref="FindInstance"/> is explicitly called.</remarks>
        public static T Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _instance;
        }

        /// <summary>
        /// Explicitly finds the single instance of <typeparamref name="T"/> in the scene, including inactive objects.
        /// </summary>
        /// <exception cref="InvalidOperationException">The operation is performed while not in play mode (see <see cref="UnityEnvironment.IsPlaying"/>).</exception>
        /// <remarks>
        /// For inactive or disabled instances, <c>Awake</c> may not have executed in time, so <see cref="Instance"/> would not be assigned yet.
        /// <br/>
        /// Call this method to locate the instance and assign <see cref="Instance"/>.
        /// <br/>
        /// Does nothing if <see cref="Instance"/> has already been assigned; logs a warning if no instance exists in the scene.
        /// </remarks>
        public static void FindInstance()
        {
            ThrowIfNotPlaying();
            if (_instance)
            {
                return;
            }
            _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (_instance)
            {
                ApplyInstanceAttributes();
            }
            else
            {
                Log.W($"Could not find a singleton instance of {typeof(T).Name} in the scene.");
            }
        }

        /// <summary>
        /// Explicitly creates a new <see cref="GameObject"/> with a <typeparamref name="T"/> component and assigns it as the single instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">The operation is performed while not in play mode (see <see cref="UnityEnvironment.IsPlaying"/>), or the single instance has already been assigned.</exception>
        /// <remarks>
        /// Use this method for singleton types that are not present in the scene and thus cannot be found by <see cref="FindInstance"/>. The created <see cref="GameObject"/> is named after <typeparamref name="T"/>.
        /// <br/>
        /// The instance is assigned and its attributes are applied before the <see cref="GameObject"/> is activated, so the created instance's <c>Awake</c> does not assign it again.
        /// </remarks>
        public static void CreateInstance()
        {
            ThrowIfNotPlaying();
            ThrowIfInstanceAlreadyExists();
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.SetActive(false);
            _instance = gameObject.AddComponent<T>();
            ApplyInstanceAttributes();
            gameObject.SetActive(true);
        }

        private static void ApplyInstanceAttributes()
        {
            var type       = typeof(T);
            var gameObject = _instance.gameObject;
            var attributes = Attribute.GetCustomAttributes(type);
            if (Array.Exists(attributes, IsDoNotDestroyOnLoadAttribute))
            {
                DontDestroyOnLoad(gameObject);
            }
            if (Array.Find(attributes, IsWithHideFlagsAttribute) is WithHideFlagsAttribute withHideFlagsAttribute)
            {
                gameObject.hideFlags |= withHideFlagsAttribute.HideFlags;
            }

            static bool IsDoNotDestroyOnLoadAttribute(Attribute attribute)
            {
                return attribute is DoNotDestroyOnLoadAttribute;
            }

            static bool IsWithHideFlagsAttribute(Attribute e)
            {
                return e is WithHideFlagsAttribute;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfNotPlaying()
        {
            if (!UnityEnvironment.IsPlaying)
            {
                throw new InvalidOperationException(
#if UNITY_EDITOR
                    "This operation is only valid in play mode."
#else
                    $"The application is no longer running, so this operation is invalid. To avoid this exception, ensure that the value of {nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} is {true}."
#endif
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfInstanceAlreadyExists()
        {
            if (_instance)
            {
                ThrowInvalidOperationExceptionForAnotherInstanceAlreadyExists();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowInvalidOperationExceptionForAnotherInstanceAlreadyExists()
        {
            throw new InvalidOperationException($"Another instance of the singleton {typeof(T).Name} already exists.");
        }

        /// <summary>
        /// Assigns the single instance when this <see cref="MonoBehaviour"/> awakes.
        /// </summary>
        /// <exception cref="InvalidOperationException">A singleton instance already exists, and it was not assigned by <see cref="FindInstance"/> or <see cref="CreateInstance"/>.</exception>
        /// <remarks>
        /// If the instance has already been assigned, this method does nothing when it was explicitly found via <see cref="FindInstance"/> or created via <see cref="CreateInstance"/>;
        /// otherwise, a duplicate instance is treated as a programming error and throws an exception.
        /// </remarks>
        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = (T)this;
                ApplyInstanceAttributes();
            }
            else if (!ReferenceEquals(_instance, this))
            {
                ThrowInvalidOperationExceptionForAnotherInstanceAlreadyExists();
            }
        }
    }
}
