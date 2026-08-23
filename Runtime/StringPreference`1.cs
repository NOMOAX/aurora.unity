using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose primitive value is a <see cref="string"/>.
    /// </summary>
    /// <inheritdoc />
    public class StringPreference<TValue> : Preference<TValue>
    {
        private readonly PreferenceConverterPair<TValue, string> _converterPair;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPreference{TValue}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="converterPair">An object that provides a pair of converter methods to convert between the user's value and the primitive value in <see cref="PlayerPrefs"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="converterPair"/> is <see langword="null"/>.</exception>
        public StringPreference(string key, PreferenceConverterPair<TValue, string> converterPair) : base(key)
        {
            if (converterPair == null)
            {
                throw new ArgumentNullException(nameof(converterPair));
            }
            _converterPair = converterPair;
        }

        /// <inheritdoc />
        public sealed override PreferenceValueType ValueType => PreferenceValueType.String;

        /// <inheritdoc />
        public override void SetValue(TValue value)
        {
            var stringValue = _converterPair.ToPreferenceValue(value);
            PlayerPrefs.SetString(Key, stringValue);
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            var stringValue = PlayerPrefs.GetString(Key);
            return _converterPair.ToValue(stringValue);
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            var defaultPreferenceValue = _converterPair.ToPreferenceValue(defaultValue);
            var stringValue            = PlayerPrefs.GetString(Key, defaultPreferenceValue);
            return _converterPair.ToValue(stringValue);
        }
    }
}
