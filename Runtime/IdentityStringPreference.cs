using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 表示 <see cref="PlayerPrefs"/> 中的值为 <see cref="string"/> 的一项。
    /// </summary>
    public sealed class IdentityStringPreference : StringPreference<string>
    {
        /// <summary>
        /// 初始化 <see cref="IdentityStringPreference"/> 类的新实例。
        /// </summary>
        /// <param name="key">键。</param>
        public IdentityStringPreference(string key) : base(key, IdentityPreferenceConverterPair<string>.Instance)
        {
        }
    }
}
