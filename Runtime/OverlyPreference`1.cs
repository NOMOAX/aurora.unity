using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 当调用 <see cref="GetValue()"/> 时如果值不存在，会设置 <see cref="PlayerPrefs"/> 中 <see cref="Preference{TValue}.Key"/> 对应的值为 <see cref="DefaultValuePreference{TValue}.DefaultValue"/>，然后返回；当调用 <see cref="GetValue(TValue)"/> 时如果值不存在，会设置 <see cref="PlayerPrefs"/> 中 <see cref="Preference{TValue}.Key"/> 对应的值为传入的默认值，然后返回。
    /// </summary>
    /// <inheritdoc />
    public sealed class OverlyPreference<TValue> : DefaultValuePreference<TValue>
    {
        /// <summary>
        /// 初始化 <see cref="OverlyPreference{TValue}"/> 类的新实例。
        /// </summary>
        /// <inheritdoc />
        public OverlyPreference(Preference<TValue> preference, TValue defaultValue = default) : base(
            preference,
            defaultValue
        )
        {
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            if (KeyExists)
            {
                return Preference.GetValue();
            }
            var defaultValue = DefaultValue;
            SetValue(defaultValue);
            return defaultValue;
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            if (KeyExists)
            {
                return Preference.GetValue();
            }
            SetValue(defaultValue);
            return defaultValue;
        }
    }
}
