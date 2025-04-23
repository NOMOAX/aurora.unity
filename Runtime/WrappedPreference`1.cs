using System;

namespace Aurora.Unity
{
    /// <summary>
    /// 对另一个 <see cref="Preference{TValue}"/> 进行包装，以便执行额外操作。
    /// </summary>
    /// <inheritdoc />
    public class WrappedPreference<TValue> : Preference<TValue>
    {
        public readonly Preference<TValue> Preference;

        /// <summary>
        /// 初始化 <see cref="WrappedPreference{TValue}"/> 类的新实例。
        /// </summary>
        /// <param name="preference">被包装的 <see cref="Preference{TValue}"/>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="preference"/> 为 <see langword="null"/>。</exception>
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
