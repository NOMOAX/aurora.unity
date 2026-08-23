using System.Collections;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="MonoBehaviour"/>.
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
