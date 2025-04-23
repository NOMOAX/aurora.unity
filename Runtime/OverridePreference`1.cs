using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 当 <see cref="GetValue()"/> 引发异常时，会设置 <see cref="PlayerPrefs"/> 中 <see cref="Preference{TValue}.Key"/> 对应的值为 <see cref="DefaultValuePreference{TValue}.DefaultValue"/>，然后返回；当 <see cref="GetValue(TValue)"/> 引发异常时，会设置 <see cref="PlayerPrefs"/> 中 <see cref="Preference{TValue}.Key"/> 对应的值为传入的默认值，然后返回。
    /// </summary>
    /// <inheritdoc />
    /// <remarks>建议使用此类型，能解决 <see cref="PlayerPrefs"/> 中的值意外损坏的情形。</remarks>
    public sealed class OverridePreference<TValue> : DefaultValuePreference<TValue>
    {
        /// <summary>
        /// 初始化 <see cref="OverridePreference{TValue}"/> 类的新实例。
        /// </summary>
        /// <inheritdoc />
        public OverridePreference(Preference<TValue> preference, TValue defaultValue = default) : base(
            preference,
            defaultValue
        )
        {
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            try
            {
                return Preference.GetValue();
            }
            catch (Exception)
            {
                var defaultValue = DefaultValue;
                SetValue(defaultValue);
                return defaultValue;
            }
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            try
            {
                return Preference.GetValue(defaultValue);
            }
            catch (Exception)
            {
                SetValue(defaultValue);
                return defaultValue;
            }
        }
    }
}
