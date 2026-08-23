using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose value is a <see cref="float"/>.
    /// </summary>
    public sealed class IdentitySinglePreference : SinglePreference<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentitySinglePreference"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public IdentitySinglePreference(string key) : base(key, IdentityPreferenceConverterPair<float>.Instance)
        {
        }
    }
}
