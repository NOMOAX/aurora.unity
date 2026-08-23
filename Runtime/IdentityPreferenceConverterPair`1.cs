using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Implements <see cref="PreferenceConverterPair{TValue,TPreferenceValue}"/> for the case where the type of the user's value equals the type of the primitive value in <see cref="PlayerPrefs"/>, so no conversion is needed.
    /// </summary>
    /// <typeparam name="T">The type of both the user's value and the primitive value in <see cref="PlayerPrefs"/>.</typeparam>
    public sealed class IdentityPreferenceConverterPair<T> : PreferenceConverterPair<T, T>
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static IdentityPreferenceConverterPair<T> Instance { get; } = new(obj => obj);

        private IdentityPreferenceConverterPair(Converter<T, T> converter) : base(converter, converter)
        {
        }
    }
}
