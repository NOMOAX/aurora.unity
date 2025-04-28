using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为使用单一实例模式的单一行为提供基类。
    /// </summary>
    /// <typeparam name="T">要使用单一实例模式的单一行为的类型。</typeparam>
    /// <remarks>你可以为 <typeparamref name="T"/> 类型添加 <see cref="DoNotDestroyOnLoadAttribute"/> 特性，以便在首次获取或创建 <see cref="Instance"/> 时对该单一实例执行 <see cref="UnityEngine.Object.DontDestroyOnLoad"/>。</remarks>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        /// <summary>
        /// 获取 <typeparamref name="T"/> 的单一实例，若不存在则寻找或创建新的。
        /// </summary>
        /// <exception cref="InvalidOperationException">应用程序即将结束运行，或者在编辑器环境下并且不在播放模式中。</exception>
        public static T Instance => InstanceAlreadyExists != null
                                        ? InstanceAlreadyExists
                                        : InstanceAlreadyExists = FetchOrCreateInstance();

        /// <summary>
        /// 获取 <typeparamref name="T"/> 的单一实例，若不存在则返回 <see langword="null"/>。
        /// </summary>
        public static T InstanceAlreadyExists { get; private set; }

        /// <summary>
        /// 确保单一实例存在。
        /// </summary>
        /// <exception cref="InvalidOperationException">应用程序即将结束运行，或者在编辑器环境下并且不在播放模式中。</exception>
        public static void EnsureInstanceExists()
        {
            if (InstanceAlreadyExists != null)
            {
                return;
            }
            InstanceAlreadyExists = FetchOrCreateInstance();
        }

        private static T FetchOrCreateInstance()
        {
            if (!UnityEnvironment.IsPlaying)
            {
#if UNITY_EDITOR
                throw new InvalidOperationException("在编辑器环境下，并且不在播放模式中，不能执行此操作");
#else
                throw new InvalidOperationException($"应用程序程序已结束，不应该执行此操作；若要避免此异常，请确保 {nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} 的值为 {bool.TrueString}");
#endif
            }
            var        type  = typeof(T);
            var        found = FindObjectOfType<T>();
            GameObject gameObject;
            T          instance;
            if (found != null)
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
                (WithHideFlagsAttribute) Attribute.GetCustomAttribute(type, typeof(WithHideFlagsAttribute));
            if (withHideFlagsAttribute != null)
            {
                gameObject.hideFlags |= withHideFlagsAttribute.HideFlags;
            }
            return instance;
        }
    }
}
