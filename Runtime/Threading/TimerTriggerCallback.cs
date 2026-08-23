namespace Aurora.Unity.Threading
{
    /// <summary>
    /// Represents the method executed when a timer triggers.
    /// </summary>
    /// <param name="timer">The timer.</param>
    /// <param name="state">The user-supplied state object; or <see langword="null"/>.</param>
    public delegate void TimerTriggerCallback(ITimer timer, object state);
}
