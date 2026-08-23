using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Represents an item in <see cref="PlayerPrefs"/> whose value is a <see cref="string"/>.
    /// </summary>
    public sealed class IdentityStringPreference : StringPreference<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityStringPreference"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public IdentityStringPreference(string key) : base(key, IdentityPreferenceConverterPair<string>.Instance)
        {
        }
    }
}
