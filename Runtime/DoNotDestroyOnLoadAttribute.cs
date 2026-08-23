using System;

namespace Aurora.Unity
{
    /// <summary>
    /// Marks a type that inherits from <see cref="SingletonBehaviour{T}"/> so that <see cref="UnityEngine.Object.DontDestroyOnLoad"/> is executed on the single instance when <see cref="SingletonBehaviour{T}.Instance"/> is first retrieved or created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotDestroyOnLoadAttribute : Attribute
    {
    }
}
