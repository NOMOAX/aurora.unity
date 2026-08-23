using System;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Profiles a section of code.
    /// </summary>
    /// <remarks>Asynchronous profiling is not supported.</remarks>
    public struct ProfilerScope : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilerScope"/> struct and begins profiling.
        /// </summary>
        /// <param name="name">A string used to identify the profiling section.</param>
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
        /// Initializes a new instance of the <see cref="ProfilerScope"/> struct.
        /// </summary>
        /// <param name="name">A string used to identify the profiling section.</param>
        /// <param name="targetObject">An object that provides context for the profiling section.</param>
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
        /// Ends profiling.
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
