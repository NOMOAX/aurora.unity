using UnityEngine.EventSystems;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="UIBehaviour"/>.
    /// </summary>
    public interface IUIBehaviour : IMonoBehaviour
    {
        /// <seealso cref="UIBehaviour.IsActive"/>
        bool IsActive();

        /// <seealso cref="UIBehaviour.IsDestroyed"/>
        bool IsDestroyed();
    }
}
