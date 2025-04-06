using System.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组成员，它们与 <see cref="MonoBehaviour"/> 的一些公开的实例成员具有相同的签名。
    /// </summary>
    public interface IMonoBehaviour : IBehaviour
    {
        /// <seealso cref="MonoBehaviour.StartCoroutine(System.Collections.IEnumerator)"/>
        Coroutine StartCoroutine(IEnumerator routine);

        /// <seealso cref="MonoBehaviour.StopCoroutine(System.Collections.IEnumerator)"/>
        void StopCoroutine(IEnumerator routine);

        /// <seealso cref="MonoBehaviour.StopCoroutine(UnityEngine.Coroutine)"/>
        void StopCoroutine(Coroutine routine);

        /// <seealso cref="MonoBehaviour.StopAllCoroutines"/>
        void StopAllCoroutines();
    }
}
