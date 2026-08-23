using UnityEngine;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// A view created directly at runtime without a prefab.
    /// </summary>
    public abstract class PrefabLessView : View
    {
        /// <summary>
        /// Sets <see cref="GameObject.layer"/> to "UI".
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            gameObject.layer = UnityEnvironment.UILayer;
        }
    }
}
