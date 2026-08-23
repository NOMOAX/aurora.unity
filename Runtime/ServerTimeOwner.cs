using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a mechanism by which a client can set the server time at some moment and then obtain the instantaneous server time at any later time.
    /// </summary>
    public sealed class ServerTimeOwner
    {
        private DateTimeOffset? _serverTime;

        private double _realTimeSinceStartupWhenServerTimeIsSet;

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        public DateTimeOffset? CurrentTime
        {
            get
            {
                if (_serverTime.HasValue)
                {
                    return _serverTime.Value + TimeSpan.FromSeconds(
                               GetRealtimeSinceStartup() - _realTimeSinceStartupWhenServerTimeIsSet
                           );
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _serverTime                              = value.Value.ToUniversalTime();
                    _realTimeSinceStartupWhenServerTimeIsSet = GetRealtimeSinceStartup();
                }
                else
                {
                    _serverTime = null;
                }
            }
        }

        private static double GetRealtimeSinceStartup()
        {
            return Time.realtimeSinceStartupAsDouble;
        }
    }
}
