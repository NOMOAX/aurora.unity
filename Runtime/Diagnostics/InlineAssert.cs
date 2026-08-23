using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Diagnostics
{
    /// <summary>
    /// 断言并返回用户传入的原始值。
    /// <br/>
    /// 断言失败时抛出异常。
    /// </summary>
    /// <seealso cref="Assert"/>
    /// <seealso cref="InlineAssertNoThrow"/>
    public static class InlineAssert
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(nameof(InlineAssert) + "." + nameof(Equals) + " should not be used for Assertions", true)]
        public new static bool Equals(object obj1, object obj2)
        {
            throw new InvalidOperationException(
                nameof(InlineAssert) + "." + nameof(Equals) + " should not be used for Assertions"
            );
        }

        [Obsolete(nameof(InlineAssert) + "." + nameof(ReferenceEquals) + " should not be used for Assertions", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static bool ReferenceEquals(object obj1, object obj2)
        {
            throw new InvalidOperationException(
                nameof(InlineAssert) + "." + nameof(ReferenceEquals) + " should not be used for Assertions"
            );
        }

        /// <seealso cref="Assert.IsTrue(bool)"/>
        public static bool IsTrue(bool condition)
        {
            Assert.IsTrue(condition);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return condition;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        /// <seealso cref="Assert.IsTrue(bool,string)"/>
        public static bool IsTrue(bool condition, string message)
        {
            Assert.IsTrue(condition, message);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return condition;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        /// <seealso cref="Assert.IsFalse(bool)"/>
        public static bool IsFalse(bool condition)
        {
            Assert.IsFalse(condition);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return condition;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        /// <seealso cref="Assert.IsFalse(bool,string)"/>
        public static bool IsFalse(bool condition, string message)
        {
            Assert.IsFalse(condition, message);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            return condition;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float)"/>
        public static float AreApproximatelyEqual(float expected, float actual)
        {
            Assert.AreApproximatelyEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,string)"/>
        public static float AreApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreApproximatelyEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,float)"/>
        public static float AreApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreApproximatelyEqual(expected, actual, tolerance);
            return actual;
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,float,string)"/>
        public static float AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreApproximatelyEqual(expected, actual, tolerance, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual)
        {
            Assert.AreNotApproximatelyEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,string)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreNotApproximatelyEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,float)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreNotApproximatelyEqual(expected, actual, tolerance);
            return actual;
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,float,string)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreNotApproximatelyEqual(expected, actual, tolerance, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T)"/>
        public static T AreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T,string)"/>
        public static T AreEqual<T>(T expected, T actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T,string,IEqualityComparer{T})"/>
        public static T AreEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            Assert.AreEqual(expected, actual, message, comparer);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(Object,Object,string)"/>
        public static Object AreEqual(Object expected, Object actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T)"/>
        public static T AreNotEqual<T>(T expected, T actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T,string)"/>
        public static T AreNotEqual<T>(T expected, T actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T,string,IEqualityComparer{T})"/>
        public static T AreNotEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            Assert.AreNotEqual(expected, actual, message, comparer);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(Object,Object,string)"/>
        public static Object AreNotEqual(Object expected, Object actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.IsNull{T}(T)"/>
        public static T IsNull<T>(T value) where T : class
        {
            Assert.IsNull(value);
            return value;
        }

        /// <seealso cref="Assert.IsNull{T}(T,string)"/>
        public static T IsNull<T>(T value, string message) where T : class
        {
            Assert.IsNull(value, message);
            return value;
        }

        /// <seealso cref="Assert.IsNull(Object,string)"/>
        public static Object IsNull(Object value, string message)
        {
            Assert.IsNull(value, message);
            return value;
        }

        /// <seealso cref="Assert.IsNotNull{T}(T)"/>
        public static T IsNotNull<T>(T value) where T : class
        {
            Assert.IsNotNull(value);
            return value;
        }

        /// <seealso cref="Assert.IsNotNull{T}(T,string)"/>
        public static T IsNotNull<T>(T value, string message) where T : class
        {
            Assert.IsNotNull(value, message);
            return value;
        }

        /// <seealso cref="Assert.IsNotNull(Object,string)"/>
        public static Object IsNotNull(Object value, string message)
        {
            Assert.IsNotNull(value, message);
            return value;
        }

        /// <seealso cref="Assert.AreEqual(sbyte,sbyte)"/>
        public static sbyte AreEqual(sbyte expected, sbyte actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(sbyte,sbyte,string)"/>
        public static sbyte AreEqual(sbyte expected, sbyte actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(sbyte,sbyte)"/>
        public static sbyte AreNotEqual(sbyte expected, sbyte actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(sbyte,sbyte,string)"/>
        public static sbyte AreNotEqual(sbyte expected, sbyte actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(byte,byte)"/>
        public static byte AreEqual(byte expected, byte actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(byte,byte,string)"/>
        public static byte AreEqual(byte expected, byte actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(byte,byte)"/>
        public static byte AreNotEqual(byte expected, byte actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(byte,byte,string)"/>
        public static byte AreNotEqual(byte expected, byte actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(char,char)"/>
        public static char AreEqual(char expected, char actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(char,char,string)"/>
        public static char AreEqual(char expected, char actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(char,char)"/>
        public static char AreNotEqual(char expected, char actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(char,char,string)"/>
        public static char AreNotEqual(char expected, char actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(short,short)"/>
        public static short AreEqual(short expected, short actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(short,short,string)"/>
        public static short AreEqual(short expected, short actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(short,short)"/>
        public static short AreNotEqual(short expected, short actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(short,short,string)"/>
        public static short AreNotEqual(short expected, short actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(ushort,ushort)"/>
        public static ushort AreEqual(ushort expected, ushort actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(ushort,ushort,string)"/>
        public static ushort AreEqual(ushort expected, ushort actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(ushort,ushort)"/>
        public static ushort AreNotEqual(ushort expected, ushort actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(ushort,ushort,string)"/>
        public static ushort AreNotEqual(ushort expected, ushort actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(int,int)"/>
        public static int AreEqual(int expected, int actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(int,int,string)"/>
        public static int AreEqual(int expected, int actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(int,int)"/>
        public static int AreNotEqual(int expected, int actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(int,int,string)"/>
        public static int AreNotEqual(int expected, int actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(uint,uint)"/>
        public static uint AreEqual(uint expected, uint actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(uint,uint,string)"/>
        public static uint AreEqual(uint expected, uint actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(uint,uint)"/>
        public static uint AreNotEqual(uint expected, uint actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(uint,uint,string)"/>
        public static uint AreNotEqual(uint expected, uint actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(long,long)"/>
        public static long AreEqual(long expected, long actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(long,long,string)"/>
        public static long AreEqual(long expected, long actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(long,long)"/>
        public static long AreNotEqual(long expected, long actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(long,long,string)"/>
        public static long AreNotEqual(long expected, long actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(ulong,ulong)"/>
        public static ulong AreEqual(ulong expected, ulong actual)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(ulong,ulong,string)"/>
        public static ulong AreEqual(ulong expected, ulong actual, string message)
        {
            Assert.AreEqual(expected, actual, message);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(ulong,ulong)"/>
        public static ulong AreNotEqual(ulong expected, ulong actual)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(ulong,ulong,string)"/>
        public static ulong AreNotEqual(ulong expected, ulong actual, string message)
        {
            Assert.AreNotEqual(expected, actual, message);
            return actual;
        }
    }
}
