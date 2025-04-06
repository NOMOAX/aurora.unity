using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 变换为矩形变换的单线行为。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class MonoBehaviour2D : MonoBehaviour
    {
        private RectTransform _rectTransform;

        /// <summary>
        /// 获取与此实例关联的矩形变换。
        /// </summary>
        public RectTransform RectTransform =>
            _rectTransform != null ? _rectTransform : _rectTransform = (RectTransform) transform;
    }
}
