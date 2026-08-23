using System;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 表示 <see cref="PlayerPrefs"/> 中的一项。
    /// </summary>
    /// <typeparam name="TValue">用户使用的值的类型。</typeparam>
    public abstract class Preference<TValue>
    {
        private readonly string _key;

        /// <summary>
        /// 初始化 <see cref="Preference{T}"/> 类的新实例。
        /// </summary>
        /// <param name="key">键。</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <see langword="null"/>。</exception>
        protected Preference(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        /// <summary>
        /// 键。
        /// </summary>
        public string Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        /// <summary>
        /// 判断 <see cref="PlayerPrefs"/> 中是否存在 <see cref="Key"/>。
        /// </summary>
        public bool KeyExists => PlayerPrefs.HasKey(_key);

        /// <summary>
        /// 在 <see cref="PlayerPrefs"/> 中 <see cref="Key"/> 对应的基本值的类型。
        /// </summary>
        public abstract PreferenceValueType ValueType { get; }

        /// <summary>
        /// 设置 <see cref="PlayerPrefs"/> 中 <see cref="Key"/> 对应的值。
        /// </summary>
        /// <param name="value">值。</param>
        /// <remarks>在派生类的实现中，不要调用 <see cref="PlayerPrefs.Save"/>。</remarks>
        public abstract void SetValue(TValue value);

        /// <summary>
        /// 获取 <see cref="PlayerPrefs"/> 中 <see cref="Key"/> 对应的值。
        /// </summary>
        /// <returns>如果 <see cref="PlayerPrefs"/> 中存在 <see cref="Key"/>，则为对应的值；否则为 <typeparamref name="TValue"/> 的默认值。</returns>
        public abstract TValue GetValue();

        /// <summary>
        /// 获取 <see cref="PlayerPrefs"/> 中 <see cref="Key"/> 对应的值。
        /// </summary>
        /// <param name="defaultValue">当 <see cref="PlayerPrefs"/> 中不存在 <see cref="Key"/> 时，返回的默认值。</param>
        /// <returns>如果 <see cref="PlayerPrefs"/> 中存在此键，则为对应的值；否则为 <paramref name="defaultValue"/>。</returns>
        public abstract TValue GetValue(TValue defaultValue);

        /// <summary>
        /// 从 <see cref="PlayerPrefs"/> 中移除 <see cref="Key"/>。
        /// </summary>
        /// <remarks>如果 <see cref="PlayerPrefs"/> 中不存在 <see cref="Key"/>，此方法啊不会产生任何作用。</remarks>
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
