using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="UnityEngine.Object"/> class.
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
        /// Gets the memory address of the native C++ object wrapped by the <see cref="Object"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> instance.</param>
        /// <returns>The memory address of the native C++ object wrapped by <paramref name="obj"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
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
        /// Gets the identifier of the <see cref="Object"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> instance.</param>
        /// <returns>The identifier of <paramref name="obj"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
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
        /// Gets a value indicating whether the <see cref="Object"/> instance is alive.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is alive; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
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
        /// Gets a value indicating whether an <see cref="Object"/> instance with the specified identifier exists.
        /// </summary>
        /// <param name="instanceId">The identifier.</param>
        /// <returns><see langword="true"/> if an <see cref="Object"/> instance with identifier <paramref name="instanceId"/> exists; otherwise <see langword="false"/>.</returns>
        public static bool DoesObjectWithInstanceIdExist(int instanceId)
        {
            var objectWithInstanceIdExist = InternalDoesObjectWithInstanceIdExist(instanceId);
            return objectWithInstanceIdExist;
        }

        /// <summary>
        /// Gets a value indicating whether two <see cref="Object"/> instances are equal.
        /// </summary>
        /// <param name="objA">The first <see cref="Object"/> instance.</param>
        /// <param name="objB">The second <see cref="Object"/> instance.</param>
        /// <returns><see langword="true"/> if the references of <paramref name="objA"/> and <paramref name="objB"/> are equal, or one of them is destroyed while the other is <see langword="null"/>, or their identifiers are equal; otherwise <see langword="false"/>.</returns>
        /// <remarks>This method is the version of <see cref="Object.op_Equality"/> without the Unity main-thread check.</remarks>
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
