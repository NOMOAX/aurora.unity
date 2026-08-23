using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose primitive value is a <see cref="float"/>.
    /// </summary>
    /// <inheritdoc />
    public class SinglePreference<TValue> : Preference<TValue>
    {
        private readonly PreferenceConverterPair<TValue, float> _converterPair;

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePreference{TValue}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="converterPair">An object that provides a pair of converter methods to convert between the user's value and the primitive value in <see cref="PlayerPrefs"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="converterPair"/> is <see langword="null"/>.</exception>
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
