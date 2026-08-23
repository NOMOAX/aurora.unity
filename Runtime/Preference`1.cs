using System;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the user's value.</typeparam>
    public abstract class Preference<TValue>
    {
        private readonly string _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="Preference{T}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        protected Preference(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        /// <summary>
        /// The key.
        /// </summary>
        public string Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        /// <summary>
        /// Determines whether <see cref="Key"/> exists in <see cref="PlayerPrefs"/>.
        /// </summary>
        public bool KeyExists => PlayerPrefs.HasKey(_key);

        /// <summary>
        /// The type of the primitive value stored under <see cref="Key"/> in <see cref="PlayerPrefs"/>.
        /// </summary>
        public abstract PreferenceValueType ValueType { get; }

        /// <summary>
        /// Sets the value stored under <see cref="Key"/> in <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>In the implementation of a derived class, do not call <see cref="PlayerPrefs.Save"/>.</remarks>
        public abstract void SetValue(TValue value);

        /// <summary>
        /// Gets the value stored under <see cref="Key"/> in <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <returns>The corresponding value if <see cref="Key"/> exists in <see cref="PlayerPrefs"/>; otherwise the default value of <typeparamref name="TValue"/>.</returns>
        public abstract TValue GetValue();

        /// <summary>
        /// Gets the value stored under <see cref="Key"/> in <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <param name="defaultValue">The default value returned when <see cref="Key"/> does not exist in <see cref="PlayerPrefs"/>.</param>
        /// <returns>The corresponding value if this key exists in <see cref="PlayerPrefs"/>; otherwise <paramref name="defaultValue"/>.</returns>
        public abstract TValue GetValue(TValue defaultValue);

        /// <summary>
        /// Removes <see cref="Key"/> from <see cref="PlayerPrefs"/>.
        /// </summary>
        /// <remarks>If <see cref="Key"/> does not exist in <see cref="PlayerPrefs"/>, this method has no effect.</remarks>
        public void Remove()
        {
            PlayerPrefs.DeleteKey(_key);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.Append(GetType());
                stringBuilder.Append(':');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(Key));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(Key);
                stringBuilder.Append(',');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(KeyExists));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                var keyExists = KeyExists;
                stringBuilder.Append(keyExists);
                stringBuilder.Append(',');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(ValueType));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(ValueType);
                if (keyExists)
                {
                    stringBuilder.Append(',');
                    stringBuilder.Append(' ');
                    stringBuilder.Append("Value");
                    stringBuilder.Append(' ');
                    stringBuilder.Append('=');
                    stringBuilder.Append(' ');
                    if (TryGetValueWhenKeyExists(out var value, out var exception))
                    {
                        stringBuilder.Append(value);
                    }
                    else
                    {
                        stringBuilder.Append(exception.GetType());
                        stringBuilder.Append(' ');
                        stringBuilder.Append("was throw");
                    }
                }
                return stringBuilder.ToString();
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }

            bool TryGetValueWhenKeyExists(out TValue value, out Exception exception)
            {
                try
                {
                    value     = GetValue();
                    exception = null;
                    return true;
                }
                catch (Exception e)
                {
                    value     = default;
                    exception = e;
                    return false;
                }
            }
        }
    }
}
