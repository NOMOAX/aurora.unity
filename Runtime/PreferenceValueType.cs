using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// <see cref="PlayerPrefs"/> 中的基本值的类型。
    /// </summary>
    public enum PreferenceValueType
    {
        /// <summary>
        /// 32 位有符号整数。
        /// </summary>
        /// <seealso cref="int"/>
        Int32,

        /// <summary>
        /// 单精度浮点数。
        /// </summary>
        /// <seealso cref="float"/>
        Single,

        /// <summary>
        /// 字符串。
        /// </summary>
        /// <seealso cref="string"/>
        String
    }
}
