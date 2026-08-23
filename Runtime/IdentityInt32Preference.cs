using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose value is an <see cref="int"/>.
    /// </summary>
    public sealed class IdentityInt32Preference : Int32Preference<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityInt32Preference"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public IdentityInt32Preference(string key) : base(key, IdentityPreferenceConverterPair<int>.Instance)
        {
        }
    }
}
