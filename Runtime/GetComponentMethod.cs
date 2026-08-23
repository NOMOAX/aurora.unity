using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// The method used to get a component from a game object.
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
