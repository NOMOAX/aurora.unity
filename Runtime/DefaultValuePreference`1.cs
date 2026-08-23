using System;
using System.Runtime.CompilerServices;

namespace Aurora.Unity
{
    /// <summary>
    /// Wraps another <see cref="Preference{TValue}"/> and holds a default value in order to perform additional operations.
    /// </summary>
    /// <inheritdoc />
    public abstract class DefaultValuePreference<TValue> : WrappedPreference<TValue>
    {
        private readonly TValue _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValuePreference{TValue}"/> class.
        /// </summary>
        /// <param name="preference">The wrapped <see cref="Preference{TValue}"/>.</param>
        /// <param name="defaultValue">The specified default value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="preference"/> is <see langword="null"/>.</exception>
        protected DefaultValuePreference(Preference<TValue> preference, TValue defaultValue) : base(preference)
        {
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// The default value passed in from the constructor.
        /// </summary>
        public TValue DefaultValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultValue;
        }
    }
}
