namespace Aurora.Unity.UI
{
    /// <summary>
    /// The state of an <see cref="EnhancedButton"/>.
    /// </summary>
    public enum EnhancedButtonState
    {
        /// <summary>
        /// The default state.
        /// </summary>
        Default,

        /// <summary>
        /// The pointer is inside.
        /// </summary>
        Hovered,

        /// <summary>
        /// The pointer is pressed and inside.
        /// </summary>
        /// <list type="table">
        /// <listheader>
        /// <term>Pointer action</term>
        /// <description>Transitions to a new state</description>
        /// </listheader>
        /// <item>
        /// <term>Pointer release</term>
        /// <description><see cref="Hovered"/></description>
        /// </item>
        /// <item>
        /// <term>Pointer leaves inside</term>
        /// <description><see cref="Default"/></description>
        /// </item>
        /// <item>
        /// <term>Pointer leaves inside and then re-enters inside</term>
        /// <description><see cref="Hovered"/></description>
        /// </item>
        /// </list>
        Pressed
    }
}
