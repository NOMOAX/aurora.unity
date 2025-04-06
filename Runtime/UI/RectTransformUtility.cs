using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// <para>为 <see cref="RectTransform"/> 类提供工具方法。</para>
    /// <para>对于与 <see cref="RectTransform"/> 有关的英文词进行中文解释和翻译：</para>
    /// <list type="table">
    /// <listheader>
    /// <term>值</term>
    /// <description>含义</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="RectTransform"/></term>
    /// <description>矩形变换。具有矩形的位置、大小、锚点和轴心信息。</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.rect"/></term>
    /// <description>矩形（本地坐标系）。</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchorMin"/></term>
    /// <description>
    /// 左下角的锚点。定义为父矩形变换大小的一个比例。
    /// <br/>
    /// (0, 0) 相当于锚定到父矩形变换的左下角，(1, 1) 相当于锚定到父矩形变换的右上角。
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchorMax"/></term>
    /// <description>
    /// 右上角的锚点。定义为父矩形变换大小的一个比例。
    /// <br/>
    /// (0, 0) 相当于锚定到父矩形变换的左下角，(1, 1) 相当于锚定到父矩形变换的右上角。
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchoredPosition"/></term>
    /// <description>轴心相对于锚点参考点的位置。</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.sizeDelta"/></term>
    /// <description>自身大小减去由锚点定义的矩形的大小。</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.pivot"/></term>
    /// <description>
    /// 轴心。定义为自身大小的一个比例。(0, 0) 相当于左下角，(1, 1) 相当于右上角。
    /// <br/>
    /// </description>
    /// </item>
    /// <item>
    /// <term>anchor reference point</term>
    /// <description>
    /// 锚点参考点。
    /// <br/>
    /// （这个值由 <see cref="RectTransform.anchorMin"/>、<see cref="RectTransform.anchorMax"/> 和 <see cref="RectTransform.pivot"/> 计算得到。）
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchoredPosition3D"/></term>
    /// <description>
    /// 轴心相对于锚点参考点的 3D 位置。
    /// <br/>
    /// （这个值由 <see cref="RectTransform.anchoredPosition"/> 和 <see cref="Transform.localPosition"/> 计算得到。）
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.offsetMin"/></term>
    /// <description>
    /// 左下角相对于左下角的锚点的偏移。
    /// <br/>
    /// （这个值由 <see cref="RectTransform.anchoredPosition"/>、<see cref="RectTransform.sizeDelta"/> 和 <see cref="RectTransform.pivot"/> 计算得到。）
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.offsetMax"/></term>
    /// <description>
    /// 右上角相对于右上角的锚点的偏移。
    /// <br/>
    /// （这个值由 <see cref="RectTransform.anchoredPosition"/>、<see cref="RectTransform.sizeDelta"/> 和 <see cref="RectTransform.pivot"/> 计算得到。）
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public static class RectTransformUtility
    {
        /// <summary>
        /// 由用户自行使用，可作为 <see cref="RectTransform.GetLocalCorners">RectTransform.GetLocalCorners</see> 和 <see cref="RectTransform.GetWorldCorners">RectTransform.GetWorldCorners</see> 的参数。
        /// </summary>
        public static readonly Vector3[] FourCornersArray = new Vector3[4];

        #region 工具

        /// <summary>
        /// 使矩形变换与其父变换的四边对其。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void AlignToParentEdges(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (!(rectTransform.parent is RectTransform))
            {
                return;
            }
            rectTransform.anchorMin        = Vector2.zero;
            rectTransform.anchorMax        = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta        = Vector2.zero;
        }

        #endregion

        #region 基本取值

        /// <summary>
        /// 获取矩形变换的矩形（本地坐标系）。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的矩形（本地坐标系）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Rect GetRect(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect;
        }

        /// <summary>
        /// 获取矩形变换的左下角的锚点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的左下角的锚点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetAnchorMin(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchorMin;
        }

        /// <summary>
        /// 获取矩形变换的右上角的锚点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的右上角的锚点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetAnchorMax(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchorMax;
        }

        /// <summary>
        /// 获取矩形变换的轴心相对于锚点参考点的位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的轴心相对于锚点参考点的位置。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetAnchoredPosition(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchoredPosition;
        }

        /// <summary>
        /// 获取矩形变换的大小减去由锚点定义的矩形的大小。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的大小减去由锚点定义的矩形的大小。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <remarks>如果矩形变换没有父变换，或父变换不是矩形变换，则认为由锚点定义的矩形的大小为 (0, 0)，此时获取到的是矩形变换的大小。</remarks>
        public static Vector2 GetSizeDelta(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.sizeDelta;
        }

        /// <summary>
        /// 获取矩形变换的轴心。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的轴心。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetPivot(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.pivot;
        }

        /// <summary>
        /// 获取矩形变换的锚点参考点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的锚点参考点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetAnchorReferencePoint(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;
            var pivot     = rectTransform.pivot;
            return anchorMin + (anchorMax - anchorMin) * pivot;
        }

        /// <summary>
        /// 获取矩形变换的轴心相对于锚点参考点的 3D 位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的轴心相对于锚点参考点的 3D 位置。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetAnchoredPosition3D(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchoredPosition3D;
        }

        /// <summary>
        /// 获取矩形变换的左下角相对于左下角的锚点的偏移。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的左下角相对于左下角的锚点的偏移。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetOffsetMin(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.offsetMin;
        }

        /// <summary>
        /// 获取矩形变换的右上角相对于右上角的锚点的偏移。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的右上角相对于右上角的锚点的偏移。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetOffsetMax(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.offsetMax;
        }

        #endregion

        #region 基本赋值

        /// <summary>
        /// 设置矩形变换的左下角的锚点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchorMin(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchorMin = value;
        }

        /// <summary>
        /// 设置矩形变换的右上角的锚点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchorMax(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchorMax = value;
        }

        /// <summary>
        /// 设置矩形变换的轴心相对于锚点参考点的位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchoredPosition(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchoredPosition = value;
        }

        /// <summary>
        /// 设置矩形变换的大小减去由锚点定义的矩形的大小。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetSizeDelta(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.sizeDelta = value;
        }

        /// <summary>
        /// 设置矩形变换的轴心。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetPivot(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.pivot = value;
        }

        /// <summary>
        /// 设置矩形变换的轴心相对于锚点参考点的 3D 位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchoredPosition3D(RectTransform rectTransform, Vector3 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchoredPosition3D = value;
        }

        /// <summary>
        /// 设置矩形变换的左下角相对于左下角的锚点的偏移。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetOffsetMin(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.offsetMin = value;
        }

        /// <summary>
        /// 设置矩形变换的右上角相对于右上角的锚点的偏移。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetOffsetMax(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.offsetMax = value;
        }

        #endregion

        #region 进阶取值

        /// <summary>
        /// 获取矩形变换的宽度。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的宽度。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static float GetWidth(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.width;
        }

        /// <summary>
        /// 获取矩形变换的高度。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的高度。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static float GetHeight(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.height;
        }

        /// <summary>
        /// 获取矩形变换的大小。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的大小。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetSize(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.size;
        }

        /// <summary>
        /// 获取矩形变换的边的位置（本地坐标系）。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="edge">边。</param>
        /// <returns>矩形变换的边的位置（本地坐标系）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="edge"/> 不是定义在 <see cref="RectTransform.Edge"/> 中的值。</exception>
        public static float GetEdge(RectTransform rectTransform, RectTransform.Edge edge)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return edge switch
            {
                RectTransform.Edge.Left   => rectTransform.rect.xMin,
                RectTransform.Edge.Right  => rectTransform.rect.xMax,
                RectTransform.Edge.Top    => rectTransform.rect.yMax,
                RectTransform.Edge.Bottom => rectTransform.rect.yMin,
                _                         => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <returns>一个矩形变换本地坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector2 GetLocalPoint(RectTransform rectTransform, Vector2 normalizedRectCoordinates)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetLocalPoint(rectTransform, normalizedRectCoordinates);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <returns>一个世界坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetWorldPoint(RectTransform rectTransform, Vector2 normalizedRectCoordinates)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <param name="localPointOffset">在转换到世界坐标系前，要给本地坐标系中的点加上的偏移量。</param>
        /// <returns>一个世界坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetWorldPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Vector2       localPointOffset)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系，再将点从世界坐标系转换到屏幕坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <param name="camera">用于将点从世界坐标系转换到屏幕坐标系的相机。</param>
        /// <returns>一个屏幕坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 或 <paramref name="camera"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetScreenPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Camera        camera)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
            return camera.WorldToScreenPoint(worldPoint);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系，再将点从世界坐标系转换到屏幕坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <param name="camera">用于将点从世界坐标系转换到屏幕坐标系的相机。</param>
        /// <param name="eye">详见 <see cref="Camera.MonoOrStereoscopicEye"/>。</param>
        /// <returns>一个屏幕坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 或 <paramref name="camera"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetScreenPoint(
            RectTransform                rectTransform,
            Vector2                      normalizedRectCoordinates,
            Camera                       camera,
            Camera.MonoOrStereoscopicEye eye)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
            return camera.WorldToScreenPoint(worldPoint, eye);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系，再将点从世界坐标系转换到屏幕坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <param name="camera">用于将点从世界坐标系转换到屏幕坐标系的相机。</param>
        /// <param name="localPointOffset">在转换到世界坐标系前，要给本地坐标系中的点加上的偏移量。</param>
        /// <param name="worldPointOffset">在转换到屏幕坐标系前，要给世界坐标系中的点加上的偏移量。</param>
        /// <returns>一个屏幕坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 或 <paramref name="camera"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetScreenPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Camera        camera,
            Vector2       localPointOffset,
            Vector3       worldPointOffset)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
            return camera.WorldToScreenPoint(worldPoint + worldPointOffset);
        }

        /// <summary>
        /// 根据指定的标准化坐标，计算矩形变换本地坐标系中的点，然后将点从本地坐标系转换到世界坐标系，再将点从世界坐标系转换到屏幕坐标系。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="normalizedRectCoordinates">标准化坐标。</param>
        /// <param name="camera">用于将点从世界坐标系转换到屏幕坐标系的相机。</param>
        /// <param name="eye">详见 <see cref="Camera.MonoOrStereoscopicEye"/>。</param>
        /// <param name="localPointOffset">在转换到世界坐标系前，要给本地坐标系中的点加上的偏移量。</param>
        /// <param name="worldPointOffset">在转换到屏幕坐标系前，要给世界坐标系中的点加上的偏移量。</param>
        /// <returns>一个屏幕坐标系中的点。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 或 <paramref name="camera"/> 为 <see langword="null"/>。</exception>
        public static Vector3 GetScreenPoint(
            RectTransform                rectTransform,
            Vector2                      normalizedRectCoordinates,
            Camera                       camera,
            Camera.MonoOrStereoscopicEye eye,
            Vector2                      localPointOffset,
            Vector3                      worldPointOffset)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
            return camera.WorldToScreenPoint(worldPoint + worldPointOffset, eye);
        }

        /// <summary>
        /// 获取矩形变换的轴心相对于锚点参考点的位置的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的轴心相对于锚点参考点的位置的 x 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static float GetInspectorPosX(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.anchoredPosition.x;
        }

        /// <summary>
        /// 获取矩形变换的轴心相对于锚点参考点的位置的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的轴心相对于锚点参考点的位置的 y 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static float GetInspectorPosY(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.anchoredPosition.y;
        }

        /// <summary>
        /// 获取矩形变换的本地位置的 z 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的本地位置的 z 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static float GetInspectorPosZ(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.localPosition.y;
        }

        /// <summary>
        /// 获取矩形变换的左下角相对于左下角的锚点的偏移的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的左下角相对于左下角的锚点的偏移的 x 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static float GetInspectorLeft(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.offsetMin.x;
        }

        /// <summary>
        /// 获取矩形变换的右上角相对于右上角的锚点的偏移的 x 分量的相反数。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的右上角相对于右上角的锚点的偏移的 x 分量的相反数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static float GetInspectorRight(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return -rectTransform.offsetMax.x;
        }

        /// <summary>
        /// 获取矩形变换的左下角相对于左下角的锚点的偏移的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的左下角相对于左下角的锚点的偏移的 x 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static float GetInspectorBottom(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.offsetMin.y;
        }

        /// <summary>
        /// 获取矩形变换的右上角相对于右上角的锚点的偏移的 y 分量的相反数。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的右上角相对于右上角的锚点的偏移的 x 分量的相反数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static float GetInspectorTop(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return -rectTransform.offsetMax.y;
        }

        /// <summary>
        /// 获取矩形变换的大小减去由锚点定义的矩形的大小的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的大小减去由锚点定义的矩形的大小的 x 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static float GetInspectorWidth(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.sizeDelta.x;
        }

        /// <summary>
        /// 获取矩形变换的大小减去由锚点定义的矩形的大小的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的大小减去由锚点定义的矩形的大小的 y 分量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static float GetInspectorHeight(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            return rectTransform.sizeDelta.y;
        }

        /// <summary>
        /// 获取矩形变换的布局根变换。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <returns>矩形变换的布局根变换。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static RectTransform GetLayoutRoot(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var components = PredefinedPools<Component>.List.Get();
            try
            {
                var layoutRoot = rectTransform;
                for (var parent = layoutRoot.parent as RectTransform;
                     parent != null;
                     parent = parent.parent as RectTransform)
                {
                    parent.GetComponents(typeof(ILayoutGroup), components);
                    if (ContainsActiveAndEnabledBehaviour(components))
                    {
                        layoutRoot = parent;
                    }
                    else
                    {
                        break;
                    }
                }
                if (layoutRoot != rectTransform)
                {
                    return layoutRoot;
                }
                layoutRoot.GetComponents(typeof(ILayoutController), components);
                return ContainsActiveAndEnabledBehaviour(components) ? layoutRoot : null;
            }
            finally
            {
                PredefinedPools<Component>.List.Return(components);
            }
        }

        #endregion

        #region 进阶赋值

        /// <summary>
        /// 设置矩形变换的宽度。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetWidth(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
        }

        /// <summary>
        /// 设置矩形变换的高度。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetHeight(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
        }

        /// <summary>
        /// 设置矩形变换的大小。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetSize(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   value.y);
            var parentSize = GetParentSize(rectTransform);
            rectTransform.sizeDelta = value - parentSize * (rectTransform.anchorMax - rectTransform.anchorMin);
        }

        /// <summary>
        /// 设置矩形变换的左下角的锚点，并且相应地调整其它属性，以使得矩形变换看上去仍然处于原有位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchorMinSmart(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin == value)
            {
                return;
            }
            var parent = rectTransform.parent as RectTransform;
            if (parent == null)
            {
                rectTransform.anchorMin = value;
                return;
            }
            var oldValue      = rectTransform.anchorMin;
            var offsetSize    = (value - oldValue) * parent.rect.size;
            var roundingDelta = Vector2.zero;
            if (ShouldDoIntSnapping(rectTransform))
            {
                for (var i = 0; i < 2; i++)
                {
                    var offsetSizeComponent = offsetSize[i];
                    roundingDelta[i] = Mathf.Round(offsetSizeComponent) - offsetSizeComponent;
                }
            }
            offsetSize += roundingDelta;
            var offsetPosition = offsetSize * (Vector2.one - rectTransform.pivot);
            rectTransform.anchorMin        =  value;
            rectTransform.anchoredPosition -= offsetPosition;
            rectTransform.sizeDelta        += offsetSize;
        }

        /// <summary>
        /// 设置矩形变换的右上角的锚点，并且相应地调整其它属性，以使得矩形变换看上去仍然处于原有位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetAnchorMaxSmart(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMax == value)
            {
                return;
            }
            var parent = rectTransform.parent as RectTransform;
            if (parent == null)
            {
                rectTransform.anchorMax = value;
                return;
            }
            var oldValue      = rectTransform.anchorMax;
            var offsetSize    = (value - oldValue) * parent.rect.size;
            var roundingDelta = Vector2.zero;
            if (ShouldDoIntSnapping(rectTransform))
            {
                for (var i = 0; i < 2; i++)
                {
                    var offsetSizeComponent = offsetSize[i];
                    roundingDelta[i] = Mathf.Round(offsetSizeComponent) - offsetSizeComponent;
                }
            }
            offsetSize += roundingDelta;
            var offsetPosition = offsetSize * rectTransform.pivot;
            rectTransform.anchorMax        =  value;
            rectTransform.anchoredPosition -= offsetPosition;
            rectTransform.sizeDelta        -= offsetSize;
        }

        /// <summary>
        /// 设置矩形变换的轴心，并且相应地调整其它属性，以使得矩形变换看上去仍然处于原有位置。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetPivotSmart(RectTransform rectTransform, Vector2 value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.pivot == value)
            {
                return;
            }
            var cornerBefore = GetReferenceCorner(rectTransform);
            rectTransform.pivot = value;
            var cornerAfter  = GetReferenceCorner(rectTransform);
            var cornerOffset = cornerAfter - cornerBefore;
            rectTransform.anchoredPosition -= (Vector2) cornerOffset;
            var position = rectTransform.position;
            position.z             -= cornerOffset.z;
            rectTransform.position =  position;
        }

        /// <summary>
        /// 设置矩形变换的轴心相对于锚点参考点的位置的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static void SetInspectorPosX(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x             = value;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// 设置矩形变换的轴心相对于锚点参考点的位置的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static void SetInspectorPosY(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.y             = value;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// 设置矩形变换的轴心相对于锚点参考点的位置的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        public static void SetInspectorPosZ(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            var localPosition = rectTransform.localPosition;
            localPosition.z             = value;
            rectTransform.localPosition = localPosition;
        }

        /// <summary>
        /// 设置矩形变换的左下角相对于左下角的锚点的偏移的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static void SetInspectorLeft(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var offsetMin = rectTransform.offsetMin;
            offsetMin.x             = value;
            rectTransform.offsetMin = offsetMin;
        }

        /// <summary>
        /// 设置矩形变换的右上角相对于右上角的锚点的偏移的 x 分量的相反数。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static void SetInspectorRight(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var offsetMax = rectTransform.offsetMax;
            offsetMax.x             = -value;
            rectTransform.offsetMax = offsetMax;
        }

        /// <summary>
        /// 设置矩形变换的左下角相对于左下角的锚点的偏移的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static void SetInspectorBottom(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var offsetMin = rectTransform.offsetMin;
            offsetMin.y             = value;
            rectTransform.offsetMin = offsetMin;
        }

        /// <summary>
        /// 设置矩形变换的右上角相对于右上角的锚点的偏移的 y 分量的相反数。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static void SetInspectorTop(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var offsetMax = rectTransform.offsetMax;
            offsetMax.y             = -value;
            rectTransform.offsetMax = offsetMax;
        }

        /// <summary>
        /// 设置矩形变换的大小减去由锚点定义的矩形的大小的 x 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.x"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.x"/> 分量。</exception>
        public static void SetInspectorWidth(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.x != rectTransform.anchorMax.x)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x             = value;
            rectTransform.sizeDelta = sizeDelta;
        }

        /// <summary>
        /// 设置矩形变换的大小减去由锚点定义的矩形的大小的 y 分量。
        /// </summary>
        /// <param name="rectTransform">矩形变换。</param>
        /// <param name="value">要设置的值。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="rectTransform"/> 的 <see cref="RectTransform.anchorMin"/> 的 <see cref="Vector2.y"/> 分量不等于 <see cref="RectTransform.anchorMax"/> 的 <see cref="Vector2.y"/> 分量。</exception>
        public static void SetInspectorHeight(RectTransform rectTransform, float value)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException();
            }
            if (rectTransform.anchorMin.y != rectTransform.anchorMax.y)
            {
                throw new ArgumentException(null, nameof(rectTransform));
            }
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y             = value;
            rectTransform.sizeDelta = sizeDelta;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 InternalGetLocalPoint(RectTransform rectTransform, Vector2 normalizedRectCoordinates)
        {
            var rect       = rectTransform.rect;
            var localPoint = AuroraUnityMath.NormalizedToPointUnclamped(rect, normalizedRectCoordinates);
            return localPoint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 InternalGetWorldPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Vector2       localPointOffset)
        {
            var localPoint = InternalGetLocalPoint(rectTransform, normalizedRectCoordinates);
            var worldPoint = rectTransform.TransformPoint(localPoint + localPointOffset);
            return worldPoint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GetParentSize(RectTransform rectTransform)
        {
            var parentRect = rectTransform.parent as RectTransform;
            return parentRect != null ? parentRect.rect.size : Vector2.zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldDoIntSnapping(Component component)
        {
            var canvas = component.GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.WorldSpace;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetReferenceCorner(RectTransform rectTransform)
        {
            return (Vector3) rectTransform.rect.min + rectTransform.localPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsActiveAndEnabledBehaviour(List<Component> components)
        {
            return components.FindIndex(PredicateIsActiveAndEnabledBehaviour) >= 0;
        }

        private static readonly Predicate<Component> PredicateIsActiveAndEnabledBehaviour = IsActiveAndEnabledBehaviour;

        private static bool IsActiveAndEnabledBehaviour(Component component)
        {
            return component != null && component is Behaviour { isActiveAndEnabled: true };
        }
    }
}
