using System;
using System.Runtime.CompilerServices;

namespace Aurora.Unity
{
    /// <summary>
    /// 对另一个 <see cref="Preference{TValue}"/> 进行包装，并持有一个默认值，以便执行额外操作。
    /// </summary>
    /// <inheritdoc />
    public abstract class DefaultValuePreference<TValue> : WrappedPreference<TValue>
    {
        private readonly TValue _defaultValue;

        /// <summary>
        /// 初始化 <see cref="DefaultValuePreference{TValue}"/> 类的新实例。
        /// </summary>
        /// <param name="preference">被包装的 <see cref="Preference{TValue}"/>。</param>
        /// <param name="defaultValue">指定的默认值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="preference"/> 为 <see langword="null"/>。</exception>
        protected DefaultValuePreference(Preference<TValue> preference, TValue defaultValue = default) : base(
            preference
        )
        {
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// 从构造函数传入的默认值。
        /// </summary>
        public TValue DefaultValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultValue;
        }
    }
}
