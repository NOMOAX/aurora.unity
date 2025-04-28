using UnityEngine;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 无需预制体、在运行时直接创建的界面。
    /// </summary>
    public abstract class PrefabLessView : View
    {
        /// <summary>
        /// 设置 <see cref="GameObject.layer"/> 为"UI"。
        /// </summary>
        protected virtual void Awake()
        {
            gameObject.layer = UnityEnvironment.UILayer;
        }
    }
}
