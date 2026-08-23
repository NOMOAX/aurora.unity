using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Aurora.Diagnostics;
using Aurora.Pooling;
using UnityEngine;
using ILogger = Aurora.Diagnostics.ILogger;

namespace Aurora.Unity
{
    /// <summary>
    /// 写入到 Unity 控制台。
    /// </summary>
    public sealed class UnityConsoleLogger : ILogger
    {
        /// <summary>
        /// 获取单一实例。
        /// </summary>
        public static UnityConsoleLogger Instance { get; } = new();

        private UnityConsoleLogger()
        {
        }

        void ILogger.Log(object value, LogLevel logLevel)
        {
            if (!EnumUtility<LogLevel>.IsDefined(logLevel))
            {
                return;
            }
            if (logLevel < Log.Level)
            {
                return;
            }
            if (GetLogAction(logLevel) is var logAction && logAction is null)
            {
                return;
            }
            var logString = GetLogString(value, logLevel);
            logAction(logString);
        }

        /// <summary>
        /// 根据写入等级，获取执行写入的方法。
        /// </summary>
        /// <param name="logLevel">写入等级。</param>
        /// <returns>如果需要执行写入，则为执行写入的方法；否则为 <see langword="null"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Action<object> GetLogAction(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.V => Debug.Log,
                LogLevel.D => Debug.Log,
                LogLevel.I => Debug.Log,
                LogLevel.W => Debug.LogWarning,
                LogLevel.E => Debug.LogError,
                _          => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        /// <summary>
        /// 获取实际写入的字符串。
        /// </summary>
        /// <param name="value">写入对象。</param>
        /// <param name="logLevel">写入等级。</param>
        /// <returns>实际写入的字符串。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetLogString(object value, LogLevel logLevel)
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
#if UNITY_EDITOR
                using (new BoldScope(stringBuilder))
#endif
                {
                    stringBuilder.Append(GetType().Name);
                    stringBuilder.Append(' ');
                    stringBuilder.Append('(');
                    {
#if UNITY_EDITOR
                        using (new ColorScope(stringBuilder, GetHtmlColor(logLevel)))
#endif
                        {
                            stringBuilder.Append(logLevel.ToString());
                        }
                        LogUtility.AppendCurrentThreadIdString(stringBuilder);
                        LogUtility.AppendDateTimeOffsetString(stringBuilder);
                    }
                    stringBuilder.Append(')');
                }
                stringBuilder.Append(' ');
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetHtmlColor(LogLevel logLevel)
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.Append('#');
                stringBuilder.Append(UnityEngine.ColorUtility.ToHtmlStringRGBA(GetColor(logLevel)));
                return stringBuilder.ToString();
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }
#endif

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color32 GetColor(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.V => UnityEnvironment.IsProSkin switch
                {
                    true  => new Color32(112, 112, 112, 255),
                    false => new Color32(124, 124, 124, 255)
                },
                LogLevel.D => UnityEnvironment.IsProSkin switch
                {
                    true  => new Color32(66, 171, 234, 255),
                    false => new Color32(21, 127, 191, 255)
                },
                LogLevel.I => UnityEnvironment.IsProSkin switch
                {
                    true  => new Color32(240, 240, 240, 255),
                    false => new Color32(255, 255, 255, 255)
                },
                LogLevel.W => UnityEnvironment.IsProSkin switch
                {
                    true  => new Color32(255, 193, 7, 255),
                    false => new Color32(201, 151, 0, 255)
                },
                LogLevel.E => UnityEnvironment.IsProSkin switch
                {
                    true  => new Color32(255, 83, 74, 255),
                    false => new Color32(177, 12, 12, 255)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }
#endif

#if UNITY_EDITOR
        private struct BoldScope : IDisposable
        {
            private StringBuilder _stringBuilder;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal BoldScope(StringBuilder stringBuilder)
            {
                _stringBuilder = stringBuilder;
                _stringBuilder.Append("<b>");
            }

            void IDisposable.Dispose()
            {
                var stringBuilder = _stringBuilder;
                if (stringBuilder is null || Interlocked.CompareExchange(ref _stringBuilder, null, stringBuilder) !=
                    stringBuilder)
                {
                    return;
                }
                stringBuilder.Append("</b>");
            }
        }
#endif

#if UNITY_EDITOR
        private struct ColorScope : IDisposable
        {
            private StringBuilder _stringBuilder;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ColorScope(StringBuilder stringBuilder, string htmlColor)
            {
                _stringBuilder = stringBuilder;
                _stringBuilder.AppendFormat("<color={0}>", htmlColor);
            }

            void IDisposable.Dispose()
            {
                var stringBuilder = _stringBuilder;
                if (stringBuilder is null || Interlocked.CompareExchange(ref _stringBuilder, null, stringBuilder) !=
                    stringBuilder)
                {
                    return;
                }
                stringBuilder.Append("</color>");
            }
        }
#endif
    }
}
