using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// The type of the primitive value in <see cref="PlayerPrefs"/>.
    /// </summary>
    public enum PreferenceValueType
    {
        /// <summary>
        /// A 32-bit signed integer.
        /// </summary>
        /// <seealso cref="int"/>
        Int32,

        /// <summary>
        /// A single-precision floating-point number.
        /// </summary>
        /// <seealso cref="float"/>
        Single,

        /// <summary>
        /// A string.
        /// </summary>
        /// <seealso cref="string"/>
        String
    }
}
