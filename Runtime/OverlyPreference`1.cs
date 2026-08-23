using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// When <see cref="GetValue()"/> is called and the value does not exist, the value stored under <see cref="Preference{TValue}.Key"/> in <see cref="PlayerPrefs"/> is set to <see cref="DefaultValuePreference{TValue}.DefaultValue"/> and then returned; when <see cref="GetValue(TValue)"/> is called and the value does not exist, the value stored under <see cref="Preference{TValue}.Key"/> in <see cref="PlayerPrefs"/> is set to the passed-in default value and then returned.
    /// </summary>
    /// <inheritdoc />
    public sealed class OverlyPreference<TValue> : DefaultValuePreference<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverlyPreference{TValue}"/> class.
        /// </summary>
        /// <inheritdoc />
        public OverlyPreference(Preference<TValue> preference, TValue defaultValue) : base(preference, defaultValue)
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
