using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// When <see cref="GetValue()"/> throws, the value stored under <see cref="Preference{TValue}.Key"/> in <see cref="PlayerPrefs"/> is set to <see cref="DefaultValuePreference{TValue}.DefaultValue"/> and then returned; when <see cref="GetValue(TValue)"/> throws, the value stored under <see cref="Preference{TValue}.Key"/> in <see cref="PlayerPrefs"/> is set to the passed-in default value and then returned.
    /// </summary>
    /// <inheritdoc />
    /// <remarks>It is recommended to use this type to handle the case where a value in <see cref="PlayerPrefs"/> is accidentally corrupted.</remarks>
    public sealed class OverridePreference<TValue> : DefaultValuePreference<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverridePreference{TValue}"/> class.
        /// </summary>
        /// <inheritdoc />
        public OverridePreference(Preference<TValue> preference, TValue defaultValue) : base(preference, defaultValue)
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
