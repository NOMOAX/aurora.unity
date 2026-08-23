using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Aurora.Pooling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using Object = UnityEngine.Object;

namespace Aurora.Unity.Diagnostics
{
    /// <summary>
    /// Asserts a condition and returns the original value passed in.
    /// <br/>
    /// Does not throw an exception when the assertion fails; it only prints the failure message.
    /// </summary>
    /// <seealso cref="Assert"/>
    /// <seealso cref="InlineAssert"/>
    public static class InlineAssertNoThrow
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete(nameof(InlineAssertNoThrow) + "." + nameof(Equals) + " should not be used for Assertions", true)]
        public new static bool Equals(object obj1, object obj2)
        {
            throw new InvalidOperationException(
                nameof(InlineAssertNoThrow) + "." + nameof(Equals) + " should not be used for Assertions"
            );
        }

        [Obsolete(
            nameof(InlineAssertNoThrow) + "." + nameof(ReferenceEquals) + " should not be used for Assertions",
            true
        )]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static bool ReferenceEquals(object obj1, object obj2)
        {
            throw new InvalidOperationException(
                nameof(InlineAssertNoThrow) + "." + nameof(ReferenceEquals) + " should not be used for Assertions"
            );
        }

        private static void Fail(string message, string userMessage)
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                if (userMessage != null)
                {
                    stringBuilder.Append(userMessage);
                    stringBuilder.Append('\n');
                }
                stringBuilder.Append(message ?? "Assertion has failed\n");
                Debug.LogAssertion(stringBuilder.ToString());
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }

        /// <seealso cref="Assert.IsTrue(bool)"/>
        public static bool IsTrue(bool condition)
        {
            return IsTrue(condition, null);
        }

        /// <seealso cref="Assert.IsTrue(bool,string)"/>
        public static bool IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                Fail(AssertionMessageUtil.BooleanFailureMessage(true), message);
            }
            return condition;
        }

        /// <seealso cref="Assert.IsFalse(bool)"/>
        public static bool IsFalse(bool condition)
        {
            return IsFalse(condition, null);
        }

        /// <seealso cref="Assert.IsFalse(bool,string)"/>
        public static bool IsFalse(bool condition, string message)
        {
            if (condition)
            {
                Fail(AssertionMessageUtil.BooleanFailureMessage(false), message);
            }
            return condition;
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float)"/>
        public static float AreApproximatelyEqual(float expected, float actual)
        {
            return AreApproximatelyEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,string)"/>
        public static float AreApproximatelyEqual(float expected, float actual, string message)
        {
            return AreEqual(expected, actual, message, FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,float)"/>
        public static float AreApproximatelyEqual(float expected, float actual, float tolerance)
        {
            return AreApproximatelyEqual(expected, actual, tolerance, null);
        }

        /// <seealso cref="Assert.AreApproximatelyEqual(float,float,float,string)"/>
        public static float AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            return AreEqual(expected, actual, message, new FloatComparer(tolerance));
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual)
        {
            return AreNotApproximatelyEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,string)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, string message)
        {
            return AreNotEqual(expected, actual, message, FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,float)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, float tolerance)
        {
            return AreNotApproximatelyEqual(expected, actual, tolerance, null);
        }

        /// <seealso cref="Assert.AreNotApproximatelyEqual(float,float,float,string)"/>
        public static float AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            return AreNotEqual(expected, actual, message, new FloatComparer(tolerance));
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T)"/>
        public static T AreEqual<T>(T expected, T actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T,string)"/>
        public static T AreEqual<T>(T expected, T actual, string message)
        {
            return AreEqual(expected, actual, message, EqualityComparer<T>.Default);
        }

        /// <seealso cref="Assert.AreEqual{T}(T,T,string,IEqualityComparer{T})"/>
        public static T AreEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)AreEqual((Object)(object)expected, (Object)(object)actual, message);
            }
            if (!comparer.Equals(actual, expected))
            {
                Fail(AssertionMessageUtil.GetEqualityMessage(actual, expected, true), message);
            }
            return actual;
        }

        /// <seealso cref="Assert.AreEqual(Object,Object,string)"/>
        public static Object AreEqual(Object expected, Object actual, string message)
        {
            if (expected != actual)
            {
                Fail(AssertionMessageUtil.GetEqualityMessage(actual, expected, true), message);
            }
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T)"/>
        public static T AreNotEqual<T>(T expected, T actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T,string)"/>
        public static T AreNotEqual<T>(T expected, T actual, string message)
        {
            return AreNotEqual(expected, actual, message, EqualityComparer<T>.Default);
        }

        /// <seealso cref="Assert.AreNotEqual{T}(T,T,string,IEqualityComparer{T})"/>
        public static T AreNotEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)AreNotEqual((Object)(object)expected, (Object)(object)actual, message);
            }
            if (comparer.Equals(actual, expected))
            {
                Fail(AssertionMessageUtil.GetEqualityMessage(actual, expected, false), message);
            }
            return actual;
        }

        /// <seealso cref="Assert.AreNotEqual(Object,Object,string)"/>
        public static Object AreNotEqual(Object expected, Object actual, string message)
        {
            if (expected == actual)
            {
                Fail(AssertionMessageUtil.GetEqualityMessage(actual, expected, false), message);
            }
            return actual;
        }

        /// <seealso cref="Assert.IsNull{T}(T)"/>
        public static T IsNull<T>(T value) where T : class
        {
            return IsNull(value, null);
        }

        /// <seealso cref="Assert.IsNull{T}(T,string)"/>
        public static T IsNull<T>(T value, string message) where T : class
        {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)IsNull((Object)(object)value, message);
            }
            if (value != null)
            {
                Fail(AssertionMessageUtil.NullFailureMessage(true), message);
            }
            return value;
        }

        /// <seealso cref="Assert.IsNull(Object,string)"/>
        public static Object IsNull(Object value, string message)
        {
            if (value != null)
            {
                Fail(AssertionMessageUtil.NullFailureMessage(true), message);
            }
            return value;
        }

        /// <seealso cref="Assert.IsNotNull{T}(T)"/>
        public static T IsNotNull<T>(T value) where T : class
        {
            return IsNotNull(value, null);
        }

        /// <seealso cref="Assert.IsNotNull{T}(T,string)"/>
        public static T IsNotNull<T>(T value, string message) where T : class
        {
            if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)IsNotNull((Object)(object)value, message);
            }
            if (value == null)
            {
                Fail(AssertionMessageUtil.NullFailureMessage(false), message);
            }
            return value;
        }

        /// <seealso cref="Assert.IsNotNull(Object,string)"/>
        public static Object IsNotNull(Object value, string message)
        {
            if (value == null)
            {
                Fail(AssertionMessageUtil.NullFailureMessage(false), message);
            }
            return value;
        }

        /// <seealso cref="Assert.AreEqual(sbyte,sbyte)"/>
        public static sbyte AreEqual(sbyte expected, sbyte actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(sbyte,sbyte,string)"/>
        public static sbyte AreEqual(sbyte expected, sbyte actual, string message)
        {
            return AreEqual<sbyte>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(sbyte,sbyte)"/>
        public static sbyte AreNotEqual(sbyte expected, sbyte actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(sbyte,sbyte,string)"/>
        public static sbyte AreNotEqual(sbyte expected, sbyte actual, string message)
        {
            return AreNotEqual<sbyte>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(byte,byte)"/>
        public static byte AreEqual(byte expected, byte actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(byte,byte,string)"/>
        public static byte AreEqual(byte expected, byte actual, string message)
        {
            return AreEqual<byte>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(byte,byte)"/>
        public static byte AreNotEqual(byte expected, byte actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(byte,byte,string)"/>
        public static byte AreNotEqual(byte expected, byte actual, string message)
        {
            return AreNotEqual<byte>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(char,char)"/>
        public static char AreEqual(char expected, char actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(char,char,string)"/>
        public static char AreEqual(char expected, char actual, string message)
        {
            return AreEqual<char>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(char,char)"/>
        public static char AreNotEqual(char expected, char actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(char,char,string)"/>
        public static char AreNotEqual(char expected, char actual, string message)
        {
            return AreNotEqual<char>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(short,short)"/>
        public static short AreEqual(short expected, short actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(short,short,string)"/>
        public static short AreEqual(short expected, short actual, string message)
        {
            return AreEqual<short>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(short,short)"/>
        public static short AreNotEqual(short expected, short actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(short,short,string)"/>
        public static short AreNotEqual(short expected, short actual, string message)
        {
            return AreNotEqual<short>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(ushort,ushort)"/>
        public static ushort AreEqual(ushort expected, ushort actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(ushort,ushort,string)"/>
        public static ushort AreEqual(ushort expected, ushort actual, string message)
        {
            return AreEqual<ushort>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(ushort,ushort)"/>
        public static ushort AreNotEqual(ushort expected, ushort actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(ushort,ushort,string)"/>
        public static ushort AreNotEqual(ushort expected, ushort actual, string message)
        {
            return AreNotEqual<ushort>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(int,int)"/>
        public static int AreEqual(int expected, int actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(int,int,string)"/>
        public static int AreEqual(int expected, int actual, string message)
        {
            return AreEqual<int>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(int,int)"/>
        public static int AreNotEqual(int expected, int actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(int,int,string)"/>
        public static int AreNotEqual(int expected, int actual, string message)
        {
            return AreNotEqual<int>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(uint,uint)"/>
        public static uint AreEqual(uint expected, uint actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(uint,uint,string)"/>
        public static uint AreEqual(uint expected, uint actual, string message)
        {
            return AreEqual<uint>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(uint,uint)"/>
        public static uint AreNotEqual(uint expected, uint actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(uint,uint,string)"/>
        public static uint AreNotEqual(uint expected, uint actual, string message)
        {
            return AreNotEqual<uint>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(long,long)"/>
        public static long AreEqual(long expected, long actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(long,long,string)"/>
        public static long AreEqual(long expected, long actual, string message)
        {
            return AreEqual<long>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(long,long)"/>
        public static long AreNotEqual(long expected, long actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(long,long,string)"/>
        public static long AreNotEqual(long expected, long actual, string message)
        {
            return AreNotEqual<long>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreEqual(ulong,ulong)"/>
        public static ulong AreEqual(ulong expected, ulong actual)
        {
            return AreEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreEqual(ulong,ulong,string)"/>
        public static ulong AreEqual(ulong expected, ulong actual, string message)
        {
            return AreEqual<ulong>(expected, actual, message);
        }

        /// <seealso cref="Assert.AreNotEqual(ulong,ulong)"/>
        public static ulong AreNotEqual(ulong expected, ulong actual)
        {
            return AreNotEqual(expected, actual, null);
        }

        /// <seealso cref="Assert.AreNotEqual(ulong,ulong,string)"/>
        public static ulong AreNotEqual(ulong expected, ulong actual, string message)
        {
            return AreNotEqual<ulong>(expected, actual, message);
        }

        private static class AssertionMessageUtil
        {
            private static string GetMessage(string failureMessage)
            {
                return string.Format(NumberFormatInfo.InvariantInfo, "{0} {1}", "Assertion failure.", failureMessage);
            }

            private static string GetMessage(string failureMessage, string expected)
            {
                return GetMessage(
                    string.Format(
                        NumberFormatInfo.InvariantInfo,
                        "{0}{1}{2} {3}",
                        failureMessage,
                        System.Environment.NewLine,
                        "Expected:",
                        expected
                    )
                );
            }

            internal static string GetEqualityMessage(object actual, object expected, bool expectEqual)
            {
                return GetMessage(
                    string.Format(NumberFormatInfo.InvariantInfo, "Values are {0}equal.", expectEqual ? "not " : ""),
                    string.Format(
                        NumberFormatInfo.InvariantInfo,
                        "{0} {2} {1}",
                        actual,
                        expected,
                        expectEqual ? "==" : "!="
                    )
                );
            }

            internal static string NullFailureMessage(bool expectNull)
            {
                return GetMessage(
                    string.Format(NumberFormatInfo.InvariantInfo, "Value was {0}Null", expectNull ? "not " : ""),
                    string.Format(NumberFormatInfo.InvariantInfo, "Value was {0}Null", expectNull ? "" : "not ")
                );
            }

            internal static string BooleanFailureMessage(bool expected)
            {
                return GetMessage("Value was " + !expected, expected.ToString());
            }
        }
    }
}
