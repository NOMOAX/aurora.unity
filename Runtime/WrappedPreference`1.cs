using System;

namespace Aurora.Unity
{
    /// <summary>
    /// Wraps another <see cref="Preference{TValue}"/> to perform additional operations.
    /// </summary>
    /// <inheritdoc />
    public class WrappedPreference<TValue> : Preference<TValue>
    {
        public readonly Preference<TValue> Preference;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedPreference{TValue}"/> class.
        /// </summary>
        /// <param name="preference">The wrapped <see cref="Preference{TValue}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="preference"/> is <see langword="null"/>.</exception>
        public WrappedPreference(Preference<TValue> preference) : base(preference?.Key ?? string.Empty)
        {
            if (preference == null)
            {
                throw new ArgumentNullException();
            }
            Preference = preference;
        }

        /// <inheritdoc />
        public sealed override PreferenceValueType ValueType => Preference.ValueType;

        /// <inheritdoc />
        public override void SetValue(TValue value)
        {
            Preference.SetValue(value);
        }

        /// <inheritdoc />
        public override TValue GetValue()
        {
            return Preference.GetValue();
        }

        /// <inheritdoc />
        public override TValue GetValue(TValue defaultValue)
        {
            return Preference.GetValue(defaultValue);
        }
    }
}
