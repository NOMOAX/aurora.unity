using UnityEngine.EventSystems;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组成员，它们与 <see cref="UIBehaviour"/> 的一些公开的实例成员具有相同的签名。
    /// </summary>
    public interface IUIBehaviour : IMonoBehaviour
    {
        /// <seealso cref="UIBehaviour.IsActive"/>
        bool IsActive();

        /// <seealso cref="UIBehaviour.IsDestroyed"/>
        bool IsDestroyed();
    }
}
