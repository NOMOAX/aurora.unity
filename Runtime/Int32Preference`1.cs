using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose primitive value is an <see cref="int"/>.
    /// </summary>
    /// <inheritdoc />
    public class Int32Preference<TValue> : Preference<TValue>
    {
        private readonly PreferenceConverterPair<TValue, int> _converterPair;

        /// <summary>
        /// Initializes a new instance of the <see cref="Int32Preference{TValue}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="converterPair">An object that provides a pair of converter methods to convert between the user's value and the primitive value in <see cref="PlayerPrefs"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="converterPair"/> is <see langword="null"/>.</exception>
        public Int32Preference(string key, PreferenceConverterPair<TValue, int> converterPair) : base(key)
        {
            if (converterPair == null)
            {
                throw new ArgumentNullException(nameof(converterPair));
            }
            _converterPair = converterPair;
        }

        /// <inheritdoc />
        public sealed override PreferenceValueType ValueType => PreferenceValueType.Int32;

        /// <inheritdoc />
        public override void SetValue(TValue value)
        {
            var intValue = _converterPair.ToPreferenceValue(value);
            PlayerPrefs.SetInt(Key, intValue);
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            var intValue = PlayerPrefs.GetInt(Key);
            return _converterPair.ToValue(intValue);
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            var defaultPreferenceValue = _converterPair.ToPreferenceValue(defaultValue);
            var intValue               = PlayerPrefs.GetInt(Key, defaultPreferenceValue);
            return _converterPair.ToValue(intValue);
        }
    }
}
