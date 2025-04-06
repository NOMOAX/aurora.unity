using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一种机制，客户端可以在某一时刻设置服务器时间，然后就可以在之后的任何时间获取即时的服务器时间。
    /// </summary>
    public sealed class ServerTimeOwner
    {
        private DateTimeOffset? _serverTime;

        private double _realTimeSinceStartupWhenServerTimeIsSet;

        /// <summary>
        /// 获取或设置当前时间。
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
