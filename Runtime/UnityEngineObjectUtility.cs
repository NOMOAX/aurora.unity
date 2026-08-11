using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="UnityEngine.Object"/> 类提供工具方法。
    /// </summary>
    public static class UnityEngineObjectUtility
    {
        private static readonly Func<Object, IntPtr> FuncGetCachedPtr;

        private static readonly Func<int, bool> FuncDoesObjectWithInstanceIdExist;

        static UnityEngineObjectUtility()
        {
            var unityEngineObjectType = typeof(Object);

            var getCachedPtrMethodInfo = unityEngineObjectType.GetMethod(
                "GetCachedPtr",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Standard | CallingConventions.HasThis,
                Type.EmptyTypes,
                null
            );
            Assert.IsNotNull(getCachedPtrMethodInfo);
            FuncGetCachedPtr = (Func<Object, IntPtr>)Delegate.CreateDelegate(
                typeof(Func<Object, IntPtr>),
                getCachedPtrMethodInfo
            );

            var doesObjectWithInstanceIdExistMethodInfo = unityEngineObjectType.GetMethod(
                "DoesObjectWithInstanceIDExist",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                CallingConventions.Standard,
                new[] { typeof(int) },
                null
            );
            Assert.IsNotNull(doesObjectWithInstanceIdExistMethodInfo);
            FuncDoesObjectWithInstanceIdExist = (Func<int, bool>)Delegate.CreateDelegate(
                typeof(Func<int, bool>),
                doesObjectWithInstanceIdExistMethodInfo
            );
        }

        /// <summary>
        /// 获取由 <see cref="Object"/> 实例包装的原生 C++ 对象的内存地址。
        /// </summary>
        /// <param name="obj"><see cref="Object"/> 实例。</param>
        /// <returns>由 <paramref name="obj"/> 包装的原生 C++ 对象的内存地址。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> 为 <see langword="null"/>。</exception>
        public static IntPtr GetCachedPtr(Object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            var cachedPtr = InternalGetCachedPtr(obj);
            return cachedPtr;
        }

        /// <summary>
        /// 获取 <see cref="Object"/> 实例的标识符。
        /// </summary>
        /// <param name="obj"><see cref="Object"/> 实例。</param>
        /// <returns><paramref name="obj"/> 的标识符。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> 为 <see langword="null"/>。</exception>
        public static int GetInstanceId(Object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            var instanceId = InternalGetInstanceId(obj);
            return instanceId;
        }

        /// <summary>
        /// 获取一个值，这个值指示 <see cref="Object"/> 实例是否处于存活状态。
        /// </summary>
        /// <param name="obj"><see cref="Object"/> 实例。</param>
        /// <returns>如果 <paramref name="obj"/> 处于存活状态，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> 为 <see langword="null"/>。</exception>
        public static bool IsAlive(Object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            var isAlive = InternalIsAlive(obj);
            return isAlive;
        }

        /// <summary>
        /// 获取一个值，这个值指示是否存在具有指定标识符的 <see cref="Object"/> 实例。
        /// </summary>
        /// <param name="instanceId">标识符。</param>
        /// <returns>如果存在标识符为 <paramref name="instanceId"/> 的 <see cref="Object"/> 实例，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool DoesObjectWithInstanceIdExist(int instanceId)
        {
            var objectWithInstanceIdExist = InternalDoesObjectWithInstanceIdExist(instanceId);
            return objectWithInstanceIdExist;
        }

        /// <summary>
        /// 获取一个值，这个值指示两个 <see cref="Object"/> 实例是否相等。
        /// </summary>
        /// <param name="objA">第一个 <see cref="Object"/> 实例。</param>
        /// <param name="objB">第二个 <see cref="Object"/> 实例。</param>
        /// <returns>如果 <paramref name="objA"/> 和 <paramref name="objB"/> 的引用相等，或者其中一个处于销毁状态而另一个为 <see langword="null"/>，或者它们的标识符相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <remarks>此方法是 <see cref="Object.op_Equality"/> 去除 Unity 主线程校验的版本。</remarks>
        public static bool Equals(Object objA, Object objB)
        {
            if (ReferenceEquals(objA, objB))
            {
                return true;
            }
            if (objB is null)
            {
                var aIsAlive = InternalIsAlive(objA);
                return !aIsAlive;
            }
            if (objA is null)
            {
                var bIsAlive = InternalIsAlive(objB);
                return !bIsAlive;
            }
            var aId = InternalGetInstanceId(objA);
            var bId = InternalGetInstanceId(objB);
            return aId == bId;
        }

        private static IntPtr InternalGetCachedPtr(Object obj)
        {
            var cachedPtr = FuncGetCachedPtr(obj);
            return cachedPtr;
        }

        internal static int InternalGetInstanceId(Object obj)
        {
            var instanceId = obj.GetHashCode();
            return instanceId;
        }

        private static bool InternalIsAlive(Object obj)
        {
            var cachedPtr = InternalGetCachedPtr(obj);
            if (cachedPtr != IntPtr.Zero)
            {
                return true;
            }
            if (obj is MonoBehaviour or ScriptableObject)
            {
                return false;
            }
            var instanceId                = InternalGetInstanceId(obj);
            var objectWithInstanceIdExist = InternalDoesObjectWithInstanceIdExist(instanceId);
            return objectWithInstanceIdExist;
        }

        private static bool InternalDoesObjectWithInstanceIdExist(int instanceId)
        {
            var objectWithInstanceIdExist = FuncDoesObjectWithInstanceIdExist(instanceId);
            return objectWithInstanceIdExist;
        }
    }
}
