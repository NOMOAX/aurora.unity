namespace Aurora.Unity.UI
{
    /// <summary>
    /// How the duration of <see cref="ScrollView"/> auto-snap is computed.
    /// </summary>
    public enum ScrollViewSnapDurationMode
    {
        /// <summary>
        /// Uses a fixed snap duration.
        /// </summary>
        /// <seealso cref="ScrollView.snapDuration"/>
        Fixed,

        /// <summary>
        /// Computes the snap duration from distance and speed.
        /// </summary>
        /// <seealso cref="ScrollView.snapSpeed"/>
        Dynamic
    }
}
