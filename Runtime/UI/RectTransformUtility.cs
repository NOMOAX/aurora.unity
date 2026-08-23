using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aurora.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// <para>Provides utility methods for the <see cref="RectTransform"/> class.</para>
    /// <para>Explains and translates the English terms related to <see cref="RectTransform"/> in Chinese:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="RectTransform"/></term>
    /// <description>RectTransform. Has rectangle position, size, anchor, and pivot information.</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.rect"/></term>
    /// <description>Rectangle (in local coordinate system).</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchorMin"/></term>
    /// <description>
    /// The bottom-left anchor. Defined as a ratio of the parent RectTransform size.
    /// <br/>
    /// (0, 0) is equivalent to anchoring to the bottom-left corner of the parent RectTransform, (1, 1) is equivalent to anchoring to the top-right corner.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchorMax"/></term>
    /// <description>
    /// The top-right anchor. Defined as a ratio of the parent RectTransform size.
    /// <br/>
    /// (0, 0) is equivalent to anchoring to the bottom-left corner of the parent RectTransform, (1, 1) is equivalent to anchoring to the top-right corner.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchoredPosition"/></term>
    /// <description>The position of the pivot relative to the anchor reference point.</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.sizeDelta"/></term>
    /// <description>The own size minus the size of the rectangle defined by the anchors.</description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.pivot"/></term>
    /// <description>
    /// The pivot. Defined as a ratio of the own size. (0, 0) is equivalent to the bottom-left corner, (1, 1) is equivalent to the top-right corner.
    /// <br/>
    /// </description>
    /// </item>
    /// <item>
    /// <term>anchor reference point</term>
    /// <description>
    /// The anchor reference point.
    /// <br/>
    /// (This value is computed from <see cref="RectTransform.anchorMin"/>, <see cref="RectTransform.anchorMax"/>, and <see cref="RectTransform.pivot"/>.)
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.anchoredPosition3D"/></term>
    /// <description>
    /// The 3D position of the pivot relative to the anchor reference point.
    /// <br/>
    /// (This value is computed from <see cref="RectTransform.anchoredPosition"/> and <see cref="Transform.localPosition"/>.)
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.offsetMin"/></term>
    /// <description>
    /// The bottom-left corner offset relative to the bottom-left anchor.
    /// <br/>
    /// (This value is computed from <see cref="RectTransform.anchoredPosition"/>, <see cref="RectTransform.sizeDelta"/>, and <see cref="RectTransform.pivot"/>.)
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="RectTransform.offsetMax"/></term>
    /// <description>
    /// The top-right corner offset relative to the top-right anchor.
    /// <br/>
    /// (This value is computed from <see cref="RectTransform.anchoredPosition"/>, <see cref="RectTransform.sizeDelta"/>, and <see cref="RectTransform.pivot"/>.)
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public static class RectTransformUtility
    {
        /// <summary>
        /// For the user's own use, can be used as an argument to <see cref="RectTransform.GetLocalCorners">RectTransform.GetLocalCorners</see> and <see cref="RectTransform.GetWorldCorners">RectTransform.GetWorldCorners</see>.
        /// </summary>
        public static readonly Vector3[] FourCornersArray = new Vector3[4];

        #region Utility

        /// <summary>
        /// Gets a message suitable for logging a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>A message suitable for logging a RectTransform.</returns>
        public static string GetLogMessage(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                return null;
            }
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.Append(rectTransform.name);
                stringBuilder.Append(' ');
                stringBuilder.Append('(');
                stringBuilder.Append(nameof(RectTransform));
                stringBuilder.Append(')');
                stringBuilder.Append(' ');
                stringBuilder.Append('(');
                stringBuilder.Append("scene path");
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.GetScenePath());
                stringBuilder.Append(')');
                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(Transform.localPosition));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.localPosition);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(Transform.localRotation));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.localRotation);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(Transform.localScale));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.localScale);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.anchorMin));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.anchorMin);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.anchorMax));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.anchorMax);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.anchoredPosition));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.anchoredPosition);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.sizeDelta));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.sizeDelta);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.pivot));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.pivot);

                stringBuilder.Append('\n');
                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(Rect.size));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.rect.size);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.offsetMin));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.offsetMin);

                stringBuilder.Append('\n');

                stringBuilder.Append('-');
                stringBuilder.Append(' ');
                stringBuilder.Append(nameof(RectTransform.offsetMax));
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                stringBuilder.Append(rectTransform.offsetMax);

                return stringBuilder.ToString();
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }

        /// <summary>
        /// Aligns the four edges of a RectTransform with its parent transform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void AlignToParentEdges(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.parent is not RectTransform)
            {
                return;
            }
            rectTransform.anchorMin        = Vector2.zero;
            rectTransform.anchorMax        = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta        = Vector2.zero;
        }

        #endregion

        #region Basic getters

        /// <summary>
        /// Gets the rectangle of a RectTransform (in local coordinate system).
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The rectangle of the RectTransform (in local coordinate system).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Rect GetRect(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect;
        }

        /// <summary>
        /// Gets the bottom-left anchor of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The bottom-left anchor of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetAnchorMin(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchorMin;
        }

        /// <summary>
        /// Gets the top-right anchor of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The top-right anchor of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetAnchorMax(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchorMax;
        }

        /// <summary>
        /// Gets the position of the pivot of a RectTransform relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The position of the pivot relative to the anchor reference point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetAnchoredPosition(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchoredPosition;
        }

        /// <summary>
        /// Gets the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The size minus the size of the rectangle defined by the anchors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <remarks>If the RectTransform has no parent transform, or its parent is not a RectTransform, the size of the rectangle defined by the anchors is considered (0, 0), in which case the RectTransform size is returned.</remarks>
        public static Vector2 GetSizeDelta(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.sizeDelta;
        }

        /// <summary>
        /// Gets the pivot of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The pivot of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetPivot(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.pivot;
        }

        /// <summary>
        /// Gets the anchor reference point of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The anchor reference point of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetAnchorReferencePoint(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;
            var pivot     = rectTransform.pivot;
            return anchorMin + (anchorMax - anchorMin) * pivot;
        }

        /// <summary>
        /// Gets the 3D position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The 3D position of the pivot relative to the anchor reference point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector3 GetAnchoredPosition3D(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.anchoredPosition3D;
        }

        /// <summary>
        /// Gets the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The bottom-left corner offset relative to the bottom-left anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetOffsetMin(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.offsetMin;
        }

        /// <summary>
        /// Gets the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The top-right corner offset relative to the top-right anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetOffsetMax(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.offsetMax;
        }

        #endregion

        #region Basic setters

        /// <summary>
        /// Sets the bottom-left anchor of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchorMin(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchorMin = value;
        }

        /// <summary>
        /// Sets the top-right anchor of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchorMax(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchorMax = value;
        }

        /// <summary>
        /// Sets the position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchoredPosition(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchoredPosition = value;
        }

        /// <summary>
        /// Sets the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetSizeDelta(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.sizeDelta = value;
        }

        /// <summary>
        /// Sets the pivot of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetPivot(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.pivot = value;
        }

        /// <summary>
        /// Sets the 3D position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchoredPosition3D(RectTransform rectTransform, Vector3 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.anchoredPosition3D = value;
        }

        /// <summary>
        /// Sets the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetOffsetMin(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.offsetMin = value;
        }

        /// <summary>
        /// Sets the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetOffsetMax(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.offsetMax = value;
        }

        #endregion

        #region Advanced getters

        /// <summary>
        /// Gets the width of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The width of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static float GetWidth(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.width;
        }

        /// <summary>
        /// Gets the height of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The height of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static float GetHeight(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.height;
        }

        /// <summary>
        /// Gets the size of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The size of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetSize(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.rect.size;
        }

        /// <summary>
        /// Gets the position of an edge of a RectTransform (in local coordinate system).
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="edge">The edge.</param>
        /// <returns>The position of the edge (in local coordinate system).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="edge"/> is not a member defined in the <see cref="RectTransform.Edge"/> enum.</exception>
        public static float GetEdge(RectTransform rectTransform, RectTransform.Edge edge)
        {
            if (!rectTransform)
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
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <returns>A point in the local coordinate system of a RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector2 GetLocalPoint(RectTransform rectTransform, Vector2 normalizedRectCoordinates)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetLocalPoint(rectTransform, normalizedRectCoordinates);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <returns>A point in the world coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector3 GetWorldPoint(RectTransform rectTransform, Vector2 normalizedRectCoordinates)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <param name="localPointOffset">The offset to add to the local point before converting to the world coordinate system.</param>
        /// <returns>A point in the world coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static Vector3 GetWorldPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Vector2       localPointOffset)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system, and then from the world to the screen coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <param name="camera">The camera used to convert the point from the world to the screen coordinate system.</param>
        /// <returns>A point in the screen coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> or <paramref name="camera"/> is <see langword="null"/>.</exception>
        public static Vector3 GetScreenPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Camera        camera)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (!camera)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
            return camera.WorldToScreenPoint(worldPoint);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system, and then from the world to the screen coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <param name="camera">The camera used to convert the point from the world to the screen coordinate system.</param>
        /// <param name="eye">See <see cref="Camera.MonoOrStereoscopicEye"/>.</param>
        /// <returns>A point in the screen coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> or <paramref name="camera"/> is <see langword="null"/>.</exception>
        public static Vector3 GetScreenPoint(
            RectTransform                rectTransform,
            Vector2                      normalizedRectCoordinates,
            Camera                       camera,
            Camera.MonoOrStereoscopicEye eye)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (!camera)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, Vector2.zero);
            return camera.WorldToScreenPoint(worldPoint, eye);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system, and then from the world to the screen coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <param name="camera">The camera used to convert the point from the world to the screen coordinate system.</param>
        /// <param name="localPointOffset">The offset to add to the local point before converting to the world coordinate system.</param>
        /// <param name="worldPointOffset">The offset to add to the world point before converting to the screen coordinate system.</param>
        /// <returns>A point in the screen coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> or <paramref name="camera"/> is <see langword="null"/>.</exception>
        public static Vector3 GetScreenPoint(
            RectTransform rectTransform,
            Vector2       normalizedRectCoordinates,
            Camera        camera,
            Vector2       localPointOffset,
            Vector3       worldPointOffset)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (!camera)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
            return camera.WorldToScreenPoint(worldPoint + worldPointOffset);
        }

        /// <summary>
        /// Computes a point in the local coordinate system of a RectTransform from the specified normalized coordinates, then converts the point from the local to the world coordinate system, and then from the world to the screen coordinate system.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="normalizedRectCoordinates">The normalized coordinates.</param>
        /// <param name="camera">The camera used to convert the point from the world to the screen coordinate system.</param>
        /// <param name="eye">See <see cref="Camera.MonoOrStereoscopicEye"/>.</param>
        /// <param name="localPointOffset">The offset to add to the local point before converting to the world coordinate system.</param>
        /// <param name="worldPointOffset">The offset to add to the world point before converting to the screen coordinate system.</param>
        /// <returns>A point in the screen coordinate system.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> or <paramref name="camera"/> is <see langword="null"/>.</exception>
        public static Vector3 GetScreenPoint(
            RectTransform                rectTransform,
            Vector2                      normalizedRectCoordinates,
            Camera                       camera,
            Camera.MonoOrStereoscopicEye eye,
            Vector2                      localPointOffset,
            Vector3                      worldPointOffset)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (!camera)
            {
                throw new ArgumentNullException(nameof(camera));
            }
            var worldPoint = InternalGetWorldPoint(rectTransform, normalizedRectCoordinates, localPointOffset);
            return camera.WorldToScreenPoint(worldPoint + worldPointOffset, eye);
        }

        /// <summary>
        /// Gets the x component of the position of the pivot of a RectTransform relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The x component of the position of the pivot relative to the anchor reference point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorPosX(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the y component of the position of the pivot of a RectTransform relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The y component of the position of the pivot relative to the anchor reference point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorPosY(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the z component of the local position of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The z component of the local position of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static float GetInspectorPosZ(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            return rectTransform.localPosition.y;
        }

        /// <summary>
        /// Gets the x component of the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The x component of the bottom-left corner offset relative to the bottom-left anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorLeft(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the negation of the x component of the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The negation of the x component of the top-right corner offset relative to the top-right anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorRight(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the y component of the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The x component of the bottom-left corner offset relative to the bottom-left anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorBottom(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the negation of the y component of the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The negation of the x component of the top-right corner offset relative to the top-right anchor.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorTop(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the x component of the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The x component of the size minus the size of the rectangle defined by the anchors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorWidth(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the y component of the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The y component of the size minus the size of the rectangle defined by the anchors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static float GetInspectorHeight(RectTransform rectTransform)
        {
            if (!rectTransform)
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
        /// Gets the layout root transform of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <returns>The layout root transform of the RectTransform.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static RectTransform GetLayoutRoot(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var components = PredefinedPools<Component>.List.Get();
            try
            {
                var layoutRoot = rectTransform;
                for (var parent = layoutRoot.parent as RectTransform; parent; parent = parent.parent as RectTransform)
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

        #region Advanced setters

        /// <summary>
        /// Sets the width of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetWidth(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
        }

        /// <summary>
        /// Sets the height of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetHeight(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
        }

        /// <summary>
        /// Sets the size of a RectTransform.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetSize(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   value.y);
            var parentSize = GetParentSize(rectTransform);
            rectTransform.sizeDelta = value - parentSize * (rectTransform.anchorMax - rectTransform.anchorMin);
        }

        /// <summary>
        /// Sets the bottom-left anchor of a RectTransform, and correspondingly adjusts other properties so that the RectTransform still appears at its original position.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchorMinSmart(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMin == value)
            {
                return;
            }
            var parent = rectTransform.parent as RectTransform;
            if (!parent)
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
        /// Sets the top-right anchor of a RectTransform, and correspondingly adjusts other properties so that the RectTransform still appears at its original position.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetAnchorMaxSmart(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (rectTransform.anchorMax == value)
            {
                return;
            }
            var parent = rectTransform.parent as RectTransform;
            if (!parent)
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
        /// Sets the pivot of a RectTransform, and correspondingly adjusts other properties so that the RectTransform still appears at its original position.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetPivotSmart(RectTransform rectTransform, Vector2 value)
        {
            if (!rectTransform)
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
            rectTransform.anchoredPosition -= (Vector2)cornerOffset;
            var position = rectTransform.position;
            position.z             -= cornerOffset.z;
            rectTransform.position =  position;
        }

        /// <summary>
        /// Sets the x component of the position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorPosX(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the y component of the position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorPosY(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the y component of the position of the pivot relative to the anchor reference point.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        public static void SetInspectorPosZ(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException();
            }
            var localPosition = rectTransform.localPosition;
            localPosition.z             = value;
            rectTransform.localPosition = localPosition;
        }

        /// <summary>
        /// Sets the x component of the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorLeft(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the negation of the x component of the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorRight(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the y component of the bottom-left corner offset relative to the bottom-left anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorBottom(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the negation of the y component of the top-right corner offset relative to the top-right anchor.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorTop(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the x component of the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.x"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.x"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorWidth(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
        /// Sets the y component of the size of a RectTransform minus the size of the rectangle defined by the anchors.
        /// </summary>
        /// <param name="rectTransform">The RectTransform.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rectTransform"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Vector2.y"/> component of <paramref name="rectTransform"/>'s <see cref="RectTransform.anchorMin"/> is not equal to the <see cref="Vector2.y"/> component of its <see cref="RectTransform.anchorMax"/>.</exception>
        public static void SetInspectorHeight(RectTransform rectTransform, float value)
        {
            if (!rectTransform)
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
            var localPoint = UnityMath.NormalizedToPointUnclamped(rect, normalizedRectCoordinates);
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
            return parentRect ? parentRect.rect.size : Vector2.zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldDoIntSnapping(Component component)
        {
            return component.GetComponentInParent<Canvas>() is { renderMode: not RenderMode.WorldSpace };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetReferenceCorner(RectTransform rectTransform)
        {
            return (Vector3)rectTransform.rect.min + rectTransform.localPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsActiveAndEnabledBehaviour(List<Component> components)
        {
            return components.FindIndex(PredicateIsActiveAndEnabledBehaviour) >= 0;
        }

        private static readonly Predicate<Component> PredicateIsActiveAndEnabledBehaviour = IsActiveAndEnabledBehaviour;

        private static bool IsActiveAndEnabledBehaviour(Component component)
        {
            return component is Behaviour { isActiveAndEnabled: true };
        }
    }
}
