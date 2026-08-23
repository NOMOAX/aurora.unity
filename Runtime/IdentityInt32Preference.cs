using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 表示 <see cref="PlayerPrefs"/> 中的值为 <see cref="int"/> 的一项。
    /// </summary>
    public sealed class IdentityInt32Preference : Int32Preference<int>
    {
        /// <summary>
        /// 初始化 <see cref="IdentityInt32Preference"/> 类的新实例。
        /// </summary>
        /// <param name="key">键。</param>
        public IdentityInt32Preference(string key) : base(key, IdentityPreferenceConverterPair<int>.Instance)
        {
        }
    }
}
