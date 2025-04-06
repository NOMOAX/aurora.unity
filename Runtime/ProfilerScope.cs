using System;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 分析一段代码。
    /// </summary>
    /// <remarks>不支持异步分析。</remarks>
    public struct ProfilerScope : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// 初始化 <see cref="ProfilerScope"/> 结构的新实例，并开始分析。
        /// </summary>
        /// <param name="name">用于识别分析的字符串。</param>
        public ProfilerScope(string name)
        {
            try
            {
                Profiler.BeginSample(name);
                _disposed = false;
            }
            catch (Exception)
            {
                _disposed = true;
                throw;
            }
        }

        /// <summary>
        /// 初始化 <see cref="ProfilerScope"/> 结构的新实例。
        /// </summary>
        /// <param name="name">用于识别分析的字符串。</param>
        /// <param name="targetObject">为分析提供上下文的对象。</param>
        public ProfilerScope(string name, Object targetObject)
        {
            try
            {
                Profiler.BeginSample(name, targetObject);
                _disposed = false;
            }
            catch (Exception)
            {
                _disposed = true;
                throw;
            }
        }

        /// <summary>
        /// 结束分析。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Profiler.EndSample();
            _disposed = true;
        }
    }
}
