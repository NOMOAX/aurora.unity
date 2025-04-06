using System;

namespace Aurora.Unity
{
    /// <summary>
    /// 用于修饰继承 <see cref="SingletonBehaviour{T}"/> 类的类型，以在首次获取或创建 <see cref="SingletonBehaviour{T}.Instance"/> 时对该单一实例执行 <see cref="UnityEngine.Object.DontDestroyOnLoad"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotDestroyOnLoadAttribute : Attribute
    {
    }
}
