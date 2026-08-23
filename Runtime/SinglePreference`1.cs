using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 表示 <see cref="PlayerPrefs"/> 中的基本值为 <see cref="float"/> 的一项。
    /// </summary>
    /// <inheritdoc />
    public class SinglePreference<TValue> : Preference<TValue>
    {
        private readonly PreferenceConverterPair<TValue, float> _converterPair;

        /// <summary>
        /// 初始化 <see cref="SinglePreference{TValue}"/> 类的新实例。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="converterPair">一个对象，它提供一对转换器方法，可将用户使用的值与 <see cref="PlayerPrefs"/> 中的基本值互相转换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="converterPair"/> 为 <see langword="null"/>。</exception>
        public SinglePreference(string key, PreferenceConverterPair<TValue, float> converterPair) : base(key)
        {
            if (converterPair == null)
            {
                throw new ArgumentNullException(nameof(converterPair));
            }
            _converterPair = converterPair;
        }

        /// <inheritdoc />
        public sealed override PreferenceValueType ValueType => PreferenceValueType.Single;

        /// <inheritdoc />
        public override void SetValue(TValue value)
        {
            var floatValue = _converterPair.ToPreferenceValue(value);
            PlayerPrefs.SetFloat(Key, floatValue);
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            var floatValue = PlayerPrefs.GetFloat(Key);
            return _converterPair.ToValue(floatValue);
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            var defaultPreferenceValue = _converterPair.ToPreferenceValue(defaultValue);
            var floatValue             = PlayerPrefs.GetFloat(Key, defaultPreferenceValue);
            return _converterPair.ToValue(floatValue);
        }
    }
}
