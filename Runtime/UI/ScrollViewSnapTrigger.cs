using System;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// The trigger conditions for the auto-snap of <see cref="ScrollView"/>.
    /// </summary>
    [Flags]
    public enum ScrollViewSnapTrigger
    {
        /// <summary>
        /// No snap.
        /// </summary>
        None = 0,

        /// <summary>
        /// Auto-snaps when dragging ends.
        /// </summary>
        /// <remarks>Simple, common, and convenient; it auto-snaps as soon as dragging ends</remarks>
        OnEndDrag = 1 << 0,

        /// <summary>
        /// Auto-snaps when not dragging, the normalized scroll position changes, and the velocity is below a threshold.
        /// </summary>
        /// <remarks>Generally used when the user quickly drags to "flick" the content out; the content decelerates slowly due to inertia (with <see cref="ScrollRect.inertia">ScrollRect.inertia</see> set to <see langword="true"/>), and auto-snaps when the velocity drops below the threshold</remarks>
        /// <seealso cref="ScrollView.snapSpeedThreshold"/>
        /// <seealso cref="ScrollView.scrollSnapDelay"/>
        OnNormalizedScrollPositionChanged = 1 << 1,

        /// <summary>
        /// Auto-snaps when not dragging, the pointer is released, and the velocity of <see cref="ScrollRect"/> is very low.
        /// </summary>
        /// <remarks>Generally used together with <see cref="OnNormalizedScrollPositionChanged"/>; on the basis of <see cref="OnNormalizedScrollPositionChanged"/>, the user did not "flick" the content out but held still after a drag and then released, at which point the velocity of <see cref="ScrollRect"/> is very low, so it auto-snaps</remarks>
        OnPointerUpWithLowSpeed = 1 << 2
    }
}
