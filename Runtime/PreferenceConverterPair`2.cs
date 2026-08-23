using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a pair of converter methods to convert between the user's value and the primitive value in <see cref="PlayerPrefs"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the user's value.</typeparam>
    /// <typeparam name="TPreferenceValue">The type of the primitive value in <see cref="PlayerPrefs"/>.</typeparam>
    public class PreferenceConverterPair<TValue, TPreferenceValue>
    {
        /// <summary>
        /// Represents a method used to convert the user's value to the primitive value in <see cref="PlayerPrefs"/>.
        /// </summary>
        public readonly Converter<TValue, TPreferenceValue> ToPreferenceValue;

        /// <summary>
        /// Represents a method used to convert the primitive value in <see cref="PlayerPrefs"/> to the user's value.
        /// </summary>
        public readonly Converter<TPreferenceValue, TValue> ToValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreferenceConverterPair{TValue,TPreferenceValue}"/> class.
        /// </summary>
        /// <param name="toPreferenceValue">Represents a method used to convert the user's value to the primitive value in <see cref="PlayerPrefs"/>.</param>
        /// <param name="toValue">Represents a method used to convert the primitive value in <see cref="PlayerPrefs"/> to the user's value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="toPreferenceValue"/> or <paramref name="toValue"/> is <see langword="null"/>.</exception>
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
