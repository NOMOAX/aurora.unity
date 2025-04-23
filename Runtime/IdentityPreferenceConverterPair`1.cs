using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 实现 <see cref="PreferenceConverterPair{TValue,TPreferenceValue}"/>，当用户使用的值和 <see cref="PlayerPrefs"/> 中基本值的类型相等时，无需做任何转换。
    /// </summary>
    /// <typeparam name="T">用户使用的值和 <see cref="PlayerPrefs"/> 中基本值的类型。</typeparam>
    public sealed class IdentityPreferenceConverterPair<T> : PreferenceConverterPair<T, T>
    {
        /// <summary>
        /// 获取单一实例。
        /// </summary>
        public static IdentityPreferenceConverterPair<T> Instance { get; } =
            new IdentityPreferenceConverterPair<T>(obj => obj);

        private IdentityPreferenceConverterPair(Converter<T, T> converter) : base(converter, converter)
        {
        }
    }
}
