using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 从游戏物体获取组件的方法。
    /// </summary>
    public enum GetComponentMethod
    {
        /// <seealso cref="GameObject.GetComponent{T}"/>
        Self,

        /// <seealso cref="GameObject.GetComponentInParent{T}()"/>
        Parent,

        /// <seealso cref="GameObject.GetComponentInParent{T}(bool)"/>
        ParentIncludingInactive,

        /// <seealso cref="GameObject.GetComponentInChildren{T}()"/>
        Children,

        /// <seealso cref="GameObject.GetComponentInChildren{T}(bool)"/>
        ChildrenIncludingInactive
    }
}
