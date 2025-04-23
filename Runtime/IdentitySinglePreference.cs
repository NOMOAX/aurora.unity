using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 表示 <see cref="PlayerPrefs"/> 中的值为 <see cref="float"/> 的一项。
    /// </summary>
    public sealed class IdentitySinglePreference : SinglePreference<float>
    {
        /// <summary>
        /// 初始化 <see cref="IdentitySinglePreference"/> 类的新实例。
        /// </summary>
        /// <param name="key">键。</param>
        public IdentitySinglePreference(string key) : base(key, IdentityPreferenceConverterPair<float>.Instance)
        {
        }
    }
}
