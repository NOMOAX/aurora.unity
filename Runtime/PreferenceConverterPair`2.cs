using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一对转换器方法，可将用户使用的值与 <see cref="PlayerPrefs"/> 中的基本值互相转换。
    /// </summary>
    /// <typeparam name="TValue">用户使用的值的类型。</typeparam>
    /// <typeparam name="TPreferenceValue"><see cref="PlayerPrefs"/> 中的基本值的类型。</typeparam>
    public class PreferenceConverterPair<TValue, TPreferenceValue>
    {
        /// <summary>
        /// 表示一个方法，用于将用户使用的值转换为 <see cref="PlayerPrefs"/> 中的基本值。
        /// </summary>
        public readonly Converter<TValue, TPreferenceValue> ToPreferenceValue;

        /// <summary>
        /// 表示一个方法，用于将 <see cref="PlayerPrefs"/> 中的基本值转换为用户使用的值。
        /// </summary>
        public readonly Converter<TPreferenceValue, TValue> ToValue;

        /// <summary>
        /// 初始化 <see cref="PreferenceConverterPair{TValue,TPreferenceValue}"/> 类的新实例。
        /// </summary>
        /// <param name="toPreferenceValue">表示一个方法，用于将用户使用的值转换为 <see cref="PlayerPrefs"/> 中的基本值。</param>
        /// <param name="toValue">表示一个方法，用于将 <see cref="PlayerPrefs"/> 中的基本值转换为用户使用的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="toPreferenceValue"/> 或 <paramref name="toValue"/> 为 <see langword="null"/>。</exception>
        public PreferenceConverterPair(
            Converter<TValue, TPreferenceValue> toPreferenceValue,
            Converter<TPreferenceValue, TValue> toValue)
        {
            if (toPreferenceValue == null)
            {
                throw new ArgumentNullException(nameof(toPreferenceValue));
            }
            if (toValue == null)
            {
                throw new ArgumentNullException(nameof(toValue));
            }
            ToPreferenceValue = toPreferenceValue;
            ToValue           = toValue;
        }
    }
}
