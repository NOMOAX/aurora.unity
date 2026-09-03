using System;

namespace Aurora.Unity
{
    /// <summary>
    /// Marks a type that inherits from <see cref="SingletonBehaviour{T}"/> so that <see cref="UnityEngine.Object.DontDestroyOnLoad"/> is executed on the single instance when it is assigned.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotDestroyOnLoadAttribute : Attribute
    {
    }
}
