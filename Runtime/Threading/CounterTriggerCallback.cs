namespace Aurora.Unity.Threading
{
    /// <summary>
    /// Represents the method executed when a counter triggers.
    /// </summary>
    /// <param name="counter">The counter.</param>
    /// <param name="state">The user-supplied state object; or <see langword="null"/>.</param>
    public delegate void CounterTriggerCallback(ICounter counter, object state);
}
